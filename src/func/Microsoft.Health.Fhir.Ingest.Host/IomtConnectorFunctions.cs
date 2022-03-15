// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using DevLab.JmesPath;
using EnsureThat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Health.Common.Telemetry;
using Microsoft.Health.Expressions;
using Microsoft.Health.Fhir.Ingest.Host;
using Microsoft.Health.Fhir.Ingest.Telemetry;
using Microsoft.Health.Fhir.Ingest.Template;
using Microsoft.Health.Logging.Telemetry;

namespace Microsoft.Health.Fhir.Ingest.Service
{
    public class IomtConnectorFunctions
    {
        private readonly ITelemetryLogger _logger;
        private readonly CollectionContentTemplateFactory _collectionContentTemplateFactory;
        private readonly IExceptionTelemetryProcessor _exceptionTelemetryProcessor;

        public IomtConnectorFunctions(ITelemetryLogger logger)
        {
            _logger = EnsureArg.IsNotNull(logger, nameof(logger));
            var expressionRegister = new AssemblyExpressionRegister(typeof(IExpressionRegister).Assembly, _logger);
            var jmesPath = new JmesPath();
            expressionRegister.RegisterExpressions(jmesPath.FunctionRepository);
            _collectionContentTemplateFactory = new CollectionContentTemplateFactory(
                new JsonPathContentTemplateFactory(),
                new IotJsonPathContentTemplateFactory(),
                new IotCentralJsonPathContentTemplateFactory(),
                new CalculatedFunctionContentTemplateFactory(
                    new TemplateExpressionEvaluatorFactory(jmesPath), _logger));
            _exceptionTelemetryProcessor = new NormalizationExceptionTelemetryProcessor();
        }

        [FunctionName("MeasurementCollectionToFhir")]
        public async Task<IActionResult> MeasurementCollectionToFhir(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("template/%Template:FhirMapping%", FileAccess.Read)] string templateDefinition,
            [MeasurementFhirImport] MeasurementFhirImportService measurementImportService)
        {
            EnsureArg.IsNotNull(measurementImportService, nameof(measurementImportService));
            EnsureArg.IsNotNull(req, nameof(req));

            try
            {
                await measurementImportService.ProcessStreamAsync(req.Body, templateDefinition, _logger).ConfigureAwait(false);
                return new AcceptedResult();
            }
            catch (Exception ex)
            {
                _logger.LogMetric(
                    IomtMetrics.UnhandledException(ex.GetType().Name, ConnectorOperation.FHIRConversion),
                    1);
                throw;
            }
        }
    }
}