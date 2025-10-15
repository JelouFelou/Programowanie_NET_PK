using System;

namespace TextAnalytics.Services
{
    /// <summary>
    /// Definiuje interfejs dla usługi logowania.
    /// </summary>
    public interface ILoggerService
    {
        void LogInfo(string message);
        void LogError(string message, Exception? ex = null);
        void LogSuccess(string message);
    }
}