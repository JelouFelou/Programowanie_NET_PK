using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

// 1. Zdefiniowanie stałych dla adresów URL
string[] bookUrls = new string[]
{
    "https://www.gutenberg.org/files/84/84-0.txt",     // Frankenstein
    "https://www.gutenberg.org/files/11/11-0.txt",    // Alicja w Krainie Czarów
    "https://www.gutenberg.org/files/1661/1661-0.txt",  // Przygody Sherlocka Holmesa
    "https://www.gutenberg.org/files/2701/2701-0.txt"   // Moby Dick
};

// 2. Główna asynchroniczna metoda startowa
await RunWordAnalysis();

async Task RunWordAnalysis()
{
    // A. Pobieranie danych
    // B. Przetwarzanie danych
    // C. Generowanie raportu
}

// 3. Metoda do pobierania pojedynczego pliku
async Task<string> DownloadBookAsync(string url)
{
    return "";
}

// 4. Metoda do przetwarzania tekstu
void ProcessText(string text, ConcurrentDictionary<string, int> wordCounts)
{
}

// 5. Metoda pomocnicza do czyszczenia tekstu (dla ambitnych: odcinanie nagłówka/stopki)
string CleanText(string text)
{
    return text;
}