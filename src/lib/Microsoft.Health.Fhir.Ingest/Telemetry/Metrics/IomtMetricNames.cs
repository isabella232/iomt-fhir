﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public enum IomtMetricNames
    {
        /// <summary>
        /// The number of input events received.
        /// </summary>
        DeviceEvent,

        /// <summary>
        /// The latency between the event ingestion time and normalization processing. An increase here indicates a backlog of messages to process.
        /// </summary>
        DeviceEventProcessingLatency,

        /// <summary>
        /// The latency between the event ingestion time and normalization processing, in milliseconds. An increase here indicates a backlog of messages to process.
        /// </summary>
        DeviceEventProcessingLatencyMs,

        /// <summary>
        /// A metric that measures the amount of data (in bytes) ingested by normalization processing.
        /// </summary>
        DeviceIngressSizeBytes,

        /// <summary>
        /// The number of normalized events generated for further processing.
        /// </summary>
        NormalizedEvent,

        /// <summary>
        /// The number of measurement readings to import to FHIR.
        /// </summary>
        Measurement,

        /// <summary>
        /// The number of measurement groups generated by the FHIR processor based on provided input.
        /// </summary>
        MeasurementGroup,

        /// <summary>
        /// The latency between event ingestion and output to FHIR processor.
        /// </summary>
        MeasurementIngestionLatency,

        /// <summary>
        /// The latency between event ingestion and output to FHIR processor, in milliseconds.
        /// </summary>
        MeasurementIngestionLatencyMs,
    }
}
