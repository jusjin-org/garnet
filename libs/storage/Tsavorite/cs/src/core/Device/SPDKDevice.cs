﻿using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using HdrHistogram;
using System.Collections.Generic;

namespace Tsavorite.core
{
    public class SPDKDevice : StorageDeviceBase
    {
        private const int nsid = 1;
        private const uint sector_size = 4096;
        readonly ILogger logger;

        private int num_pending = 0;

        private long elapsed_ticks = 0;
        private uint io_num = 0;
        private ulong io_size = 0;
        private long nanosec_per_tick =
                       (1000L * 1000L * 1000L) / Stopwatch.Frequency;

        private const string spdk_library_name = "spdk_device";
        private const string spdk_library_path =
                               "runtimes/linux-x64/native/libspdk_device.so";
        public delegate void AsyncIOCallback(IntPtr context, int result,
                                             ulong bytesTransferred);
        private ConcurrentQueue<IntPtr> spdk_device_queue =
                                          new ConcurrentQueue<IntPtr>();
        private AsyncIOCallback _callback_delegate;

        private LongHistogram[] io_submit_metric = { new(1, TimeStamp.Seconds(100), 2), new(1, TimeStamp.Seconds(100), 2) };
        private LongHistogram[] io_metric = { new(1, TimeStamp.Seconds(100), 2), new(1, TimeStamp.Seconds(100), 2) };
        private LongHistogram[] callback_metric = { new(1, TimeStamp.Seconds(100), 2), new(1, TimeStamp.Seconds(100), 2) };
        private int metrics_version = 0;

        class ManagedCallback
        {
            public readonly IntPtr spdk_device;
            private DeviceIOCompletionCallback callback;
            private object context;
            public readonly Stopwatch stop_watch;

            public ManagedCallback(IntPtr spdk_device,
                                   DeviceIOCompletionCallback callback,
                                   object context)
            {
                this.spdk_device = spdk_device;
                this.callback = callback;
                this.context = context;
                this.stop_watch = new Stopwatch();
                this.stop_watch.Start();
            }

            public void call(uint error_code, uint num_bytes)
            {
                this.callback(error_code, num_bytes, context);
            }
        }

        void _callback(IntPtr context, int error_code, ulong num_bytes)
        {
            Interlocked.Decrement(ref this.num_pending);
            if (error_code < 0)
            {
                error_code = -error_code;
            }
            GCHandle handle = GCHandle.FromIntPtr(context);
            ManagedCallback managed_callback =
                                (handle.Target as ManagedCallback);
            managed_callback.stop_watch.Stop();
            Interlocked.Add(ref this.io_num, 1);
            Interlocked.Add(
                ref this.elapsed_ticks,
                managed_callback.stop_watch.ElapsedTicks
            );
            Interlocked.Add(ref this.io_size, num_bytes);

            this.spdk_device_queue.Enqueue(managed_callback.spdk_device);
            managed_callback.call((uint)error_code, (uint)num_bytes);
            handle.Free();
        }

        public SPDKDevice(string filename, uint sectorSize, long capacity)
                 : base(filename, sectorSize, capacity)
        {
            NativeLibrary.SetDllImportResolver(typeof(SPDKDevice).Assembly,
                                               import_resolver);
        }

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

        [DllImport(spdk_library_name, EntryPoint = "spdk_device_init",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern int spdk_device_init();

        [DllImport(spdk_library_name, EntryPoint = "spdk_device_create",
                   CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr spdk_device_create(int nsid);

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

        public SPDKDevice(string filename,
                          bool delete_on_close = true,
                          bool disable_file_buffering = true,
                          long capacity = Devices.CAPACITY_UNSPECIFIED,
                          ILogger logger = null)
                : this(filename, sector_size, capacity)
        {
            this._callback_delegate = this._callback;
            this.ThrottleLimit = 1024;

            spdk_device_init();

            for (int i = 0; i < 2; i++)
            {
                this.spdk_device_queue.Enqueue(
                    spdk_device_create(SPDKDevice.nsid)
                );
            }

            begin_poller();
        }

        public LongHistogram get_io_submit_metric()
        {
            return this.io_submit_metric[this.metrics_version];
        }

        public LongHistogram get_io_metric()
        {
            return this.io_metric[this.metrics_version];
        }

        public LongHistogram get_callback_metric()
        {
            return this.callback_metric[this.metrics_version];
        }

        public void reset_metrics()
        {
            this.io_submit_metric[this.metrics_version].Reset();
            this.io_metric[this.metrics_version].Reset();
            this.callback_metric[this.metrics_version].Reset();
            this.metrics_version = (this.metrics_version + 1) % 2;
        }

        public override bool Throttle() => this.num_pending > ThrottleLimit;

        private ulong get_address(int segment_id, ulong offset)
        {
            return ((ulong)segment_id << this.segmentSizeBits) | offset;
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
                IntPtr spdk_device;
                while (!this.spdk_device_queue.TryDequeue(out spdk_device))
                {
                    Debug.WriteLine("Can't get spdk_device when reading");
                }
                GCHandle handle = GCHandle.Alloc(new ManagedCallback(spdk_device,
                                                                     callback,
                                                                     context),
                                                GCHandleType.Normal);
                int _result = spdk_device_read_async(
                    spdk_device,
                    this.get_address(segment_id, source_address),
                    destination_address,
                    read_length,
                    this._callback_delegate,
                    GCHandle.ToIntPtr(handle)
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
                IntPtr spdk_device;
                while (!this.spdk_device_queue.TryDequeue(out spdk_device))
                {
                    Debug.WriteLine("Can't get spdk_device when writing");
                }
                GCHandle handle = GCHandle.Alloc(
                    new ManagedCallback(spdk_device, callback, context),
                    GCHandleType.Normal
                );
                int _result = spdk_device_write_async(
                    spdk_device,
                    source_address,
                    this.get_address(segment_id, destination_address),
                    write_length,
                    this._callback_delegate,
                    GCHandle.ToIntPtr(handle)
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
}
