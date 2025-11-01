using JetBrains.Profiler.Api;
using PerformantSitecore.Feature.DotTrace.Pipeline.HttpRequestBegin;
using Sitecore.Pipelines.HttpRequest;

namespace PerformantSitecore.Feature.DotTrace.Pipeline.HttpRequestEnd;

public class StopProfiling : HttpRequestProcessor
{
    public override void Process(HttpRequestArgs args)
    {
        // Stop the profiler if it is running
        if (MeasureProfiler.GetFeatures().HasFlag(MeasureFeatures.Ready))
        {
            if(StartProfiling.MeasuresLeftCount >= 0)
                MeasureProfiler.StopCollectingData();
            if(StartProfiling.MeasuresLeftCount == 0)
                MeasureProfiler.SaveData();
        }
    }
}
