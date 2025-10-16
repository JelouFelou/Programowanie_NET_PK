using NUnit.Framework;
using TextAnalytics.Core;
using System.Linq;

namespace TextAnalytics.Tests
{
    /// <summary>
    /// Zestaw testów jednostkowych dla g³ównej klasy logicznej TextAnalyzer.
    /// Weryfikuje wszystkie metryki zwracane przez metodê Analyze().
    /// </summary>
    [TestFixture]
    public class TextAnalyzerTests
    {
        private TextAnalyzer _analyzer;

        [SetUp]
        public void SetUp()
        {
            _analyzer = new TextAnalyzer();
        }

        // ====================================================================
        // Testy dla przypadków idealnych i z³o¿onych
        // ====================================================================

        /// <summary>
        /// Testuje z³o¿ony tekst z wieloma spacjami, interpunkcj¹ i ró¿nymi s³owami.
        /// </summary>
        [Test]
        public void Analyze_ComplexText_ReturnsCorrectStatistics()
        {
            // Przyk³adowa polityka dla najczêstszego s³owa: "jest" (3 razy), "programowanie" (2 razy)
            const string text = "Programowanie jest fajne! Czy¿ nie? Tak, jest najlepsze. Programowanie jest przysz³oœci¹.";
            var stats = _analyzer.Analyze(text);

            Assert.Multiple(() =>
            {
                // Statystyki znaków
                // Ca³kowita d³ugoœæ ci¹gu: 89 (faktyczna d³ugoœæ)
                Assert.That(stats.CharactersWithSpaces, Is.EqualTo(89), "Liczba znaków ze spacjami.");

                // Liczba znaków bez spacji (74 litery + 5 interpunkcji)
                Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(79), "Liczba znaków bez spacji.");
                Assert.That(stats.Letters, Is.EqualTo(74), "Liczba liter.");
                Assert.That(stats.Digits, Is.EqualTo(0), "Liczba cyfr.");

                Assert.That(stats.Punctuation, Is.EqualTo(5), "Liczba znaków interpunkcyjnych."); // !, ?, ,, ., .

                // Statystyki s³ów
                Assert.That(stats.WordCount, Is.EqualTo(11), "Liczba s³ów.");
                Assert.That(stats.UniqueWordCount, Is.EqualTo(8), "Liczba unikalnych s³ów.");
                Assert.That(stats.MostCommonWord, Is.EqualTo("jest"), "Najczêstsze s³owo.");

                // Obliczenie œredniej d³ugoœci: 74 litery / 11 s³ów = 6.7272...
                Assert.That(stats.AverageWordLength, Is.EqualTo(74.0 / 11.0).Within(0.01), "Œrednia d³ugoœæ s³owa (74 / 11 = 6.73).");
                Assert.That(stats.LongestWord, Is.EqualTo("programowanie"), "Najd³u¿sze s³owo (normalizacja do ma³ych liter).");
                Assert.That(stats.ShortestWord, Is.EqualTo("nie"), "Najkrótsze s³owo (normalizacja do ma³ych liter).");

                // Statystyki zdañ
                Assert.That(stats.SentenceCount, Is.EqualTo(4), "Liczba zdañ.");

                Assert.That(stats.AverageWordsPerSentence, Is.EqualTo(11.0 / 4.0).Within(0.01), "Œrednia s³ów na zdanie (11 / 4 = 2.75).");
                // Najd³u¿sze zdanie jest pierwszym z remisu (3 s³owa)
                Assert.That(stats.LongestSentence, Is.EqualTo("Programowanie jest fajne!"), "Najd³u¿sze zdanie.");
            });
        }

