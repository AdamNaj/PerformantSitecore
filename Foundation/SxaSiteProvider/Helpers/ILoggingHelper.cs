using System;

namespace PerformantSitecore.Foundation.SxaSiteProvider.Helpers
{
    public interface ILoggingHelper
    {
        void LogDebug(string message, object owner);
        void LogInfo(string message, object owner);
        void LogWarning(string message, object owner);
        void LogWarning(string message, Exception exception, object owner);
        void LogError(string message, object owner);
        void LogError(string message, Exception exception, object owner);
    }
}
