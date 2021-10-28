using System;

namespace Microsoft.Health.Tools.EventDebugger
{
    public class EventProcessorOptions
    {
        public static string Category = "EventProcessor";
        
        public TimeSpan EventReadTimeout {get; set;} = TimeSpan.FromSeconds(15);
    }
}