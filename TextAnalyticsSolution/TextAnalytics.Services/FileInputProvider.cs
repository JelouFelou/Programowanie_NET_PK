using System;
using System.IO;

namespace TextAnalytics.Services
{
    /// <summary>
    /// Implementacja IInputProvider wczytująca tekst z pliku.
    /// Przyjmuje ścieżkę pliku w konstruktorze.
    /// </summary>
    public class FileInputProvider : IInputProvider
    {
        private readonly ILoggerService _logger;
        private readonly string _filePath;
        private const string DefaultFilePath = "input.txt";

        /// <summary>
        /// Konstruktor dla FileInputProvider.
        /// </summary>
        /// <param name="logger">Wstrzyknięta usługa logowania.</param>
        /// <param name="filePath">Ścieżka do pliku. Jeśli null/pusta, używa domyślnej.</param>
        public FileInputProvider(ILoggerService logger, string? filePath = null)
        {
            _logger = logger;
            // Jeśli ścieżka jest podana, użyj jej. W przeciwnym razie użyj domyślnej.
            _filePath = string.IsNullOrWhiteSpace(filePath) ? DefaultFilePath : filePath;
        }

        public string GetText()
        {
            _logger.LogInfo($"Próba wczytania tekstu z pliku: {_filePath}");

            if (!File.Exists(_filePath))
            {
                _logger.LogError($"Plik '{_filePath}' nie został znaleziony. Upewnij się, że ścieżka jest poprawna.");
                return string.Empty;
            }

            try
            {
                var text = File.ReadAllText(_filePath);

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogError($"Plik '{_filePath}' jest pusty lub zawiera tylko białe znaki.");
                    return string.Empty;
                }

                _logger.LogSuccess($"Pomyślnie wczytano {text.Length} znaków z pliku.");
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas wczytywania pliku {_filePath}", ex);
                return string.Empty;
            }
        }
    }
}
