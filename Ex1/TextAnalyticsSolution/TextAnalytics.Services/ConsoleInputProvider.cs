using System;
using System.Text;
using System.Linq;

namespace TextAnalytics.Services
{
    /// <summary>
    /// Implementacja IInputProvider wczytująca tekst bezpośrednio z konsoli.
    /// Używa poprawionej logiki odczytu dla niezawodnego wykrywania podwójnego ENTER, 
    /// jednocześnie zachowując pojedyncze puste linie w tekście wejściowym.
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
            // Komunikat bez polskich znaków, aby uniknąć problemów z kodowaniem w konsoli
            _logger.LogInfo("Oczekiwanie na dane. Wprowadz tekst do analizy (zakoncz dwukrotnym wcisnieciem ENTER):");

            var inputBuilder = new StringBuilder();
            int emptyLineCount = 0;

            // Pętla odczytuje wiersze. Zakończy się po dwóch kolejnych wierszach pustych/zawierających tylko białe znaki.
            while (true)
            {
                // To jest jedyna deklaracja zmiennej 'line' w tej metodzie. 
                // Użycie 'string? line =' w tym miejscu jest poprawne i eliminuje błąd CS0136,
                // pod warunkiem, że nie ma drugiej deklaracji.
                string? line = Console.ReadLine();

                if (line == null)
                {
                    // Koniec danych wejściowych (np. przekierowanie pliku)
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    emptyLineCount++;

                    if (emptyLineCount >= 2)
                    {
                        // Wykryto podwójny ENTER. Przerywamy pętlę.
                        // UWAGA: Nie dodajemy tej drugiej pustej linii do bufora.
                        break;
                    }

                    // Jeśli jest to PIERWSZA pusta linia (emptyLineCount == 1), 
                    // dodajemy ją do bufora, aby zachować formatowanie wejściowe.
                    inputBuilder.Append(Environment.NewLine);
                }
                else
                {
                    // Wczytano faktyczny tekst.
                    emptyLineCount = 0; // Resetujemy licznik pustych linii

                    // Dodajemy wiersz wraz z separatorem nowej linii, aby oddzielić go od następnego wejścia
                    inputBuilder.Append(line);
                    inputBuilder.Append(Environment.NewLine);
                }
            }

            // Używamy Trim() na końcu, aby usunąć potencjalnie ostatni Environment.NewLine
            // lub zbędne białe znaki na początku/końcu.
            var input = inputBuilder.ToString().Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                _logger.LogInfo("Nie wprowadzono zadnego tekstu. Zostanie przeanalizowany pusty ciag.");
            }
            else
            {
                _logger.LogSuccess($"Pomyslnie wczytano {input.Length} znakow.");
            }

            // Zwracamy wczytany i przycięty tekst.
            return input; // Normalizacja zostanie wykonana w TextAnalyzer
        }
    }
}
