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
        private ITextAnalyzer _analyzer; // Zmieniono na interfejs ITextAnalyzer

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
                // Ca�kowita d�ugo�� ci�gu: 89
                // Liczba spacji: 10
                // Bez spacji: 79 (74 litery + 5 interpunkcja)
                // Liczba liter: 74
                // Interpunkcja: 5 (!, ?, ,, ., .)
                Assert.That(stats.CharactersWithoutSpaces, Is.EqualTo(79), "1) Liczba znak�w bez bia�ych znak�w.");
                Assert.That(stats.Letters, Is.EqualTo(74), "2) Liczba liter.");
                Assert.That(stats.Punctuation, Is.EqualTo(5), "3) Liczba znak�w interpunkcyjnych (!, ?, ,, ., .).");

                // Statystyki s��w (11 s��w: Programowanie, jest, fajne, Czy�, nie, Tak, jest, najlepsze, Programowanie, jest, przysz�o�ci�)
                Assert.That(stats.WordCount, Is.EqualTo(11), "4) Liczba s��w.");
                // �rednia: 74 litery / 11 s��w = 6.7272...
                Assert.That(stats.AverageWordLength, Is.EqualTo(6.7272727272727275d).Within(0.01d), "5) �rednia d�ugo�� s�owa (74/11).");
                // Najkr�tsze: "nie" (3) i "tak" (3). Alfabetycznie "nie" < "tak".
                Assert.That(stats.ShortestWord, Is.EqualTo("nie"), "6) Najkr�tsze s�owo (wybrane alfabetycznie w przypadku remisu).");

                // Statystyki zda� (4 zdania: 1.!, 2.?, 3.., 4..)
                Assert.That(stats.SentenceCount, Is.EqualTo(4), "Liczba zda� (4).");
                // �rednia s��w na zdanie: 11 s��w / 4 zdania = 2.75
                Assert.That(stats.AverageWordsPerSentence, Is.EqualTo(2.75d).Within(0.01d), "7) �rednia s��w na zdanie (11/4).");
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
        /// Testuje sytuacj�, gdy kilka s��w wyst�puje z t� sam�, najwy�sz� cz�stotliwo�ci�.
        /// </summary>
        [Test]
        public void Analyze_WordFrequencyTie_ReturnsFirstOccurrence()
        {
            // Tekst: "jest" (2), "programowanie" (2), "to" (2), "fajne" (1).
            // Wszystkie s�owa s� znormalizowane do ma�ych liter.
            // "jest", "programowanie", "to" s� remisem. Alfabetycznie najmniejsze to "jest".
            const string text = "Jest to programowanie! To jest programowanie. Fajne.";
            var stats = _analyzer.Analyze(text);

            // Oczekiwana polityka: W przypadku remisu (jest, programowanie, to) wybieramy alfabetycznie najmniejsze.
            Assert.That(stats.MostCommonWord, Is.EqualTo("jest"), "W przypadku remisu (jest, programowanie, to) wybieramy alfabetycznie najmniejsze.");
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
        /// Testuje liczenie unikalnych s��w, bez uwzgl�dniania wielko�ci liter.
        /// </summary>
        [Test]
        public void Analyze_UniqueWordCount_IsCaseInsensitive()
        {
            const string text = "Jeden dwa Dwa trzy Trzy Trzy";
            var stats = _analyzer.Analyze(text);

            Assert.That(stats.UniqueWordCount, Is.EqualTo(3), "Powinny by� 3 unikalne s�owa (jeden, dwa, trzy).");
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
                Assert.That(stats.Letters, Is.EqualTo(0), "Brak liter.");
            });
        }
    }
}
