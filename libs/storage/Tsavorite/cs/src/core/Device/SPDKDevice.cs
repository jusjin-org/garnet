using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Tsavorite.core
{
    public class SPDKDevice : StorageDeviceBase
    {
        #region native_lib
        private const string spdk_library_name = "spdk_device";
        // private const string spdk_library_path =
        //                        "runtimes/linux-x64/native/libspdk_device.so";
        private const string spdk_library_path =
        "/root/source/repos/garnet/libs/storage/Tsavorite/cs/src/core/Device/runtimes/linux-x64/native/libspdk_device.so";

        [DllImport(spdk_library_name, EntryPoint = "init",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int init();

        [DllImport(spdk_library_name, EntryPoint = "spdk_device_create",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr spdk_device_create(uint nsid);

        [DllImport(spdk_library_name, EntryPoint = "spdk_device_get_ns_size",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern ulong spdk_device_get_ns_size(IntPtr device);

        [DllImport(spdk_library_name,
                   EntryPoint = "spdk_device_get_ns_sector_size",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern uint spdk_device_get_ns_sector_size(IntPtr device);

        [DllImport(spdk_library_name, EntryPoint = "spdk_device_read_async",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int spdk_device_read_async(IntPtr device, ulong source,
                                                 IntPtr dest, uint length,
                                                 AsyncIOCallback callback,
                                                 IntPtr context);

        [DllImport(spdk_library_name, EntryPoint = "spdk_device_write_async",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int spdk_device_write_async(IntPtr device, IntPtr source,
                                                  ulong dest, uint length,
                                                  AsyncIOCallback callback,
                                                  IntPtr context);

        [DllImport(spdk_library_name, EntryPoint = "begin_poller",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern void begin_poller();

        [DllImport(spdk_library_name, EntryPoint = "spdk_device_poll",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int spdk_device_poll(uint timeout);

        static IntPtr import_resolver(string library_name, Assembly assembley,
                                      DllImportSearchPath? search_path)
        {
            if (library_name == spdk_library_name)
            {
                return NativeLibrary.Load(spdk_library_path);
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        static SPDKDevice()
        {
            NativeLibrary.SetDllImportResolver(typeof(SPDKDevice).Assembly,
                                               import_resolver);
            init();
            begin_poller();
        }
        #endregion


        private const int nsid = 1;
        readonly ILogger logger;

        private int num_pending = 0;

        ObjectPool<SPDKIOContextWrap> spdk_context_pool;
        IntPtr native_spdk_device;

        private delegate void AsyncIOCallback(IntPtr context, int result,
                                              ulong bytesTransferred);
        private AsyncIOCallback _callback_delegate;
        private ulong start_address = 0;

        public override uint SectorSize { get; }

        public override long Capacity { get; internal set; }

        void _callback(IntPtr context, int error_code, ulong num_bytes)
        {
            if (error_code < 0)
            {
                error_code = -error_code;
            }
            SPDKIOContextWrap io_context = SPDKIOContextWrap.un_pack(context);
            DeviceIOCompletionCallback t_callback =
                                         io_context.tsavorite_callback;
            object t_context = io_context.tsavorite_callback_context;
            this.spdk_context_pool.Return(io_context);
            Interlocked.Decrement(ref this.num_pending);
            t_callback(
                (uint)error_code,
                (uint)num_bytes,
                t_context
            );
        }

        public SPDKDevice(string filename,
                          bool delete_on_close = true,
                          bool disable_file_buffering = true,
                          long capacity = Devices.CAPACITY_UNSPECIFIED,
                          ILogger logger = null)
                : base(filename, 0, capacity)
        {
            if (filename.ToLower().Contains("hlog"))
            {
                if (filename.ToLower().Contains("objstore"))
                {
                    // ObjStore/hlog
                    this.start_address = 200L * 1024 * 1024 * 1024; // 200G
                }
                else if (filename.ToLower().Contains("hlog.obj"))
                {
                    // ObjStore/hlog.obj
                    this.start_address = 300L * 1024 * 1024 * 1024; // 300G
                }
                else
                {
                    // Store/hlog
                    this.start_address = 100L * 1024 * 1024 * 1024; // 100G
                }

            }
            this._callback_delegate = this._callback;

            this.ThrottleLimit = 256;
            DefaultObjectPoolProvider p = new DefaultObjectPoolProvider();
            this.spdk_context_pool = p.Create<SPDKIOContextWrap>(
                new DefaultPooledObjectPolicy<SPDKIOContextWrap>()
            );

            this.native_spdk_device = spdk_device_create(SPDKDevice.nsid);
            this.SectorSize = spdk_device_get_ns_sector_size(
                this.native_spdk_device
            );
            this.Capacity = (long)spdk_device_get_ns_size(
                this.native_spdk_device
            );
        }

        public override void Initialize(long segmentSize,
                                        LightEpoch epoch = null,
                                        bool omitSegmentIdFromFilename = false)
        {
            this.Capacity = (this.Capacity / segmentSize) * segmentSize;
            base.Initialize(segmentSize, epoch, omitSegmentIdFromFilename);
        }

        public override bool Throttle() => this.num_pending > ThrottleLimit;

        private ulong get_address(int segment_id, ulong offset)
        {
            ulong address = ((ulong)segment_id << this.segmentSizeBits) | offset;
            address += this.start_address;
            return address;
        }

        public override void ReadAsync(int segment_id, ulong source_address,
                                       IntPtr destination_address,
                                       uint read_length,
                                       DeviceIOCompletionCallback callback,
                                       object context)
        {
            if (Interlocked.Increment(ref this.num_pending) <= 0)
            {
                throw new Exception("Cannot operate on disposed device");
            }

            try
            {
                SPDKIOContextWrap io_context = this.spdk_context_pool.Get();
                io_context.init_io(callback, context);
                int _result = spdk_device_read_async(
                    this.native_spdk_device,
                    this.get_address(segment_id, source_address),
                    destination_address,
                    read_length,
                    this._callback_delegate,
                    io_context.spdk_context_ptr
                );
                if (_result != 0)
                {
                    throw new IOException("Error reading from log file",
                                          _result);
                }
            }
            catch (IOException e)
            {
                Interlocked.Decrement(ref this.num_pending);
                callback((uint)(e.HResult & 0x0000FFFF), 0, context);
            }
            catch (Exception e)
            {
                Interlocked.Decrement(ref this.num_pending);
                callback((uint)e.HResult, 0, context);
            }
        }

        public override void WriteAsync(IntPtr source_address, int segment_id,
                                        ulong destination_address,
                                        uint write_length,
                                        DeviceIOCompletionCallback callback,
                                        object context)
        {
            if (Interlocked.Increment(ref this.num_pending) <= 0)
            {
                throw new Exception("Cannot operate on disposed device");
            }

            try
            {
                SPDKIOContextWrap io_context = this.spdk_context_pool.Get();
                io_context.init_io(callback, context);
                int _result = spdk_device_write_async(
                    this.native_spdk_device,
                    source_address,
                    this.get_address(segment_id, destination_address),
                    write_length,
                    this._callback_delegate,
                    io_context.spdk_context_ptr
                );
                if (_result != 0)
                {
                    throw new IOException("Error writing to log file",
                                          _result);
                }
            }
            catch (IOException e)
            {
                Interlocked.Decrement(ref this.num_pending);
                callback((uint)(e.HResult & 0x0000FFFF), 0, context);
            }
            catch (Exception e)
            {
                Interlocked.Decrement(ref this.num_pending);
                callback((uint)e.HResult, 0, context);
            }
        }

        public override void RemoveSegment(int segment)
        {
            // TODO: chyin implement RemoveSegment
            return;
        }

        public override void RemoveSegmentAsync(int segment,
                                                AsyncCallback callback,
                                                IAsyncResult result)
        {
            callback(result);
            return;
        }

        // TODO: chyin add Rest()
        // TODO: chyin add TryComplete()
        // TODO: chyin add GetFileSize()

        public override void Dispose()
        {
            while (this.num_pending >= 0)
            {
                Interlocked.CompareExchange(ref this.num_pending, int.MinValue,
                                            0);
                Thread.Yield();
            }

            // TODO: chyin join poller thread.

            // TODO: chyin call device destroy function.

        }
    }

    class SPDKIOContextWrap : IDisposable, IResettable
    {
        class SPDKIOContext
        {
            public DeviceIOCompletionCallback tsavorite_callback;
            public object tsavorite_callback_context;
            public SPDKIOContextWrap wrap;

            public SPDKIOContext(SPDKIOContextWrap wrap)
            {
                this.wrap = wrap;
            }
        }

        private readonly GCHandle handle;
        private readonly SPDKIOContext spdk_context;
        public IntPtr spdk_context_ptr { get; private set; }

        public DeviceIOCompletionCallback tsavorite_callback
        {
            get => this.spdk_context.tsavorite_callback;
        }

        public object tsavorite_callback_context
        {
            get => this.spdk_context.tsavorite_callback_context;
        }

        public SPDKIOContextWrap()
        {
            this.spdk_context = new SPDKIOContext(this);
            this.handle = GCHandle.Alloc(this.spdk_context,
                                         GCHandleType.Normal);
            this.spdk_context_ptr = GCHandle.ToIntPtr(this.handle);
        }

        public static SPDKIOContextWrap un_pack(IntPtr ptr)
        {
            GCHandle handle = GCHandle.FromIntPtr(ptr);
            SPDKIOContext io_context = handle.Target as SPDKIOContext;
            return io_context.wrap;
        }

        public void init_io(DeviceIOCompletionCallback tsavorite_callback,
                            object tsavorite_callback_context)
        {
            this.spdk_context.tsavorite_callback = tsavorite_callback;
            this.spdk_context.tsavorite_callback_context =
                                                     tsavorite_callback_context;
        }

        public bool TryReset()
        {
            this.spdk_context.tsavorite_callback = null;
            this.spdk_context.tsavorite_callback_context = null;
            return true;
        }

        public void Dispose()
        {
            this.handle.Free();
        }
    }
}
