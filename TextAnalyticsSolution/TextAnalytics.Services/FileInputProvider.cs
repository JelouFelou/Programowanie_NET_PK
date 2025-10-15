using System.IO;

namespace TextAnalytics.Services
{
    /// <summary>
    /// Implementacja IInputProvider wczytująca tekst z pliku.
    /// Domyślnie szuka pliku "input.txt" w katalogu aplikacji.
    /// </summary>
    public class FileInputProvider : IInputProvider
    {
        private readonly ILoggerService _logger;
        private const string DefaultFilePath = "input.txt";

        public FileInputProvider(ILoggerService logger)
        {
            _logger = logger;
        }

        public string GetText()
        {
            _logger.LogInfo($"Próba wczytania tekstu z pliku: {DefaultFilePath}");

            if (!File.Exists(DefaultFilePath))
            {
                _logger.LogError($"Plik {DefaultFilePath} nie został znaleziony. Zostanie użyty pusty ciąg.");
                return string.Empty;
            }

            try
            {
                var text = File.ReadAllText(DefaultFilePath);
                _logger.LogSuccess($"Pomyślnie wczytano {text.Length} znaków z pliku.");
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas wczytywania pliku {DefaultFilePath}", ex);
                return string.Empty;
            }
        }
    }
}