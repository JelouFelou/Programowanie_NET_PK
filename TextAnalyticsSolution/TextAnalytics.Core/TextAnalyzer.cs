using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAnalytics.Core
{
    /// <summary>
    /// Odpowiedzialne za przeprowadzenie pełnej analizy statystycznej podanego tekstu.
    /// </summary>
    public sealed class TextAnalyzer
    {
        // Znaki kończące zdanie używane do tokenizacji
        private readonly char[] SentenceTerminators = new[] { '.', '!', '?' };

        // Znaki interpunkcyjne do usunięcia przy tokenizacji słów
        private readonly char[] PunctuationToRemove =
            Enumerable.Range(0, char.MaxValue + 1)
                .Select(i => (char)i)
                .Where(c => char.IsPunctuation(c) && !new[] { '-', '\'' }.Contains(c)) // Zachowujemy - i ' dla prostoty słów
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

            // --- 1. Analiza znaków ---
            int charactersWithSpaces = text.Length;
            int charactersWithoutSpaces = text.Count(c => !char.IsWhiteSpace(c));
            int letters = text.Count(char.IsLetter);
            int digits = text.Count(char.IsDigit);
            int punctuation = text.Count(char.IsPunctuation);

            // --- 2. Analiza słów ---
            var allWords = GetCleanWords(text);
            int wordCount = allWords.Count;

            // Zapewnienie pustych wyników, jeśli nie ma słów
            if (wordCount == 0)
            {
                return new TextStatistics(charactersWithSpaces, charactersWithoutSpaces, letters, digits, punctuation,
                                            0, 0, string.Empty, 0.0, string.Empty, string.Empty, 0, 0.0, string.Empty);
            }

            int uniqueWordCount = allWords.Distinct().Count();

            string mostCommonWord = allWords
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? string.Empty;

            double averageWordLength = allWords.Average(w => w.Length);
            string longestWord = allWords.OrderByDescending(w => w.Length).First();
            string shortestWord = allWords.OrderBy(w => w.Length).First();


            // --- 3. Analiza zdań ---
            var sentences = GetSentences(text);
            int sentenceCount = sentences.Count;

            // Obliczenie słów na zdanie
            var wordsInSentences = sentences
                .Select(s => GetCleanWords(s).Count)
                .ToList();

            double averageWordsPerSentence = wordsInSentences.Any() ? wordsInSentences.Average() : 0.0;

            // Znalezienie najdłuższego zdania (mierzone liczbą słów)
            string longestSentenceText = string.Empty;
            int maxWordCount = -1;

            // Tokenizujemy słowa dla każdego zdania, aby znaleźć najdłuższe
            foreach (var sentence in sentences)
            {
                var currentWordCount = GetCleanWords(sentence).Count;
                if (currentWordCount > maxWordCount)
                {
                    maxWordCount = currentWordCount;
                    longestSentenceText = sentence.Trim();
                }
            }


            // --- 4. Zwrócenie wyników ---
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
                longestSentenceText
            );
        }

        // --- Metody pomocnicze (wymagane w API, ale używane wewnętrznie) ---

        /// <summary>
        /// Zlicza wszystkie znaki w tekście.
        /// </summary>
        /// <param name="text">Tekst wejściowy.</param>
        /// <param name="includeSpaces">Czy wliczać białe znaki.</param>
        public int CountCharacters(string text, bool includeSpaces = true)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return includeSpaces
                ? text.Length
                : text.Count(c => !char.IsWhiteSpace(c));
        }

        /// <summary>
        /// Zlicza słowa w tekście (prosta tokenizacja).
        /// </summary>
        /// <param name="text">Tekst wejściowy.</param>
        public int CountWords(string text)
        {
            return GetCleanWords(text).Count;
        }

        // --- Implementacje tokenizacji ---

        /// <summary>
        /// Czyści tekst z większości interpunkcji i zwraca listę słów w małych literach.
        /// </summary>
        private List<string> GetCleanWords(string text)
        {
            // Proste usunięcie interpunkcji i normalizacja do małych liter
            // Używamy StringBuilder do efektywnej zamiany interpunkcji na spacje
            var sb = new StringBuilder(text.ToLowerInvariant());

            foreach (var p in PunctuationToRemove)
            {
                sb.Replace(p, ' ');
            }

            // Wygładzanie tekstu (usuwanie podwójnych spacji)
            var cleanedText = sb.ToString().Replace("  ", " ");

            // Tokenizacja: Podział na słowa przy użyciu spacji/białych znaków
            return cleanedText
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.All(char.IsLetterOrDigit) && w.Length > 0) // Ostateczne filtrowanie
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
                        // Upewnij się, że zdanie nie jest tylko terminatorem (np. "....")
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
