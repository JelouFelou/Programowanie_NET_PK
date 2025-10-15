using System;
using System.IO;
using TextAnalytics.Core;
using TextAnalytics.Services;

namespace TextAnalytics.App
{
    /// <summary>
    /// Klasa zarządzająca głównym cyklem życia aplikacji konsolowej:
    /// wczytanie danych, analiza, prezentacja i zapis.
    /// Wszelkie usługi są wstrzykiwane w konstruktorze przez DI.
    /// </summary>
    public class TextAnalyticsApp
    {
        private readonly ILoggerService _logger;
        private readonly TextAnalyzer _analyzer;
        // Pole do przechowywania dostawcy danych (będzie ustawione w metodzie Run)
        private IInputProvider _inputProvider;

        public TextAnalyticsApp(ILoggerService logger, TextAnalyzer analyzer)
        {
            _logger = logger;
            _analyzer = analyzer;
            _inputProvider = null!; // Będzie zainicjowany w Run
        }

        /// <summary>
        /// Uruchamia główny proces analizy tekstu.
        /// </summary>
        /// <param name="cliArgs">Argumenty linii komend.</param>
        public void Run(string[] cliArgs)
        {
            _logger.LogInfo("Aplikacja Text Analytics uruchomiona.");

            try
            {
                string textToAnalyze = HandleInput(cliArgs);

                if (string.IsNullOrEmpty(textToAnalyze))
                {
                    _logger.LogInfo("Analiza pominięta: brak tekstu do przetworzenia.");
                    return;
                }

                _logger.LogInfo($"Przetwarzanie {textToAnalyze.Length} znaków...");

                // Analiza tekstu
                var stats = _analyzer.Analyze(textToAnalyze);

                // Prezentacja wyników w konsoli
                DisplayResults(stats);

                // Zapis wyników do JSON (wymaga Newtonsoft.Json)
                SaveResultsToJson(stats, "results.json");
            }
            catch (Exception ex)
            {
                _logger.LogError("Wystąpił krytyczny błąd aplikacji:", ex);
            }
        }

        /// <summary>
        /// Obsługuje logikę wyboru źródła danych na podstawie argumentów CLI.
        /// </summary>
        private string HandleInput(string[] cliArgs)
        {
            // 1. Sprawdzenie argumentów CLI
            if (cliArgs.Length > 0 && File.Exists(cliArgs[0]))
            {
                _logger.LogInfo($"Wykryto argument CLI: {cliArgs[0]}. Używam trybu FileInput.");
                // Użycie FileInputProvider dla ścieżki z CLI
                _inputProvider = new FileInputProvider(_logger, cliArgs[0]);
            }
            else if (cliArgs.Length > 0)
            {
                _logger.LogError($"Błąd: Ścieżka podana w argumencie CLI '{cliArgs[0]}' jest nieprawidłowa lub plik nie istnieje.");
                _logger.LogInfo("Wracam do domyślnego trybu: ConsoleInput.");
                _inputProvider = new ConsoleInputProvider(_logger);
            }
            else
            {
                _logger.LogInfo("Brak argumentów CLI. Używam domyślnego trybu: ConsoleInput.");
                _inputProvider = new ConsoleInputProvider(_logger);
            }

            // Wczytanie tekstu z wybranego dostawcy
            return _inputProvider.GetText();
        }

        /// <summary>
        /// Wyświetla statystyki w czytelnej formie na konsoli.
        /// </summary>
        private void DisplayResults(TextStatistics stats)
        {
            _logger.LogSuccess("--- RAPORT ANALIZY TEKSTU ---");
            Console.WriteLine();

            Console.WriteLine("--- 1. STATYSTYKI OGÓLNE ---");
            Console.WriteLine($"{"Znaków (ze spacjami):",-30} {stats.CharactersWithSpaces,10}");
            Console.WriteLine($"{"Znaków (bez spacji):",-30} {stats.CharactersWithoutSpaces,10}");
            Console.WriteLine($"{"Liter:",-30} {stats.Letters,10}");
            Console.WriteLine($"{"Cyfr:",-30} {stats.Digits,10}");
            Console.WriteLine($"{"Interpunkcja:",-30} {stats.Punctuation,10}");
            Console.WriteLine();

            Console.WriteLine("--- 2. STATYSTYKI SŁÓW ---");
            Console.WriteLine($"{"Liczba słów:",-30} {stats.WordCount,10}");
            Console.WriteLine($"{"Unikalne słowa:",-30} {stats.UniqueWordCount,10}");
            Console.WriteLine($"{"Średnia długość słowa:",-30} {stats.AverageWordLength:F2,10}");
            Console.WriteLine($"{"Najczęstsze słowo:",-30} {stats.MostCommonWord,10}");
            Console.WriteLine($"{"Najdłuższe słowo:",-30} {stats.LongestWord,10}");
            Console.WriteLine($"{"Najkrótsze słowo:",-30} {stats.ShortestWord,10}");
            Console.WriteLine();

            Console.WriteLine("--- 3. STATYSTYKI ZDAŃ ---");
            Console.WriteLine($"{"Liczba zdań:",-30} {stats.SentenceCount,10}");
            Console.WriteLine($"{"Średnia słów na zdanie:",-30} {stats.AverageWordsPerSentence:F2,10}");
            Console.WriteLine($"{"Najdłuższe zdanie (fragment):",-30}");
            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"\"{stats.LongestSentence.Substring(0, Math.Min(stats.LongestSentence.Length, 100))}...\"");
            Console.WriteLine(new string('-', 60));
            Console.WriteLine();
        }

        /// <summary>
        /// Serializuje i zapisuje obiekt TextStatistics do pliku JSON.
        /// </summary>
        private void SaveResultsToJson(TextStatistics stats, string filePath)
        {
            try
            {
                // Użycie Newtonsoft.Json do serializacji z wcięciami dla czytelności
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(stats, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, json);
                _logger.LogSuccess($"Wyniki analizy zapisane do pliku: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd podczas zapisu do JSON ({filePath})", ex);
            }
        }
    }
}
