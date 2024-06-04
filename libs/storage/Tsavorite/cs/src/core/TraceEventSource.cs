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

        [Event(10, Message = "{0}", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnexpectedEvent(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(10, message);
            }
        }

        //
        // NativeDevice_QueueRun
        //

        //[Event(10000, Message = "NativeDevice_QueueRun_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NativeDevice_QueueRun_In()
        {
            GenericEvent("NativeDevice_QueueRun_In");
        }

        //[Event(10001, Message = "NativeDevice_QueueRun_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NativeDevice_QueueRun_Out()
        {
            GenericEvent("NativeDevice_QueueRun_Out");
        }

        //
        // _callback
        //

        //[Event(10100, Message = "_callback_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void _callback_In()
        {
            GenericEvent("_callback_In");
        }

        //[Event(10101, Message = "_callback_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void _callback_Out()
        {
            GenericEvent("_callback_Out");
        }

        //
        // NativeStorageDevice
        //

        //[Event(10200, Message = "NativeStorageDevice_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NativeStorageDevice_In()
        {
            GenericEvent("NativeStorageDevice_In");
        }

        //[Event(10201, Message = "NativeStorageDevice_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NativeStorageDevice_Out()
        {
            GenericEvent("NativeStorageDevice_Out");
        }

        //
        // ReadAsync
        //

        //[Event(10300, Message = "ReadAsync_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadAsync_In()
        {
            GenericEvent("ReadAsync_In");
        }

        //[Event(10301, Message = "ReadAsync_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadAsync_Out()
        {
            GenericEvent("ReadAsync_Out");
        }

        //
        // WriteAsync
        //

        //[Event(10400, Message = "WriteAsync_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAsync_In()
        {
            GenericEvent("WriteAsync_In");
        }

        //[Event(10401, Message = "WriteAsync_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteAsync_Out()
        {
            GenericEvent("WriteAsync_Out");
        }

        //
        // RemoveSegment
        //

        //[Event(10500, Message = "RemoveSegment_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSegment_In()
        {
            GenericEvent("RemoveSegment_In");
        }

        //[Event(10501, Message = "RemoveSegment_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSegment_Out()
        {
            GenericEvent("RemoveSegment_Out");
        }

        //
        // RemoveSegmentAsync_cb
        //

        //[Event(10600, Message = "RemoveSegmentAsync_cb_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSegmentAsync_cb_In()
        {
            GenericEvent("RemoveSegmentAsync_cb_In");
        }

        //[Event(10601, Message = "RemoveSegmentAsync_cb_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSegmentAsync_cb_Out()
        {
            GenericEvent("RemoveSegmentAsync_cb_Out");
        }

        //
        // Dispose
        //

        //[Event(10700, Message = "Dispose_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose_In()
        {
            GenericEvent("Dispose_In");
        }

        //[Event(10701, Message = "Dispose_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose_Out()
        {
            GenericEvent("Dispose_Out");
        }

        //
        // AsyncFlushPageCallback
        //

        //[Event(10800, Message = "AsyncFlushPageCallback_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AsyncFlushPageCallback_In()
        {
            GenericEvent("AsyncFlushPageCallback_In");
        }

        //[Event(10801, Message = "AsyncFlushPageCallback_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AsyncFlushPageCallback_Out()
        {
            GenericEvent("AsyncFlushPageCallback_Out");
        }

        //
        // _work
        //

        //[Event(10900, Message = "_work_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void _work_In()
        {
            GenericEvent("_work_In");
        }

        //[Event(10901, Message = "_work_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void _work_Out()
        {
            GenericEvent("_work_Out");
        }

        //
        // SerialCommitCallbackWorker
        //
        //[Event(11000, Message = "SerialCommitCallbackWorker_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerialCommitCallbackWorker_In()
        {
            GenericEvent("SerialCommitCallbackWorker_In");
        }

        //[Event(11001, Message = "SerialCommitCallbackWorker_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerialCommitCallbackWorker_Out()
        {
            GenericEvent("SerialCommitCallbackWorker_Out");
        }

        //
        // WriteCommitMetadata
        //

        //[Event(11100, Message = "WriteCommitMetadata_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCommitMetadata_In()
        {
            GenericEvent("WriteCommitMetadata_In");
        }

        //[Event(11101, Message = "WriteCommitMetadata_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCommitMetadata_Out()
        {
            GenericEvent("WriteCommitMetadata_Out");
        }
    }
}
