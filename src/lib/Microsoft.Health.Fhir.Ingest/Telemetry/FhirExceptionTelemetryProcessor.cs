// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Extensions.Fhir;
using Microsoft.Health.Fhir.Ingest.Data;
using Microsoft.Health.Fhir.Ingest.Service;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Health.Logging.Telemetry.Exceptions;

namespace Microsoft.Health.Fhir.Ingest.Telemetry
{
    public class FhirExceptionTelemetryProcessor : ExceptionTelemetryProcessor
    {
        private readonly string _connectorStage = ConnectorOperation.FHIRConversion;

        public FhirExceptionTelemetryProcessor()
            : base (
                typeof(PatientDeviceMismatchException),
                typeof(ResourceIdentityNotDefinedException),
                typeof(NotSupportedException),
                typeof(FhirResourceNotFoundException),
                typeof(MultipleResourceFoundException<>),
                typeof(TemplateNotFoundException),
                typeof(CorrelationIdNotDefinedException))
        {
        }

        public bool HandleException(Exception ex, ITelemetryLogger logger)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(logger, nameof(logger));

            if (ex is NotSupportedException)
            {
                LogExceptionWithMetric(ex, logger, IomtMetrics.NotSupported());
                return true;
            }

            return HandleException(ex, logger, IomtMetrics.HandledException(ex.GetType().Name, _connectorStage), IomtMetrics.UnhandledException(ex.GetType().Name, _connectorStage));
        }
    }
}
