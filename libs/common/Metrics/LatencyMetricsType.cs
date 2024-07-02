// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Garnet.common
{
    /// <summary>
    /// Types of latency metrics exposed by Garnet server
    /// </summary>
    public enum LatencyMetricsType : int
    {
        /// <summary>
        /// Latency of processing, per network receive call (server side) - consider batches with only non-admin requests
        /// Measured from when we start processing the first request in packet, until when we finishing processing the last request in packet (including sending responses).
        /// Only calls that result in at least one request processed, are considered.
        /// </summary>
        NET_RS_LAT = 0,

        /// <summary>
        /// Latency of processing, per network receive call (server side) - consider batches with at least one non-admin request
        /// </summary>
        NET_RS_LAT_ADMIN = 5,

        /// <summary>
        /// Pending request completion latency
        /// </summary>
        PENDING_LAT = 1,
        /// <summary>
        /// Transaction proc latency monitoring
        /// </summary>
        TX_PROC_LAT = 2,
        /// <summary>
        /// Bytes processed, per network receive call (server side)
        /// </summary>
        NET_RS_BYTES = 3,
        /// <summary>
        /// Ops processed, per network receive call (server side)
        /// </summary>
        NET_RS_OPS = 4,

        STORE_IO_SUBMIT = 6,
        STORE_IO = 7,
        STORE_CALLBACK = 8,

        REQ_COMMAND = 9,
        REQ_AOF_ENQUEUE = 10,
        REQ_WAIT_COMMIT = 11,
    }
}