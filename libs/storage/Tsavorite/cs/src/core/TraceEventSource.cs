// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tsavorite.core
{
    [EventSource(Name = "Tsavorite.TraceEvents")]
    public class TraceEventSource : EventSource
    {
        public static TraceEventSource Tracer = new TraceEventSource();

        [Event(1, Message = "{0}", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GenericEvent(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(1, message);
            }
        }

        [Event(10000, Message = "{0}", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnexpectedEvent(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(10000, message);
            }
        }

        //
        // NativeDevice_QueueRun
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NativeDevice_QueueRun_In()
        {
            GenericEvent("NativeDevice_QueueRun_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NativeDevice_QueueRun_Out()
        {
            GenericEvent("NativeDevice_QueueRun_Out");
        }

        //
        // _callback
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void _callback_In()
        {
            GenericEvent("_callback_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void _callback_Out()
        {
            GenericEvent("_callback_Out");
        }

        //
        // NativeStorageDevice
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NativeStorageDevice_In()
        {
            GenericEvent("NativeStorageDevice_In");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NativeStorageDevice_Out()
        {
            GenericEvent("NativeStorageDevice_Out");
        }

        //
        // ReadAsync
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadAsync_In()
        {
            GenericEvent("ReadAsync_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadAsync_Out()
        {
            GenericEvent("ReadAsync_Out");
        }

        //
        // WriteAsync
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAsync_In()
        {
            GenericEvent("WriteAsync_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAsync_Out()
        {
            GenericEvent("WriteAsync_Out");
        }

        //
        // RemoveSegment
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSegment_In()
        {
            GenericEvent("RemoveSegment_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSegment_Out()
        {
            GenericEvent("RemoveSegment_Out");
        }

        //
        // RemoveSegmentAsync_cb
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSegmentAsync_cb_In()
        {
            GenericEvent("RemoveSegmentAsync_cb_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSegmentAsync_cb_Out()
        {
            GenericEvent("RemoveSegmentAsync_cb_Out");
        }

        //
        // Dispose
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose_In()
        {
            GenericEvent("Dispose_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose_Out()
        {
            GenericEvent("Dispose_Out");
        }

        //
        // AsyncFlushPageCallback
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AsyncFlushPageCallback_In()
        {
            GenericEvent("AsyncFlushPageCallback_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AsyncFlushPageCallback_Out()
        {
            GenericEvent("AsyncFlushPageCallback_Out");
        }

        //
        // _work
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void _work_In()
        {
            GenericEvent("_work_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void _work_Out()
        {
            GenericEvent("_work_Out");
        }

        //
        // SerialCommitCallbackWorker
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerialCommitCallbackWorker_In()
        {
            GenericEvent("SerialCommitCallbackWorker_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerialCommitCallbackWorker_Out()
        {
            GenericEvent("SerialCommitCallbackWorker_Out");
        }

        //
        // WriteCommitMetadata
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCommitMetadata_In()
        {
            GenericEvent("WriteCommitMetadata_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCommitMetadata_Out()
        {
            GenericEvent("WriteCommitMetadata_Out");
        }
    }
}
