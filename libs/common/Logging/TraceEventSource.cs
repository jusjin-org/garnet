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

        [Event(20000, Message = "{0}", Level = EventLevel.LogAlways)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnexpectedEvent(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(20000, message);
            }
        }

        //
        // RecvEventArg_Completed
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecvEventArg_Completed_In()
        {
            //GenericEvent("RecvEventArg_Completed_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecvEventArg_Completed_Out()
        {
            //GenericEvent("RecvEventArg_Completed_Out");
        }

        //
        // OnNetworkReceive
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNetworkReceive_In()
        {
            GenericEvent("NET_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNetworkReceive_Out()
        {
            GenericEvent("NET_Out");
        }

        //
        // Process
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process_In()
        {
            //GenericEvent("Process_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process_Out()
        {
            //GenericEvent("Process_Out");
        }

        //
        // TryProcessRequest
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryProcessRequest_In()
        {
            //GenericEvent("TryProcessRequest_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryProcessRequest_Out()
        {
            //GenericEvent("TryProcessRequest_Out");
        }

        //
        // TryConsumeMessages
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryConsumeMessages_In()
        {
            //GenericEvent("TryConsumeMessages_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryConsumeMessages_Out()
        {
            //GenericEvent("TryConsumeMessages_Out");
        }

        //
        // ProcessMessages
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessMessages_In()
        {
            //GenericEvent("ProcessMessages_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessMessages_Out()
        {
            //GenericEvent("ProcessMessages_Out");
        }

        //
        // ProcessBasicCommands
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessBasicCommands_In()
        {
            //GenericEvent("ProcessBasicCommands_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessBasicCommands_Out()
        {
            //GenericEvent("ProcessBasicCommands_Out");
        }

        //
        // NetworkSET
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NetworkSET_In()
        {
            //GenericEvent("NetworkSET_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NetworkSET_Out()
        {
            //GenericEvent("NetworkSET_Out");
        }

        //
        // SendAndReset
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendAndReset_In()
        {
            //GenericEvent("SendAndReset_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendAndReset_Out()
        {
            //GenericEvent("SendAndReset_Out");
        }

        //
        // Send
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send_In()
        {
            //GenericEvent("Send_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send_Out()
        {
            //GenericEvent("Send_Out");
        }

        //
        // WaitForCommitAsync
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForCommitAsync_In()
        {
            GenericEvent("WaitForCommitAsync_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WaitForCommitAsync_Out()
        {
            GenericEvent("WaitForCommitAsync_Out");
        }

        //
        // SendResponse
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResponse_In()
        {
            //GenericEvent("SendResponse_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResponse_Out()
        {
            //GenericEvent("SendResponse_Out");
        }

        //
        // SET
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SET_In()
        {
            //GenericEvent("SET_In");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SET_Out()
        {
            //GenericEvent("SET_Out");
        }
    }
}
