using EnsureThat;
using Microsoft.Health.Common.Telemetry;
using System;
using System.Collections.Generic;

namespace Microsoft.Health.Logging.Telemetry.Exceptions
{
    public class ExceptionTelemetryProcessor
    {
        private readonly HashSet<Type> _handledExceptions;

        public ExceptionTelemetryProcessor(params Type[] handledExceptionTypes)
        {
            _handledExceptions = new HashSet<Type>(handledExceptionTypes);
        }

        public virtual bool HandleException(Exception ex, ITelemetryLogger logger, Metric handledExceptionMetric = null, Metric unhandledExceptionMetric = null)
        {
            EnsureArg.IsNotNull(ex, nameof(ex));
            EnsureArg.IsNotNull(logger, nameof(logger));

            var exType = ex.GetType();
            var lookupType = exType.IsGenericType ? exType.GetGenericTypeDefinition() : exType;

            if (_handledExceptions.Contains(lookupType))
            {
                LogExceptionWithMetric(ex, logger, handledExceptionMetric);
                return true;
            }

            LogExceptionWithMetric(ex, logger, unhandledExceptionMetric);
            return false;
        }

        public static void LogExceptionWithMetric(Exception ex, ITelemetryLogger logger, Metric exceptionMetric = null)
        {
            logger.LogError(ex);

            Metric metric = ex is ITelemetryFormattable tel ? tel.ToMetric : exceptionMetric;
            if (metric != null)
            {
                logger.LogMetric(metric, metricValue: 1);
            }
        }
    }
}
