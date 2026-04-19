using Sitecore.Pipelines;
using Sitecore.SecurityModel;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Helpers
{
    public class PipelinesHelper : IPipelinesHelper
    {
        public void RunPipeline(string pipelineName, PipelineArgs pipelineArgs, bool disableSecurity = true)
        {
            if (!disableSecurity)
            {
                CorePipeline.Run(pipelineName, pipelineArgs);
                return;
            }

            using (new SecurityDisabler())
            {
                CorePipeline.Run(pipelineName, pipelineArgs);
            }
        }
    }
}
