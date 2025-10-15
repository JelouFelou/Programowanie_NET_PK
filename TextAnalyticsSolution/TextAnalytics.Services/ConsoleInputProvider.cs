using System;

namespace TextAnalytics.Services
{
    /// <summary>
    /// Implementacja IInputProvider wczytująca tekst bezpośrednio z konsoli.
    /// </summary>
    public class ConsoleInputProvider : IInputProvider
    {
        private readonly ILoggerService _logger;

        public ConsoleInputProvider(ILoggerService logger)
        {
            _logger = logger;
        }

        public string GetText()
        {
            _logger.LogInfo("Oczekiwanie na dane. Wprowadź tekst do analizy (zakończ dwukrotnym wciśnięciem ENTER):");

            // Wczytanie tekstu wieloliniowego z konsoli
            var input = "";
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }
                input += line + Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                _logger.LogInfo("Nie wprowadzono żadnego tekstu. Zostanie przeanalizowany pusty ciąg.");
            }

            return input.Trim();
        }
    }
}