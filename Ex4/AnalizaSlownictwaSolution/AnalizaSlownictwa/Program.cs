using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

var globalWordCounts = new ConcurrentDictionary<string, int>();

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
    var downloadStopwatch = Stopwatch.StartNew();

    Console.WriteLine("--- Start pobierania asynchronicznego ---");

    // 1. Stworzenie listy zadań (Tasks) do pobierania
    var downloadTasks = bookUrls.Select(url => DownloadBookAsync(url)).ToList();

    // 2. Oczekiwanie na ukończenie WSZYSTKICH zadań naraz (Task.WhenAll)
    string[] allBookContents = await Task.WhenAll(downloadTasks);

    downloadStopwatch.Stop();
    Console.WriteLine("--- Koniec pobierania ---");
    
    
    // B. Przetwarzanie danych
    var processStopwatch = Stopwatch.StartNew();

    Console.WriteLine("\n--- Start przetwarzania równoległego ---");

    // Użycie Parallel.ForEach do rozdzielenia pracy na wątki
    // allBookContents to tablica z treścią 4 książek
    Parallel.ForEach(allBookContents, content =>
    {
        // Każdy wątek wywołuje ProcessText dla swojej części (jednej książki)
        // i zapisuje wyniki do współdzielonego globalWordCounts.
        ProcessText(content, globalWordCounts);
    });

    processStopwatch.Stop();
    Console.WriteLine("--- Koniec przetwarzania ---");


    // C. Generowanie raportu
    Console.WriteLine("\n--- Raport końcowy ---");
    Console.WriteLine("Najczęstsze słowa:");

    // 1. Użycie LINQ do posortowania słownika malejąco według wartości (liczby wystąpień)
    // 2. Pobranie tylko 10 pierwszych wyników
    var top10Words = globalWordCounts
        .OrderByDescending(pair => pair.Value)
        .Take(10);

    int rank = 1;
    foreach (var item in top10Words)
    {
        Console.WriteLine($"{rank++}. {item.Key}: {item.Value}");
    }

    Console.WriteLine($"\nCzas pobierania: {downloadStopwatch.Elapsed.TotalSeconds:F2} sekundy");
    Console.WriteLine($"Czas przetwarzania: {processStopwatch.Elapsed.TotalSeconds:F2} sekundy");

    // Dodanie pauzy
    Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
    Console.ReadKey();

}

// 3. Metoda do pobierania pojedynczego pliku
async Task<string> DownloadBookAsync(string url)
{
    using var client = new HttpClient();
    Console.WriteLine($"Pobieranie: {url}...");
    try
    {
        // Użycie GetStringAsync do pobrania zawartości jako string
        string content = await client.GetStringAsync(url);
        Console.WriteLine($"Pobrano: {url}. Długość: {content.Length} znaków.");
        return content;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Błąd podczas pobierania {url}: {ex.Message}");
        return string.Empty; // Zwróć pusty string w przypadku błędu
    }
}

// 4. Metoda do przetwarzania tekstu
void ProcessText(string text, ConcurrentDictionary<string, int> wordCounts)
{
    // Wyczyść tekst z nagłówków/stopek
    string cleanedText = CleanText(text);

    // Zdefiniowanie separatorów, czyli znaków, które chcemy traktować jako granice słów
    char[] separators = new char[] { ' ', '\r', '\n', '.', ',', '!', '?', ':', ';', '"', '\'', '-', '(', ')', '[', ']', '/' };

    // Podział tekstu na słowa
    // StringSplitOptions.RemoveEmptyEntries usuwa puste stringi powstałe po wielokrotnych separatorach
    string[] words = cleanedText.Split(separators, StringSplitOptions.RemoveEmptyEntries);

    foreach (string word in words)
    {
        // Normalizacja: małe litery, bez uwzględniania inwariantnych różnic kulturowych
        string normalizedWord = word.ToLowerInvariant();

        // Pomijanie bardzo krótkich (jednoliterowych) "słów", które często są śmieciami po czyszczeniu
        if (normalizedWord.Length <= 1)
        {
            continue;
        }

        // Atomowa operacja:
        // TryUpdate / Add - bezpieczne w ConcurrentDictionary.
        // GetOrAdd jest najwygodniejsze: pobierz wartość, a jeśli nie istnieje, dodaj 0 i natychmiast ją zaktualizuj.

        wordCounts.AddOrUpdate(
            normalizedWord,
            1, // Jeśli słowo nie istnieje (factory), dodaj 1
            (key, oldValue) => oldValue + 1 // Jeśli słowo istnieje (update), zwiększ wartość
        );
    }
}

// 5. Metoda pomocnicza do czyszczenia tekstu (dla ambitnych: odcinanie nagłówka/stopki)
string CleanText(string text)
{
    // Prosta heurystyka: Project Gutenberg zazwyczaj zaczyna treść po "START OF THE PROJECT GUTENBERG"
    // i kończy przed "END OF THE PROJECT GUTENBERG"
    int startIndex = text.IndexOf("*** START OF THE PROJECT GUTENBERG");
    if (startIndex != -1)
    {
        // Przesunięcie, aby zacząć od miejsca po markerze
        text = text.Substring(startIndex + "*** START OF THE PROJECT GUTENBERG".Length);
    }

    int endIndex = text.IndexOf("*** END OF THE PROJECT GUTENBERG");
    if (endIndex != -1)
    {
        text = text.Substring(0, endIndex);
    }

    return text;
}