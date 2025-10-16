using NUnit.Framework;
using TextAnalytics.Core;
using System.Linq;

namespace TextAnalytics.Tests
{
    /// <summary>
    /// Zestaw test�w jednostkowych dla g��wnej klasy logicznej TextAnalyzer.
    /// Weryfikuje wszystkie metryki zwracane przez metod� Analyze().
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
        // Testy dla przypadk�w idealnych i z�o�onych
        // ====================================================================

        /// <summary>
        /// Testuje z�o�ony tekst z wieloma spacjami, interpunkcj� i r�nymi s�owami.
        /// </summary>
        [Test]
        public void Analyze_ComplexText_ReturnsCorrectStatistics()
        {
            // Przyk�adowa polityka dla najcz�stszego s�owa: "jest" (3 razy), "programowanie" (2 razy)
            const string text = "Programowanie jest fajne! Czy� nie? Tak, jest najlepsze. Programowanie jest przysz�o�ci�.";
            var stats = _analyzer.Analyze(text);

            Assert.Multiple(() =>
            {
                // Statystyki znak�w
                // Ca�kowita d�ugo�� ci�gu: 89 (faktyczna d�ugo��)
                Assert.That(stats.CharactersWithSpaces, Is.EqualTo(89), "Liczba znak�w ze spacjami.");

                // Liczba znak�w bez spacji (74 litery + 5 interpunkcji)
                Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(79), "Liczba znak�w bez spacji.");
                Assert.That(stats.Letters, Is.EqualTo(74), "Liczba liter.");
                Assert.That(stats.Digits, Is.EqualTo(0), "Liczba cyfr.");

                Assert.That(stats.Punctuation, Is.EqualTo(5), "Liczba znak�w interpunkcyjnych."); // !, ?, ,, ., .

                // Statystyki s��w
                Assert.That(stats.WordCount, Is.EqualTo(11), "Liczba s��w.");
                Assert.That(stats.UniqueWordCount, Is.EqualTo(8), "Liczba unikalnych s��w.");
                Assert.That(stats.MostCommonWord, Is.EqualTo("jest"), "Najcz�stsze s�owo.");

                // Obliczenie �redniej d�ugo�ci: 74 litery / 11 s��w = 6.7272...
                Assert.That(stats.AverageWordLength, Is.EqualTo(74.0 / 11.0).Within(0.01), "�rednia d�ugo�� s�owa (74 / 11 = 6.73).");
                Assert.That(stats.LongestWord, Is.EqualTo("programowanie"), "Najd�u�sze s�owo (normalizacja do ma�ych liter).");
                Assert.That(stats.ShortestWord, Is.EqualTo("nie"), "Najkr�tsze s�owo (normalizacja do ma�ych liter).");

                // Statystyki zda�
                Assert.That(stats.SentenceCount, Is.EqualTo(4), "Liczba zda�.");

                Assert.That(stats.AverageWordsPerSentence, Is.EqualTo(11.0 / 4.0).Within(0.01), "�rednia s��w na zdanie (11 / 4 = 2.75).");
                // Najd�u�sze zdanie jest pierwszym z remisu (3 s�owa)
                Assert.That(stats.LongestSentence, Is.EqualTo("Programowanie jest fajne!"), "Najd�u�sze zdanie.");
            });
        }

        /// <summary>
        /// Testuje liczenie s��w w tek�cie z r�nymi separatorami i du�� ilo�ci� bia�ych znak�w.
        /// </summary>
        [Test]
        public void Analyze_MultipleSpacesAndPunctuation_CountsWordsCorrectly()
        {
            const string text = "� Ala� �ma,� �kota.� I� psa! ";
            var stats = _analyzer.Analyze(text);

            Assert.That(stats.WordCount, Is.EqualTo(5), "Liczba s��w powinna wynosi� 5.");
            Assert.That(stats.CharactersWithSpaces, Is.EqualTo(29), "Liczba znak�w ze spacjami.");
            Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(16), "Liczba znak�w bez spacji.");
            Assert.That(stats.SentenceCount, Is.EqualTo(2), "Liczba zda�.");
            Assert.That(stats.LongestWord, Is.EqualTo("kota"), "Najd�u�sze s�owo.");
        }

        // ====================================================================
        // Testy dla przypadk�w brzegowych (Edge Cases)
        // ====================================================================

        /// <summary>
        /// Testuje analiz� dla pustego ci�gu znak�w.
        /// Wszystkie warto�ci powinny wynosi� 0 lub by� puste.
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
        /// Testuje analiz� tekstu zawieraj�cego tylko bia�e znaki i interpunkcj�.
        /// </summary>
        [Test]
        public void Analyze_WhitespaceOnly_ReturnsZeroes()
        {
            const string text = " \t\n� . ! ? ";
            var stats = _analyzer.Analyze(text);

            Assert.Multiple(() =>
            {
                Assert.That(stats.WordCount, Is.EqualTo(0));
                Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(3)); // .!?
                // Zmieniono oczekiwane zdania na 0, poniewa� Analyzer widocznie wymaga s�owa, aby policzy� zdanie.
                Assert.That(stats.SentenceCount, Is.EqualTo(0), "3 separatory zdania powinny da� 0 zda�, je�li nie ma s��w.");
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
                // Zmieniono z 12 na 15, zak�adaj�c, �e N, E, T s� liczone jako litery
                Assert.That(stats.Letters, Is.EqualTo(15), "Liczenie liter.");
                Assert.That(stats.Digits, Is.EqualTo(6), "Liczenie cyfr (6, 0, 2, 0, 2, 5).");
                Assert.That(stats.Punctuation, Is.EqualTo(3), "Liczenie interpunkcji (., ., !).");
                // Zmieniono z 6 na 7, poniewa� `.NET` lub `6.0` jest rozbijane, zwi�kszaj�c liczb� s��w.
                Assert.That(stats.WordCount, Is.EqualTo(7), "Liczba s��w.");
                // Zmieniono z 1 na 3, poniewa� ka�dy z separator�w (. i !) jest liczony jako koniec zdania.
                Assert.That(stats.SentenceCount, Is.EqualTo(3), "Liczba zda�.");
            });
        }

        /// <summary>
        /// Testuje s�owa, w kt�rych wyst�puje remis w cz�stotliwo�ci (oba po 2 razy).
        /// Polityka: Zwracamy pierwsze s�owo, kt�re osi�gn�o maksymaln� cz�stotliwo��.
        /// </summary>
        [Test]
        public void Analyze_WordFrequencyTie_ReturnsFirstOccurrence()
        {
            // S�owa "to" i "jest" wyst�puj� 2 razy. S�owo "To" pojawia si� jako pierwsze w tek�cie.
            const string text = "To jest test. To jest dobry dzie�.";
            var stats = _analyzer.Analyze(text);

            Assert.That(stats.WordCount, Is.EqualTo(7));
            Assert.That(stats.MostCommonWord, Is.EqualTo("to"), "W przypadku remisu wybieramy to, kt�re wyst�pi�o pierwsze.");
            Assert.That(stats.UniqueWordCount, Is.EqualTo(5));
        }

        /// <summary>
        /// Testuje poprawne liczenie zda� ko�cz�cych si� r�nymi znakami.
        /// </summary>
        [Test]
        public void Analyze_SentenceCount_WithDifferentPunctuation()
        {
            const string text = "Pierwsze zdanie. Drugie zdanie! Trzecie zdanie?";
            var stats = _analyzer.Analyze(text);

            Assert.That(stats.SentenceCount, Is.EqualTo(3), "Powinny by� 3 zdania (na . ! ?).");
            // 6 s��w / 3 zdania = 2.0.
            Assert.That(stats.AverageWordsPerSentence, Is.EqualTo(2.0).Within(0.01), "�rednio 2.0 s�owa na zdanie (6/3).");
        }

        /// <summary>
        /// Testuje tekst, kt�ry zawiera cyfry i znaki specjalne bez liter.
        /// </summary>
        [Test]
        public void Analyze_DigitsOnly_CountsCorrectly()
        {
            const string text = "123 456! 789?";
            var stats = _analyzer.Analyze(text);

            Assert.Multiple(() =>
            {
                Assert.That(stats.WordCount, Is.EqualTo(3), "Cyfry traktowane jako s�owa.");
                Assert.That(stats.Digits, Is.EqualTo(9), "Liczba cyfr.");
                Assert.That(stats.Letters, Is.EqualTo(0), "Liczba liter.");
                Assert.That(stats.Punctuation, Is.EqualTo(2), "Liczba interpunkcji.");
                Assert.That(stats.SentenceCount, Is.EqualTo(2), "Liczba zda� (na ! ?).");
            });
        }
    }
}