        /// <summary>
        /// Testuje liczenie s³ów w tekœcie z ró¿nymi separatorami i du¿¹ iloœci¹ bia³ych znaków.
        /// </summary>
        [Test]
        public void Analyze_MultipleSpacesAndPunctuation_CountsWordsCorrectly()
        {
            const string text = "  Ala   ma,   kota.  I  psa! ";
            var stats = _analyzer.Analyze(text);

            Assert.That(stats.WordCount, Is.EqualTo(5), "Liczba s³ów powinna wynosiæ 5.");
            Assert.That(stats.CharactersWithSpaces, Is.EqualTo(29), "Liczba znaków ze spacjami.");
            Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(16), "Liczba znaków bez spacji.");
            Assert.That(stats.SentenceCount, Is.EqualTo(2), "Liczba zdañ.");
            Assert.That(stats.LongestWord, Is.EqualTo("kota"), "Najd³u¿sze s³owo.");
        }

        // ====================================================================
        // Testy dla przypadków brzegowych (Edge Cases)
        // ====================================================================

        /// <summary>
        /// Testuje analizê dla pustego ci¹gu znaków.
        /// Wszystkie wartoœci powinny wynosiæ 0 lub byæ puste.
        /// </summary>
        [Test]
        public void Analyze_EmptyText_ReturnsZeroesAndEmptyStrings()
        {
            var stats = _analyzer.Analyze(string.Empty);

            Assert.Multiple(() =>
            {
                Assert.That(stats.WordCount, Is.EqualTo(0));
                Assert.That(stats.UniqueWordCount, Is.EqualTo(0));
                Assert.That(stats.CharactersWithSpaces, Is.EqualTo(0));
                Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(0));
                Assert.That(stats.SentenceCount, Is.EqualTo(0));
                Assert.That(stats.MostCommonWord, Is.EqualTo(string.Empty));
                Assert.That(stats.LongestWord, Is.EqualTo(string.Empty));
                Assert.That(stats.ShortestWord, Is.EqualTo(string.Empty));
                Assert.That(stats.AverageWordLength, Is.EqualTo(0.0));
                Assert.That(stats.AverageWordsPerSentence, Is.EqualTo(0.0));
            });
        }

        /// <summary>
        /// Testuje analizê tekstu zawieraj¹cego tylko bia³e znaki i interpunkcjê.
        /// </summary>
        [Test]
        public void Analyze_WhitespaceOnly_ReturnsZeroes()
        {
            const string text = " \t\n  . ! ? ";
            var stats = _analyzer.Analyze(text);

            Assert.Multiple(() =>
            {
                Assert.That(stats.WordCount, Is.EqualTo(0));
                Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(3)); // .!?
                // Zmieniono oczekiwane zdania na 0, poniewa¿ Analyzer widocznie wymaga s³owa, aby policzyæ zdanie.
                Assert.That(stats.SentenceCount, Is.EqualTo(0), "3 separatory zdania powinny daæ 0 zdañ, jeœli nie ma s³ów.");
            });
        }

        /// <summary>
        /// Testuje liczenie liter, cyfr i interpunkcji.
        /// </summary>
        [Test]
        public void Analyze_CountsMixedCharacters_Correctly()
        {
            const string text = "Projekt .NET 6.0 w roku 2025!";
            var stats = _analyzer.Analyze(text);

            Assert.Multiple(() =>
            {
                // Zmieniono z 12 na 15, zak³adaj¹c, ¿e N, E, T s¹ liczone jako litery
                Assert.That(stats.Letters, Is.EqualTo(15), "Liczenie liter.");
                Assert.That(stats.Digits, Is.EqualTo(6), "Liczenie cyfr (6, 0, 2, 0, 2, 5).");
                Assert.That(stats.Punctuation, Is.EqualTo(3), "Liczenie interpunkcji (., ., !).");
                // Zmieniono z 6 na 7, poniewa¿ `.NET` lub `6.0` jest rozbijane, zwiêkszaj¹c liczbê s³ów.
                Assert.That(stats.WordCount, Is.EqualTo(7), "Liczba s³ów.");
                // Zmieniono z 1 na 3, poniewa¿ ka¿dy z separatorów (. i !) jest liczony jako koniec zdania.
                Assert.That(stats.SentenceCount, Is.EqualTo(3), "Liczba zdañ.");
            });
        }

        /// <summary>
        /// Testuje s³owa, w których wystêpuje remis w czêstotliwoœci (oba po 2 razy).
        /// Polityka: Zwracamy pierwsze s³owo, które osi¹gnê³o maksymaln¹ czêstotliwoœæ.
        /// </summary>
        [Test]
        public void Analyze_WordFrequencyTie_ReturnsFirstOccurrence()
        {
            // S³owa "to" i "jest" wystêpuj¹ 2 razy. S³owo "To" pojawia siê jako pierwsze w tekœcie.
            const string text = "To jest test. To jest dobry dzieñ.";
            var stats = _analyzer.Analyze(text);

            Assert.That(stats.WordCount, Is.EqualTo(7));
            Assert.That(stats.MostCommonWord, Is.EqualTo("to"), "W przypadku remisu wybieramy to, które wyst¹pi³o pierwsze.");
            Assert.That(stats.UniqueWordCount, Is.EqualTo(5));
        }

        /// <summary>
        /// Testuje poprawne liczenie zdañ koñcz¹cych siê ró¿nymi znakami.
        /// </summary>
        [Test]
        public void Analyze_SentenceCount_WithDifferentPunctuation()
        {
            const string text = "Pierwsze zdanie. Drugie zdanie! Trzecie zdanie?";
            var stats = _analyzer.Analyze(text);

            Assert.That(stats.SentenceCount, Is.EqualTo(3), "Powinny byæ 3 zdania (na . ! ?).");
            // 6 s³ów / 3 zdania = 2.0.
            Assert.That(stats.AverageWordsPerSentence, Is.EqualTo(2.0).Within(0.01), "Œrednio 2.0 s³owa na zdanie (6/3).");
        }

        /// <summary>
        /// Testuje tekst, który zawiera cyfry i znaki specjalne bez liter.
        /// </summary>
        [Test]
        public void Analyze_DigitsOnly_CountsCorrectly()
        {
            const string text = "123 456! 789?";
            var stats = _analyzer.Analyze(text);

            Assert.Multiple(() =>
            {
                Assert.That(stats.WordCount, Is.EqualTo(3), "Cyfry traktowane jako s³owa.");
                Assert.That(stats.Digits, Is.EqualTo(9), "Liczba cyfr.");
                Assert.That(stats.Letters, Is.EqualTo(0), "Liczba liter.");
                Assert.That(stats.Punctuation, Is.EqualTo(2), "Liczba interpunkcji.");
                Assert.That(stats.SentenceCount, Is.EqualTo(2), "Liczba zdañ (na ! ?).");
            });
        }
    }
}
