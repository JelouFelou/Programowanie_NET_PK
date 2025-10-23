namespace TextAnalytics.Services
{
    /// <summary>
    /// Definiuje interfejs dla dostawcy tekstu do analizy.
    /// Umożliwia wstrzykiwanie różnych źródeł danych (konsola, plik, sieć).
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// Wczytuje tekst zgodnie z logiką implementacji.
        /// </summary>
        /// <returns>Pobrany tekst lub pusty ciąg, jeśli wystąpi błąd.</returns>
        string GetText();
    }
}