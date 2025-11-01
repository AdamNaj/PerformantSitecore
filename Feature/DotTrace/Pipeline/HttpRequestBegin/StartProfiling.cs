using JetBrains.Profiler.Api;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Pipelines.PreprocessRequest;


namespace PerformantSitecore.Feature.DotTrace.Pipeline.HttpRequestBegin;

public class StartProfiling : PreprocessRequestProcessor
{
    public static int MeasuresLeftCount = 20;
    
    public override void Process(PreprocessRequestArgs args)
    {
        Assert.ArgumentNotNull(args, nameof (args));
        Process(args);
    }

    public void Process(PipelineArgs args)
    {

        if ((MeasureProfiler.GetFeatures() & MeasureFeatures.Ready) != 0 &&
            MeasuresLeftCount >= 0)
        {
            MeasuresLeftCount--;
            if (MeasuresLeftCount >= 0)
                MeasureProfiler.StartCollectingData();
        }

    }
}
