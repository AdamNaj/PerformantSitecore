using Sitecore.Jobs.AsyncUI;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized
{
    public abstract class BaseSiteProviderInitialized : ISiteProviderInitializedProcessor
    {
        public virtual void Process(SiteProviderInitializedPipelineArgs args)
        {
            args.Stage++;
            if (JobContext.IsJob && JobContext.Job.Category.Equals("SxaSiteProvider"))
            {
                JobContext.Job.Status.Processed = args.Stage;
            }
            OnProcess(args);
        }

        public abstract void OnProcess(SiteProviderInitializedPipelineArgs args);
    }
}
