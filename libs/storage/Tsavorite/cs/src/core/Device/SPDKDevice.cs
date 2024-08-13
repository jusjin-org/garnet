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
        private static int polling_core = 15;
        private const string spdk_library_name = "spdk_device";
        // private const string spdk_library_path =
        //                        "runtimes/linux-x64/native/libspdk_device.so";
        private const string spdk_library_path =
        "/root/source/repos/garnet/libs/storage/Tsavorite/cs/src/core/Device/runtimes/linux-x64/native/libspdk_io_device.so";

        [DllImport(spdk_library_name, EntryPoint = "init",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int init();

        [DllImport(spdk_library_name, EntryPoint = "spdk_io_device_create",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr spdk_io_device_create(uint nsid);

        [DllImport(spdk_library_name, EntryPoint = "spdk_io_device_destroy",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern void spdk_io_device_destroy(IntPtr io_device);

        [DllImport(spdk_library_name, EntryPoint = "spdk_io_device_get_ns_size",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern ulong spdk_io_device_get_ns_size(int nsid);

        [DllImport(spdk_library_name,
                   EntryPoint = "spdk_io_device_get_ns_sector_size",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern uint spdk_io_device_get_ns_sector_size(int nsid);

        [DllImport(spdk_library_name, EntryPoint = "spdk_io_device_read_async",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int spdk_io_device_read_async(IntPtr io_device,
                                                    ulong source,
                                                    IntPtr dest, uint length,
                                                    AsyncIOCallback callback,
                                                    IntPtr context);

        [DllImport(spdk_library_name, EntryPoint = "spdk_io_device_write_async",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int spdk_io_device_write_async(IntPtr io_device,
                                                     IntPtr source,
                                                     ulong dest, uint length,
                                                     AsyncIOCallback callback,
                                                     IntPtr context);

        [DllImport(spdk_library_name, EntryPoint = "spdk_io_device_poll",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int spdk_io_device_poll(IntPtr io_device, uint batch_num);


        [DllImport(spdk_library_name, EntryPoint = "spdk_io_device_begin_poll",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int spdk_io_device_begin_poll(IntPtr io_device,
                                                    int affinity);

        [DllImport(spdk_library_name, EntryPoint = "spdk_io_device_stop_poll",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern void spdk_io_device_stop_poll(IntPtr io_device);

        static int spdk_io_device_num = 32;
        static ConcurrentQueue<IntPtr> spdk_io_device_queue;
        const int nsid = 1;

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
            SPDKDevice.spdk_io_device_queue = new ConcurrentQueue<IntPtr>();
            for (int i = 0; i < SPDKDevice.spdk_io_device_num; i++)
            {
                SPDKDevice.spdk_io_device_queue.Enqueue(spdk_io_device_create(
                                                            SPDKDevice.nsid
                                                        ));
            }
        }
        #endregion

        readonly ILogger logger;
        readonly IntPtr polling_spdk_io_device;

        private int num_pending = 0;

        ObjectPool<SPDKIOContextWrap> spdk_context_pool;

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
            t_callback(
                (uint)error_code,
                (uint)num_bytes,
                t_context
            );
            this.spdk_context_pool.Return(io_context);
            Interlocked.Decrement(ref this.num_pending);
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
                this.start_address = 100L * 1024 * 1024 * 1024; // 100G
            }
            while (!SPDKDevice.spdk_io_device_queue.TryDequeue(
                    out this.polling_spdk_io_device
            )) { }
            int rc = spdk_io_device_begin_poll(this.polling_spdk_io_device,
                                               SPDKDevice.polling_core);
            SPDKDevice.polling_core -= 1;
            if (rc != 0)
            {
                throw new Exception("can't init polling thread.");
            }
            this._callback_delegate = this._callback;

            this.ThrottleLimit = 256;
            DefaultObjectPoolProvider p = new DefaultObjectPoolProvider();
            this.spdk_context_pool = p.Create<SPDKIOContextWrap>(
                new DefaultPooledObjectPolicy<SPDKIOContextWrap>()
            );

            this.SectorSize = spdk_io_device_get_ns_sector_size(
                SPDKDevice.nsid
            );
            this.Capacity = (long)spdk_io_device_get_ns_size(SPDKDevice.nsid);
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


        public IntPtr get_spdk_io_device()
        {
            IntPtr spdk_io_device;
            while (!SPDKDevice.spdk_io_device_queue.TryDequeue(
                       out spdk_io_device
                    ))
            { }
            return spdk_io_device;
        }

        public void put_spdk_io_device(IntPtr spdk_io_device)
        {
            SPDKDevice.spdk_io_device_queue.Enqueue(spdk_io_device);
        }

        public void read_async_with_device(int segment_id,
                                           ulong source_address,
                                           IntPtr destination_address,
                                           uint read_length,
                                           DeviceIOCompletionCallback callback,
                                           object context,
                                           IntPtr spdk_io_device)
        {
            if (Interlocked.Increment(ref this.num_pending) <= 0)
            {
                throw new Exception("Cannot operate on disposed device");
            }

            SPDKIOContextWrap io_context = this.spdk_context_pool.Get();
            try
            {
                io_context.init_io(callback, context);
                int _result = spdk_io_device_read_async(
                    spdk_io_device,
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
                this.spdk_context_pool.Return(io_context);
                callback((uint)(e.HResult & 0x0000FFFF), 0, context);
            }
            catch (Exception e)
            {
                Interlocked.Decrement(ref this.num_pending);
                this.spdk_context_pool.Return(io_context);
                callback((uint)e.HResult, 0, context);
            }
        }

        public override void ReadAsync(int segment_id, ulong source_address,
                                       IntPtr destination_address,
                                       uint read_length,
                                       DeviceIOCompletionCallback callback,
                                       object context)
        {
            this.read_async_with_device(
                segment_id,
                source_address,
                destination_address,
                read_length,
                callback,
                context,
                this.polling_spdk_io_device
            );
        }

        public void write_async_with_device(IntPtr source_address,
                                            int segment_id,
                                            ulong destination_address,
                                            uint write_length,
                                            DeviceIOCompletionCallback callback,
                                            object context,
                                            IntPtr spdk_io_device)
        {
            if (Interlocked.Increment(ref this.num_pending) <= 0)
            {
                throw new Exception("Cannot operate on disposed device");
            }

            SPDKIOContextWrap io_context = this.spdk_context_pool.Get();
            try
            {
                io_context.init_io(callback, context);
                int _result = spdk_io_device_write_async(
                    spdk_io_device,
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
                this.spdk_context_pool.Return(io_context);
                callback((uint)(e.HResult & 0x0000FFFF), 0, context);
            }
            catch (Exception e)
            {
                Interlocked.Decrement(ref this.num_pending);
                this.spdk_context_pool.Return(io_context);
                callback((uint)e.HResult, 0, context);
            }
        }

        public override void WriteAsync(IntPtr source_address, int segment_id,
                                        ulong destination_address,
                                        uint write_length,
                                        DeviceIOCompletionCallback callback,
                                        object context)
        {
            this.write_async_with_device(
                source_address,
                segment_id,
                destination_address,
                write_length, callback,
                context,
                this.polling_spdk_io_device
            );
        }

        public bool TryComplete(IntPtr spdk_io_device)
        {
            int poll_count = 0;
            while (poll_count < 30)
            {
                int complete_io = spdk_io_device_poll(spdk_io_device, 8);
                if (complete_io != 0)
                {
                    return true;
                }
            }

            return false;
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

    public class SPDKIOContextWrap : IDisposable, IResettable
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
