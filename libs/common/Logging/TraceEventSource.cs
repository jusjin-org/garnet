// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Garnet.common
{
    [EventSource(Name = "Garnet.TraceEvents")]
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
        // RecvEventArg_Completed
        //

        //[Event(100, Message = "RecvEventArg_Completed_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecvEventArg_Completed_In()
        {
            GenericEvent("RecvEventArg_Completed_In");
        }

        //[Event(101, Message = "RecvEventArg_Completed_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecvEventArg_Completed_Out()
        {
            GenericEvent("RecvEventArg_Completed_Out");
        }

        //
        // OnNetworkReceive
        //

        //[Event(200, Message = "OnNetworkReceive_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNetworkReceive_In()
        {
            GenericEvent("OnNetworkReceive_In");
        }

        //[Event(201, Message = "OnNetworkReceive_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNetworkReceive_Out()
        {
            GenericEvent("OnNetworkReceive_Out");
        }

        //
        // Process
        //

        //[Event(300, Message = "Process_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process_In()
        {
            GenericEvent("Process_In");
        }

        //[Event(301, Message = "Process_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process_Out()
        {
            GenericEvent("Process_Out");
        }

        //
        // TryProcessRequest
        //

        //[Event(400, Message = "TryProcessRequest_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryProcessRequest_In()
        {
            GenericEvent("TryProcessRequest_In");
        }

        //[Event(401, Message = "TryProcessRequest_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryProcessRequest_Out()
        {
            GenericEvent("TryProcessRequest_Out");
        }

        //
        // TryConsumeMessages
        //

        //[Event(500, Message = "TryConsumeMessages_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryConsumeMessages_In()
        {
            GenericEvent("TryConsumeMessages_In");
        }

        //[Event(501, Message = "TryConsumeMessages_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryConsumeMessages_Out()
        {
            GenericEvent("TryConsumeMessages_Out");
        }

        //
        // ProcessMessages
        //

        //[Event(600, Message = "ProcessMessages_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessMessages_In()
        {
            GenericEvent("ProcessMessages_In");
        }

        //[Event(601, Message = "ProcessMessages_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessMessages_Out()
        {
            GenericEvent("ProcessMessages_Out");
        }

        //
        // ProcessBasicCommands
        //

        //[Event(700, Message = "ProcessBasicCommands_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessBasicCommands_In()
        {
            GenericEvent("ProcessBasicCommands_In");
        }

        //[Event(701, Message = "ProcessBasicCommands_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessBasicCommands_Out()
        {
            GenericEvent("ProcessBasicCommands_Out");
        }

        //
        // NetworkSET
        //

        //[Event(800, Message = "NetworkSET_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NetworkSET_In()
        {
            GenericEvent("NetworkSET_In");
        }

        //[Event(801, Message = "NetworkSET_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NetworkSET_Out()
        {
            GenericEvent("NetworkSET_Out");
        }

        //
        // SendAndReset
        //

        //[Event(900, Message = "SendAndReset_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendAndReset_In()
        {
            GenericEvent("SendAndReset_In");
        }

        //[Event(901, Message = "SendAndReset_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendAndReset_Out()
        {
            GenericEvent("SendAndReset_Out");
        }

        //
        // Send
        //

        //[Event(1000, Message = "Send_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send_In()
        {
            GenericEvent("Send_In");
        }

        //[Event(1001, Message = "Send_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send_Out()
        {
            GenericEvent("Send_Out");
        }

        //
        // WaitForCommitAsync
        //

        //[Event(1100, Message = "WaitForCommitAsync_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForCommitAsync_In()
        {
            GenericEvent("WaitForCommitAsync_In");
        }

        //[Event(1101, Message = "WaitForCommitAsync_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForCommitAsync_Out()
        {
            GenericEvent("WaitForCommitAsync_Out");
        }

        //
        // SendResponse
        //

        //[Event(1200, Message = "SendResponse_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResponse_In()
        {
            GenericEvent("SendResponse_In");
        }

        //[Event(1201, Message = "SendResponse_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResponse_Out()
        {
            GenericEvent("SendResponse_Out");
        }

        //
        // SET
        //

        //[Event(1300, Message = "SET_In", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SET_In()
        {
            GenericEvent("SET_In");
        }

        //[Event(1301, Message = "SET_Out", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SET_Out()
        {
            GenericEvent("SET_Out");
        }
    }
}
