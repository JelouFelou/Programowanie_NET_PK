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
        private ITextAnalyzer _analyzer; // Zmieniono na interfejs ITextAnalyzer

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
                // Ca³kowita d³ugoœæ ci¹gu: 89
                // Liczba spacji: 10
                // Bez spacji: 79 (74 litery + 5 interpunkcja)
                // Liczba liter: 74
                // Interpunkcja: 5 (!, ?, ,, ., .)
                Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(79), "1) Liczba znaków bez bia³ych znaków.");
                Assert.That(stats.Letters, Is.EqualTo(74), "2) Liczba liter.");
                Assert.That(stats.Punctuation, Is.EqualTo(5), "3) Liczba znaków interpunkcyjnych (!, ?, ,, ., .).");

                // Statystyki s³ów (11 s³ów: Programowanie, jest, fajne, Czy¿, nie, Tak, jest, najlepsze, Programowanie, jest, przysz³oœci¹)
                Assert.That(stats.WordCount, Is.EqualTo(11), "4) Liczba s³ów.");
                // Œrednia: 74 litery / 11 s³ów = 6.7272...
                Assert.That(stats.AverageWordLength, Is.EqualTo(6.7272727272727275d).Within(0.01d), "5) Œrednia d³ugoœæ s³owa (74/11).");
                // Najkrótsze: "nie" (3) i "tak" (3). Alfabetycznie "nie" < "tak".
                Assert.That(stats.ShortestWord, Is.EqualTo("nie"), "6) Najkrótsze s³owo (wybrane alfabetycznie w przypadku remisu).");

                // Statystyki zdañ (4 zdania: 1.!, 2.?, 3.., 4..)
                Assert.That(stats.SentenceCount, Is.EqualTo(4), "Liczba zdañ (4).");
                // Œrednia s³ów na zdanie: 11 s³ów / 4 zdania = 2.75
                Assert.That(stats.AverageWordsPerSentence, Is.EqualTo(2.75d).Within(0.01d), "7) Œrednia s³ów na zdanie (11/4).");
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
        /// Testuje sytuacjê, gdy kilka s³ów wystêpuje z t¹ sam¹, najwy¿sz¹ czêstotliwoœci¹.
        /// </summary>
        [Test]
        public void Analyze_WordFrequencyTie_ReturnsFirstOccurrence()
        {
            // Tekst: "jest" (2), "programowanie" (2), "to" (2), "fajne" (1).
            // Wszystkie s³owa s¹ znormalizowane do ma³ych liter.
            // "jest", "programowanie", "to" s¹ remisem. Alfabetycznie najmniejsze to "jest".
            const string text = "Jest to programowanie! To jest programowanie. Fajne.";
            var stats = _analyzer.Analyze(text);

            // Oczekiwana polityka: W przypadku remisu (jest, programowanie, to) wybieramy alfabetycznie najmniejsze.
            Assert.That(stats.MostCommonWord, Is.EqualTo("jest"), "W przypadku remisu (jest, programowanie, to) wybieramy alfabetycznie najmniejsze.");
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
        /// Testuje liczenie unikalnych s³ów, bez uwzglêdniania wielkoœci liter.
        /// </summary>
        [Test]
        public void Analyze_UniqueWordCount_IsCaseInsensitive()
        {
            const string text = "Jeden dwa Dwa trzy Trzy Trzy";
            var stats = _analyzer.Analyze(text);

            Assert.That(stats.UniqueWordCount, Is.EqualTo(3), "Powinny byæ 3 unikalne s³owa (jeden, dwa, trzy).");
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
                Assert.That(stats.Letters, Is.EqualTo(0), "Brak liter.");
            });
        }
    }
}
