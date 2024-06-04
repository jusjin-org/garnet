// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsavorite.core
{
    [EventSource(Name = "Tsavorite.TraceEvents")]
    public class TraceEventSource : EventSource
    {
        public static TraceEventSource Tracer = new TraceEventSource();

        [Event(1, Message = "Tsavorite generic event: {0}", Level = EventLevel.LogAlways)]
        public void GenericEvent(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(1, message);
            }
        }

        [Event(10, Message = "Unexpected Tsavorite event: {0}", Level = EventLevel.LogAlways)]
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

        [Event(10000, Message = "NativeDevice_QueueRun_In", Level = EventLevel.LogAlways)]
        public void NativeDevice_QueueRun_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10000);
            }
        }

        [Event(10001, Message = "NativeDevice_QueueRun_Out", Level = EventLevel.LogAlways)]
        public void NativeDevice_QueueRun_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10001);
            }
        }

        //
        // _callback
        //

        [Event(10100, Message = "_callback_In", Level = EventLevel.LogAlways)]
        public void _callback_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10100);
            }
        }

        [Event(10101, Message = "_callback_Out", Level = EventLevel.LogAlways)]
        public void _callback_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10101);
            }
        }

        //
        // NativeStorageDevice
        //

        [Event(10200, Message = "NativeStorageDevice_In", Level = EventLevel.LogAlways)]
        public void NativeStorageDevice_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10200);
            }
        }

        [Event(10201, Message = "NativeStorageDevice_Out", Level = EventLevel.LogAlways)]
        public void NativeStorageDevice_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10201);
            }
        }

        //
        // ReadAsync
        //

        [Event(10300, Message = "ReadAsync_In", Level = EventLevel.LogAlways)]
        public void ReadAsync_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10300);
            }
        }

        [Event(10301, Message = "ReadAsync_Out", Level = EventLevel.LogAlways)]
        public void ReadAsync_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10301);
            }
        }

        //
        // WriteAsync
        //

        [Event(10400, Message = "WriteAsync_In", Level = EventLevel.LogAlways)]
        public void WriteAsync_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10400);
            }
        }

        [Event(10401, Message = "WriteAsync_Out", Level = EventLevel.LogAlways)]
        public void WriteAsync_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10401);
            }
        }

        //
        // RemoveSegment
        //

        [Event(10500, Message = "RemoveSegment_In", Level = EventLevel.LogAlways)]
        public void RemoveSegment_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10500);
            }
        }

        [Event(10501, Message = "RemoveSegment_Out", Level = EventLevel.LogAlways)]
        public void RemoveSegment_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10501);
            }
        }

        //
        // RemoveSegmentAsync_cb
        //

        [Event(10600, Message = "RemoveSegmentAsync_cb_In", Level = EventLevel.LogAlways)]
        public void RemoveSegmentAsync_cb_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10600);
            }
        }

        [Event(10601, Message = "RemoveSegmentAsync_cb_Out", Level = EventLevel.LogAlways)]
        public void RemoveSegmentAsync_cb_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10601);
            }
        }

        //
        // Dispose
        //

        [Event(10700, Message = "Dispose_In", Level = EventLevel.LogAlways)]
        public void Dispose_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10700);
            }
        }

        [Event(10701, Message = "Dispose_Out", Level = EventLevel.LogAlways)]
        public void Dispose_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10701);
            }
        }

        //
        // AsyncFlushPageCallback
        //

        [Event(10800, Message = "AsyncFlushPageCallback_In", Level = EventLevel.LogAlways)]
        public void AsyncFlushPageCallback_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10800);
            }
        }

        [Event(10801, Message = "AsyncFlushPageCallback_Out", Level = EventLevel.LogAlways)]
        public void AsyncFlushPageCallback_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10801);
            }
        }

        //
        // _work
        //

        [Event(10900, Message = "_work_In", Level = EventLevel.LogAlways)]
        public void _work_In()
        {
            if (IsEnabled())
            {
                WriteEvent(10900);
            }
        }

        [Event(10901, Message = "_work_Out", Level = EventLevel.LogAlways)]
        public void _work_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(10901);
            }
        }

        //
        // SerialCommitCallbackWorker
        //
        [Event(11000, Message = "SerialCommitCallbackWorker_In", Level = EventLevel.LogAlways)]
        public void SerialCommitCallbackWorker_In()
        {
            if (IsEnabled())
            {
                WriteEvent(11000);
            }
        }

        [Event(11001, Message = "SerialCommitCallbackWorker_Out", Level = EventLevel.LogAlways)]
        public void SerialCommitCallbackWorker_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(11001);
            }
        }

        //
        // WriteCommitMetadata
        //

        [Event(11100, Message = "WriteCommitMetadata_In", Level = EventLevel.LogAlways)]
        public void WriteCommitMetadata_In()
        {
            if (IsEnabled())
            {
                WriteEvent(11100);
            }
        }

        [Event(11101, Message = "WriteCommitMetadata_Out", Level = EventLevel.LogAlways)]
        public void WriteCommitMetadata_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(11101);
            }
        }
    }
}
