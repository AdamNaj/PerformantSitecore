using System;
using System.Runtime.Serialization;

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Pipeline.SiteProviderInitialized
{
    [Serializable]
    public class SiteProviderInitializedPipelineArgs : PipelineArgs
    {
        public int Stage { get; set; }
        public Item Item { get; }

        public SiteProviderInitializedPipelineArgs(Item siteItem)
        {
            Stage = 0;
            Item = siteItem;
        }

        protected SiteProviderInitializedPipelineArgs(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Stage = info.GetInt32("SiteProviderInitializedPipelineArgs.Stage");
            var itemUriString = info.GetString("SiteProviderInitializedPipelineArgs.ItemUri");
            var itemUri = ItemUri.Parse(itemUriString);
            Item = Database.GetItem(itemUri);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SiteProviderInitializedPipelineArgs.Stage", this.Stage);
            info.AddValue("SiteProviderInitializedPipelineArgs.ItemUri", this.Item.Uri.ToString());
        }
    }
}
