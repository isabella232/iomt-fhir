// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using EnsureThat;
using Microsoft.Azure.WebJobs;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Logging.Telemetry;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Ingest.Data
{
    public class BatchingMeasurementToEventMessageAsyncCollector : IAsyncCollector<IMeasurement>
    {
        private readonly IEventHubMessageService _eventHubService;
        private readonly ITelemetryLogger _log;
        private readonly ConcurrentQueue<IMeasurement> _batchedMeasurements;
        private readonly int _batchSize;

        public BatchingMeasurementToEventMessageAsyncCollector(
            IEventHubMessageService eventHubService,
            ITelemetryLogger log,
            int batchSize = 500)
        {
            _eventHubService = EnsureArg.IsNotNull(eventHubService, nameof(eventHubService));
            _log = EnsureArg.IsNotNull(log, nameof(log));
            _batchedMeasurements = new ConcurrentQueue<IMeasurement>();
            _batchSize = batchSize;
        }

        public async Task AddAsync(IMeasurement item, CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureArg.IsNotNull(item, nameof(item));
            Ensure.String.IsNotNullOrWhiteSpace(item.DeviceId, nameof(item.DeviceId));

            _batchedMeasurements.Enqueue(item);

            if (_batchedMeasurements.Count >= _batchSize)
            {
                await DrainQueue(cancellationToken);
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await DrainQueue(cancellationToken);
        }

        private async Task DrainQueue(CancellationToken cancellationToken)
        {
            var measurements = new List<IMeasurement>();

            while (measurements.Count <= _batchSize && _batchedMeasurements.TryDequeue(out var measurement))
            {
                measurements.Add(measurement);
            }

            var submissionTasks = measurements
                .GroupBy(m => m.DeviceId)
                .Select(async grp =>
                {
                    var partitionKey = EnsureArg.IsNotNullOrWhiteSpace(grp.Key, "DeviceId");
                    var eventList = new List<EventData>();
                    foreach (var m in grp)
                    {
                        var measurementContent = JsonConvert.SerializeObject(m, Formatting.None);
                        var contentBytes = Encoding.UTF8.GetBytes(measurementContent);
                        var eventData = new EventData(contentBytes);
                        eventList.Add(eventData);
                    }

                    _log.LogTrace($"Submitting {measurements.Count()} batched events for partition {partitionKey}");
                    await _eventHubService.SendAsync(eventList, partitionKey, cancellationToken).ConfigureAwait(false);
                });
            await Task.WhenAll(submissionTasks);
        }
    }
}
