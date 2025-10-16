using System;

namespace TextAnalytics.Services
{
    /// <summary>
    /// Definiuje interfejs dla usługi logowania.
    /// </summary>
    public interface ILoggerService
    {
        void LogInfo(string message);
        // Poprawna sygnatura z opcjonalnym wyjątkiem (na podstawie FileInputProvider.cs)
        void LogError(string message, Exception? ex = null);
        void LogSuccess(string message);
    }
}