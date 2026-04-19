using System;

using Sitecore.Diagnostics;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Helpers
{
    public class LoggingHelper : ILoggingHelper
    {
        public virtual void LogDebug(string message, object owner)
        {
            Log.Debug(message, owner);
        }

        public virtual void LogInfo(string message, object owner)
        {
            Log.Info(message, owner);
        }

        public virtual void LogWarning(string message, object owner)
        {
            Log.Warn(message, owner);
        }

        public virtual void LogWarning(string message, Exception exception, object owner)
        {
            Log.Warn(message, exception, owner);
        }

        public virtual void LogError(string message, object owner)
        {
            Log.Error(message, owner);
        }

        public virtual void LogError(string message, Exception exception, object owner)
        {
            Log.Error(message, exception, owner);
        }
    }
}
