// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garnet.common
{
    [EventSource(Name = "Garnet.TraceEvents")]
    public class TraceEventSource : EventSource
    {
        public static TraceEventSource Tracer = new TraceEventSource();

        [Event(1, Message = "Garnet generic event: {0}", Level = EventLevel.LogAlways)]
        public void GenericEvent(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(1, message);
            }
        }

        [Event(10, Message = "Unexpected Garnet event: {0}", Level = EventLevel.LogAlways)]
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

        [Event(100, Message = "RecvEventArg_Completed_In", Level = EventLevel.LogAlways)]
        public void RecvEventArg_Completed_In()
        {
            if (IsEnabled())
            {
                WriteEvent(100);
            }
        }

        [Event(101, Message = "RecvEventArg_Completed_Out", Level = EventLevel.LogAlways)]
        public void RecvEventArg_Completed_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(101);
            }
        }

        //
        // OnNetworkReceive
        //

        [Event(200, Message = "OnNetworkReceive_In", Level = EventLevel.LogAlways)]
        public void OnNetworkReceive_In()
        {
            if (IsEnabled())
            {
                WriteEvent(200);
            }
        }

        [Event(201, Message = "OnNetworkReceive_Out", Level = EventLevel.LogAlways)]
        public void OnNetworkReceive_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(201);
            }
        }

        //
        // Process
        //

        [Event(300, Message = "Process_In", Level = EventLevel.LogAlways)]
        public void Process_In()
        {
            if (IsEnabled())
            {
                WriteEvent(300);
            }
        }

        [Event(301, Message = "Process_Out", Level = EventLevel.LogAlways)]
        public void Process_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(301);
            }
        }

        //
        // TryProcessRequest
        //

        [Event(400, Message = "TryProcessRequest_In", Level = EventLevel.LogAlways)]
        public void TryProcessRequest_In()
        {
            if (IsEnabled())
            {
                WriteEvent(400);
            }
        }

        [Event(401, Message = "TryProcessRequest_Out", Level = EventLevel.LogAlways)]
        public void TryProcessRequest_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(401);
            }
        }

        //
        // TryConsumeMessages
        //

        [Event(500, Message = "TryConsumeMessages_In", Level = EventLevel.LogAlways)]
        public void TryConsumeMessages_In()
        {
            if (IsEnabled())
            {
                WriteEvent(500);
            }
        }

        [Event(501, Message = "TryConsumeMessages_Out", Level = EventLevel.LogAlways)]
        public void TryConsumeMessages_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(501);
            }
        }

        //
        // ProcessMessages
        //

        [Event(600, Message = "ProcessMessages_In", Level = EventLevel.LogAlways)]
        public void ProcessMessages_In()
        {
            if (IsEnabled())
            {
                WriteEvent(600);
            }
        }

        [Event(601, Message = "ProcessMessages_Out", Level = EventLevel.LogAlways)]
        public void ProcessMessages_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(601);
            }
        }

        //
        // ProcessBasicCommands
        //

        [Event(700, Message = "ProcessBasicCommands_In", Level = EventLevel.LogAlways)]
        public void ProcessBasicCommands_In()
        {
            if (IsEnabled())
            {
                WriteEvent(700);
            }
        }

        [Event(701, Message = "ProcessBasicCommands_Out", Level = EventLevel.LogAlways)]
        public void ProcessBasicCommands_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(701);
            }
        }

        //
        // NetworkSET
        //

        [Event(800, Message = "NetworkSET_In", Level = EventLevel.LogAlways)]
        public void NetworkSET_In()
        {
            if (IsEnabled())
            {
                WriteEvent(800);
            }
        }

        [Event(801, Message = "NetworkSET_Out", Level = EventLevel.LogAlways)]
        public void NetworkSET_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(801);
            }
        }

        //
        // SendAndReset
        //

        [Event(900, Message = "SendAndReset_In", Level = EventLevel.LogAlways)]
        public void SendAndReset_In()
        {
            if (IsEnabled())
            {
                WriteEvent(900);
            }
        }

        [Event(901, Message = "SendAndReset_Out", Level = EventLevel.LogAlways)]
        public void SendAndReset_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(901);
            }
        }

        //
        // Send
        //

        [Event(1000, Message = "Send_In", Level = EventLevel.LogAlways)]
        public void Send_In()
        {
            if (IsEnabled())
            {
                WriteEvent(1000);
            }
        }

        [Event(1001, Message = "Send_Out", Level = EventLevel.LogAlways)]
        public void Send_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(1001);
            }
        }

        //
        // WaitForCommitAsync
        //

        [Event(1100, Message = "WaitForCommitAsync_In", Level = EventLevel.LogAlways)]
        public void WaitForCommitAsync_In()
        {
            if (IsEnabled())
            {
                WriteEvent(1100);
            }
        }

        [Event(1101, Message = "WaitForCommitAsync_Out", Level = EventLevel.LogAlways)]
        public void WaitForCommitAsync_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(1101);
            }
        }

        //
        // SendResponse
        //

        [Event(1200, Message = "SendResponse_In", Level = EventLevel.LogAlways)]
        public void SendResponse_In()
        {
            if (IsEnabled())
            {
                WriteEvent(1200);
            }
        }

        [Event(1201, Message = "SendResponse_Out", Level = EventLevel.LogAlways)]
        public void SendResponse_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(1201);
            }
        }

        //
        // SET
        //

        [Event(1300, Message = "SET_In", Level = EventLevel.LogAlways)]
        public void SET_In()
        {
            if (IsEnabled())
            {
                WriteEvent(1300);
            }
        }

        [Event(1301, Message = "SET_Out", Level = EventLevel.LogAlways)]
        public void SET_Out()
        {
            if (IsEnabled())
            {
                WriteEvent(1301);
            }
        }
    }
}
