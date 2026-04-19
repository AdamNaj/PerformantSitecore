using Sitecore.Pipelines;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Helpers
{
    public interface IPipelinesHelper
    {
        void RunPipeline(string pipelineName, PipelineArgs pipelineArgs, bool disableSecurity = true);
    }
}
