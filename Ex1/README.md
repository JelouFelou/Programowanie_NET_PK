# Zadanie łączone: **MultiProject + Analiza tekstu + NuGet + DI + NUnit**

## Cel zadania

* Zbudowanie **wieloprojektowego** rozwiązania .NET z poprawnymi **referencjami** między projektami.
* Implementacja **analizy tekstu** jako biblioteki wielokrotnego użytku.
* Wykorzystanie **pakietów NuGet** (np. `Newtonsoft.Json`, `Microsoft.Extensions.DependencyInjection`).
* Zastosowanie **Dependency Injection** do spięcia warstw i usług.
* Napisanie **testów jednostkowych w NUnit** dla kluczowych funkcji.

---

## Część 1: Struktura rozwiązania i referencje między projektami

1. **Utwórz Solution** o nazwie `TextAnalyticsSolution`.
2. Dodaj projekty:

   * **Class Library** `TextAnalytics.Core` – logika analizy tekstu (model + algorytmy).
   * **Class Library** `TextAnalytics.Services` – usługi (np. logger, dostawca danych).
   * **Console Application** `TextAnalytics.App` – interfejs użytkownika (CLI).
   * **NUnit Test Project** `TextAnalytics.Tests` – testy jednostkowe.
3. **Referencje**:

   * `TextAnalytics.App` ➜ referencje do `TextAnalytics.Core` i `TextAnalytics.Services`.
   * `TextAnalytics.Tests` ➜ referencja do `TextAnalytics.Core` (i ewentualnie `TextAnalytics.Services`, jeśli testujesz usługi).

---

## Część 2: Biblioteka analizy tekstu (`TextAnalytics.Core`)

### Wymagania funkcjonalne

Zaimplementuj klasę/fasadę `TextAnalyzer` (lub zestaw współpracujących klas) realizującą co najmniej:

* **Liczba znaków (ze spacjami) / (bez spacji)**
* **Liczba liter**, **liczba cyfr**, **liczba znaków interpunkcyjnych**
* **Liczba słów**, **liczba unikalnych słów**, **najczęstsze słowo**
* **Średnia długość słowa**, **najdłuższe / najkrótsze słowo**
* **Liczba zdań** (kończone `.` `!` `?`), **średnia liczba słów na zdanie**, **najdłuższe zdanie (słowa)**

Zaprojektuj model wyników, np. rekord `TextStatistics` z właściwościami jak wyżej.

### API przykładowe

```csharp
public sealed class TextAnalyzer
{
    public TextStatistics Analyze(string text);
    public int CountCharacters(string text, bool includeSpaces = true);
    public int CountWords(string text);
    // … inne metody pomocnicze
}

public sealed record TextStatistics(
    int CharactersWithSpaces,
    int CharactersWithoutSpaces,
    int Letters,
    int Digits,
    int Punctuation,
    int WordCount,
    int UniqueWordCount,
    string MostCommonWord,
    double AverageWordLength,
    string LongestWord,
    string ShortestWord,
    int SentenceCount,
    double AverageWordsPerSentence,
    string LongestSentence
);
```

---

## Część 3: Usługi i DI (`TextAnalytics.Services`)

Zaimplementuj co najmniej dwie usługi:

* `ILoggerService` z implementacją `ConsoleLogger` (logowanie zdarzeń uruchomienia, błędów, podsumowań).
* `IInputProvider` z implementacjami np. `ConsoleInputProvider` (wczytanie z klawiatury) oraz `FileInputProvider` (wczytanie z pliku).

Dodaj integrację DI:

```csharp
services
  .AddSingleton<ILoggerService, ConsoleLogger>()
  .AddSingleton<IInputProvider, ConsoleInputProvider>()
  .AddSingleton<TextAnalyzer>();
```

---

## Część 4: Aplikacja konsolowa (`TextAnalytics.App`)

### Wymagania I/O

* Program powinien przyjąć **źródło danych**:

  1. tekst z klawiatury, 2) ścieżkę do pliku, 3) ścieżkę przekazaną jako **argument CLI**.
* Obsłuż błędy: brak pliku, pusta zawartość, niepoprawna ścieżka.

### Prezentacja wyników

* Czytelny wynik w konsoli (sekcje/statystyki w kolumnach).
* Dodatkowo: **zapis wyników do JSON** (np. `results.json`) przy użyciu `Newtonsoft.Json`.

Przykład (fragment `Program.cs`):

```csharp
var services = new ServiceCollection()
    .AddSingleton<ILoggerService, ConsoleLogger>()
    .AddSingleton<IInputProvider, ConsoleInputProvider>()
    .AddSingleton<TextAnalyzer>()
    .BuildServiceProvider();

var logger = services.GetRequiredService<ILoggerService>();
var input = services.GetRequiredService<IInputProvider>();
var analyzer = services.GetRequiredService<TextAnalyzer>();

logger.Log("Aplikacja uruchomiona.");
var text = input.Read();
var stats = analyzer.Analyze(text);

Console.WriteLine($"Słowa: {stats.WordCount}, Unikalne: {stats.UniqueWordCount}");
// … wypisz resztę statystyk

var json = JsonConvert.SerializeObject(stats, Formatting.Indented);
File.WriteAllText("results.json", json);
logger.Log("Wyniki zapisane do results.json");
```

---

## Część 5: NuGet i konfiguracja

* Zainstaluj w odpowiednich projektach:

  * `Newtonsoft.Json` (serializacja wyników w `TextAnalytics.App`).
  * `Microsoft.Extensions.DependencyInjection` (DI w `TextAnalytics.App`).

---

## Część 6: Testy jednostkowe (NUnit) – `TextAnalytics.Tests`

* Utwórz testy dla kluczowych metod `TextAnalyzer` (min. **8 testów**), w tym przypadki brzegowe:

  * pusty tekst / tylko białe znaki,
  * wielokrotne spacje i interpunkcja,
  * częstotliwość słów (remis – zdefiniuj politykę: pierwsze z max. freq., lub alfabet).

Przykład szkicu testu:

```csharp
[Test]
public void CountWords_Returns2_ForHelloWorld()
{
    var a = new TextAnalyzer();
    Assert.That(a.CountWords("Hello world!"), Is.EqualTo(2));
}
```

Uruchamianie: `dotnet test` lub z Test Explorer.

---

## Część 7: Zarządzanie wersjami i aktualizacje

* Zademonstruj aktualizację wersji pakietu (np. `Newtonsoft.Json`) oraz odświeżenie zależności.
* Dodaj krótką notatkę o **SemVer** i wpływie aktualizacji na kompatybilność API.

---

## Struktura repo (propozycja)

```
TextAnalyticsSolution/
  src/
    TextAnalytics.Core/
    TextAnalytics.Services/
    TextAnalytics.App/
  tests/
    TextAnalytics.Tests/
  README.md
```

## Jak uruchomić

```bash
# build
dotnet build

# aplikacja (z plikiem)
dotnet run --project src/TextAnalytics.App -- --file "sample.txt"

# aplikacja (z wejścia z klawiatury)
dotnet run --project src/TextAnalytics.App -- --interactive

# testy
dotnet test
```
