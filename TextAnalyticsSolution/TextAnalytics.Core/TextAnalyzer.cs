using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAnalytics.Core
{
    // Klasa TextAnalyzer implementuje ITextAnalyzer, który jest teraz zdefiniowany w ITextAnalyzer.cs
    /// <summary>
    /// Odpowiedzialne za przeprowadzenie pełnej analizy statystycznej podanego tekstu.
    /// </summary>
    public sealed class TextAnalyzer : ITextAnalyzer
    {
        // Znaki kończące zdanie używane do tokenizacji
        private readonly char[] SentenceTerminators = new[] { '.', '!', '?' };

        // Znaki interpunkcyjne do usunięcia przy tokenizacji słów
        // Ważne: usuwamy wszystkie znaki interpunkcyjne oprócz myślnika i apostrofu
        private readonly char[] PunctuationToRemove =
            Enumerable.Range(0, char.MaxValue + 1)
                .Select(i => (char)i)
                .Where(c => char.IsPunctuation(c) && !new[] { '-', '\'' }.Contains(c))
                .ToArray();

        /// <summary>
        /// Główna metoda przeprowadzająca pełną analizę tekstu i zwracająca rekord TextStatistics.
        /// </summary>
        /// <param name="text">Tekst do analizy.</param>
        /// <returns>Obiekt TextStatistics zawierający wszystkie metryki.</returns>
        public TextStatistics Analyze(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new TextStatistics(0, 0, 0, 0, 0, 0, 0, string.Empty, 0.0, string.Empty, string.Empty, 0, 0.0, string.Empty);
            }

            var words = GetCleanWords(text);
            var sentences = GetSentences(text);

            // 1. Statystyki znaków
            int charactersWithSpaces = text.Length;
            int charactersWithoutSpaces = text.Count(c => !char.IsWhiteSpace(c));
            int letters = text.Count(char.IsLetter);
            int digits = text.Count(char.IsDigit);
            int punctuation = text.Count(c => char.IsPunctuation(c));

            // 2. Statystyki słów
            int wordCount = words.Count;
            int uniqueWordCount = words.Distinct().Count();

            double averageWordLength = words.Any() ? words.Average(w => w.Length) : 0.0;

            // Poprawa: W przypadku remisu, sortowanie alfabetyczne jest bardziej deterministyczne (ThenBy(w => w))
            string longestWord = words.Any() ? words.OrderByDescending(w => w.Length).ThenBy(w => w).FirstOrDefault() ?? string.Empty : string.Empty;
            string shortestWord = words.Any() ? words.OrderBy(w => w.Length).ThenBy(w => w).FirstOrDefault() ?? string.Empty : string.Empty;

            // Poprawa dla MostCommonWord (błąd remisu w teście): Sortowanie po liczbie, a następnie alfabetycznie (deterministycznie).
            string mostCommonWord = words
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .FirstOrDefault() ?? string.Empty;

            // 3. Statystyki zdań
            int sentenceCount = sentences.Count;
            double averageWordsPerSentence = sentenceCount > 0 ? (double)wordCount / sentenceCount : 0.0;

            // Poprawa dla LongestSentence (błąd w teście): Zwraca najdłuższe zdanie po długości znaków.
            string longestSentence = sentences.Any()
                ? sentences.OrderByDescending(s => s.Length).FirstOrDefault() ?? string.Empty
                : string.Empty;


            return new TextStatistics(
                charactersWithSpaces,
                charactersWithoutSpaces,
                letters,
                digits,
                punctuation,
                wordCount,
                uniqueWordCount,
                mostCommonWord,
                averageWordLength,
                longestWord,
                shortestWord,
                sentenceCount,
                averageWordsPerSentence,
                longestSentence
            );
        }

        /// <summary>
        /// Czyści tekst z większości interpunkcji i zwraca listę słów w małych literach.
        /// </summary>
        private List<string> GetCleanWords(string text)
        {
            // Normalizacja do małych liter
            var sb = new StringBuilder(text.ToLowerInvariant());

            // Zamiana całej zdefiniowanej interpunkcji na spację
            foreach (var p in PunctuationToRemove)
            {
                sb.Replace(p, ' ');
            }

            // Tokenizacja
            return sb.ToString()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(w => !string.IsNullOrWhiteSpace(w)) // Usuń pozostałe puste/białe znaki
                .ToList();
        }

        /// <summary>
        /// Dzieli tekst na zdania na podstawie ustalonych terminatorów (. ! ?).
        /// </summary>
        private List<string> GetSentences(string text)
        {
            var sentences = new List<string>();
            var sentenceBuilder = new StringBuilder();

            foreach (char c in text)
            {
                sentenceBuilder.Append(c);

                if (SentenceTerminators.Contains(c) && sentenceBuilder.Length > 0)
                {
                    var sentence = sentenceBuilder.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(sentence))
                    {
                        // Upewnij się, że zdanie zawiera słowa
                        if (GetCleanWords(sentence).Any())
                        {
                            sentences.Add(sentence);
                        }
                    }
                    sentenceBuilder.Clear();
                }
            }

            // Dodanie pozostałej części tekstu, jeśli nie kończył się terminatorem
            var remainder = sentenceBuilder.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(remainder) && GetCleanWords(remainder).Any())
            {
                sentences.Add(remainder);
            }

            return sentences;
        }
    }
}
