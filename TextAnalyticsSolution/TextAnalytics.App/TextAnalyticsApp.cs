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
        private readonly ITextAnalyzer _analyzer; // Zmieniono na interfejs ITextAnalyzer
        // Pole do przechowywania dostawcy danych (będzie ustawione w metodzie Run)
        private IInputProvider _inputProvider;

        public TextAnalyticsApp(ILoggerService logger, ITextAnalyzer analyzer) // Zmieniono na interfejs ITextAnalyzer
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
                    _logger.LogInfo("Analiza pominieta: brak tekstu do przetworzenia.");
                    return;
                }

                // 1. Analiza
                _logger.LogInfo("Rozpoczecie analizy tekstu...");
                var stats = _analyzer.Analyze(textToAnalyze);
                _logger.LogSuccess("Analiza zakończona pomyślnie.");

                // 2. Prezentacja
                PrintResults(stats);

                // 3. Zapis (opcjonalny)
                if (cliArgs.Length > 1 && cliArgs[1].ToLower() == "--save")
                {
                    string outputFilePath = cliArgs.Length > 2 ? cliArgs[2] : "results.json";
                    SaveResultsToJson(stats, outputFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Wystapil nieoczekiwany blad w trakcie dzialania aplikacji.", ex);
            }
        }

        /// <summary>
        /// Obsługuje argumenty CLI w celu określenia źródła danych (konsola vs plik).
        /// </summary>
        private string HandleInput(string[] cliArgs)
        {
            if (cliArgs.Length > 0 && cliArgs[0].ToLower() == "--file")
            {
                // Tryb plikowy: ścieżka może być pierwszym lub drugim argumentem
                string? filePath = cliArgs.Length > 1 && cliArgs[1].ToLower() != "--save" ? cliArgs[1] : null;

                // FileInputProvider wymaga wstrzyknięcia loggera, a następnie ścieżki pliku
                _inputProvider = new FileInputProvider(_logger, filePath);
            }
            else
            {
                // Domyślny tryb konsolowy
                _inputProvider = new ConsoleInputProvider(_logger);
            }

            return _inputProvider.GetText();
        }

        /// <summary>
        /// Wyświetla wyniki analizy w konsoli.
        /// </summary>
        private void PrintResults(TextStatistics stats)
        {
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("                WYNIKI ANALIZY TEKSTU");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();

            Console.WriteLine("--- 1. STATYSTYKI ZNAKÓW ---");
            Console.WriteLine($@"{"Znakow (z bialymi znakami):",-30} {stats.CharactersWithSpaces,10}");
            Console.WriteLine($@"{"Znakow (bez bialych znakow):",-30} {stats.CharactersWithoutSpaces,10}");
            Console.WriteLine($@"{"Liter:",-30} {stats.Letters,10}");
            Console.WriteLine($@"{"Cyfr:",-30} {stats.Digits,10}");
            Console.WriteLine($@"{"Interpunkcja:",-30} {stats.Punctuation,10}");
            Console.WriteLine();

            Console.WriteLine("--- 2. STATYSTYKI SLOW ---");
            Console.WriteLine($@"{"Wszystkich slow:",-30} {stats.WordCount,10}");
            Console.WriteLine($@"{"Unikalnych slow:",-30} {stats.UniqueWordCount,10}");
            Console.WriteLine($@"{"Najczesciej wystepujace slowo:",-30} {stats.MostCommonWord,10}");
            Console.WriteLine($@"{"Srednia dlugosc slowa:",-30} {stats.AverageWordLength:F2,10}");
            Console.WriteLine($@"{"Najdluzsze slowo:",-30} {stats.LongestWord,10}");
            Console.WriteLine($@"{"Najkrotsze slowo:",-30} {stats.ShortestWord,10}");
            Console.WriteLine();

            Console.WriteLine("--- 3. STATYSTYKI ZDAŃ ---");
            Console.WriteLine($@"{"Liczba zdan:",-30} {stats.SentenceCount,10}");
            Console.WriteLine($@"{"Srednia slow na zdanie:",-30} {stats.AverageWordsPerSentence:F2,10}");
            Console.WriteLine($@"{"Najdluzsze zdanie (fragment):",-30}");
            Console.WriteLine(new string('-', 60));
            Console.WriteLine($@"""{stats.LongestSentence.Substring(0, Math.Min(stats.LongestSentence.Length, 100))}...""");
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
                _logger.LogError($"Blad zapisu pliku JSON do '{filePath}'.", ex);
            }
        }
    }
}
