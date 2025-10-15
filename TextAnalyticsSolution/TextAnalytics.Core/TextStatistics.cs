namespace TextAnalytics.Core
{
    public sealed record TextStatistics(
        // Liczba znaków
        int CharactersWithSpaces,
        int CharactersWithoutSpaces,
        int Letters,
        int Digits,
        int Punctuation,

        // Statystyki słów
        int WordCount,
        int UniqueWordCount,
        string MostCommonWord,
        double AverageWordLength,
        string LongestWord,
        string ShortestWord,

        // Statystyki zdań
        int SentenceCount,
        double AverageWordsPerSentence,
        string LongestSentence
    );
}