using System;

namespace TextAnalytics.Services
{
    /// <summary>
    /// Prosta implementacja ILoggerService, która loguje komunikaty do konsoli.
    /// Używa kolorów dla lepszej czytelności.
    /// </summary>
    public class ConsoleLogger : ILoggerService
    {
        private void WriteColoredMessage(string prefix, string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss} {prefix}] {message}");
            Console.ResetColor();
        }

        public void LogInfo(string message)
        {
            WriteColoredMessage("INFO", message, ConsoleColor.White);
        }

        public void LogError(string message, Exception? ex = null)
        {
            var fullMessage = message;
            if (ex != null)
            {
                // Użycie ex.Message jest bezpieczne, ponieważ ex nie jest null.
                fullMessage += $" Szczegoly: {ex.Message}";
            }
            WriteColoredMessage("ERROR", fullMessage, ConsoleColor.Red);
        }

        public void LogSuccess(string message)
        {
            WriteColoredMessage("SUCCESS", message, ConsoleColor.Green);
        }
    }
}