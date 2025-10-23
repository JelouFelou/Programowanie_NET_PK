# 🧩 Zadanie z C#/.NET: System Zarządzania Biblioteką i Rezerwacjami (książki + e‑booki)

## 🎯 Cel

Zaprojektuj i zaimplementuj aplikację konsolową w **C# (.NET)** do zarządzania zasobami biblioteki (książki oraz e‑booki), użytkownikami i **rezerwacjami/wypożyczeniami**. Dodaj moduł analityczny (kompozycja) obliczający statystyki z wykorzystaniem operacji na kolekcjach.

---

## 📦 Wymagany zakres języka i technologii

* **Klasy, właściwości, enkapsulacja**
* **Dziedziczenie, polimorfizm**
* **Klasy abstrakcyjne**, **interfejsy**
* **Kompozycja** (moduł analityczny w oparciu o serwis biblioteki)
* **Wyjątki** (scenariusze błędne)
* **Delegaty i zdarzenia** (powiadomienia o nowych rezerwacjach/zwrotach)
* **Wyrażenia lambda + LINQ** (filtrowanie/wyszukiwanie)
* **Metody rozszerzające** (operacje pomocnicze na kolekcjach)
* **Testy jednostkowe** (xUnit lub NUnit)

---

## 🧱 Architektura i moduły

### 1) Model domenowy

**Abstrakcyjna klasa `LibraryItem`** – wspólna baza dla wszystkich zasobów biblioteki.

```csharp
public abstract class LibraryItem
{
    public int Id { get; }
    public string Title { get; protected set; }
    public bool IsAvailable { get; protected set; } = true;

    protected LibraryItem(int id, string title)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    public abstract void DisplayInfo();
}
```

**`Book` : `LibraryItem`**
Właściwości: `Author`, `Isbn`. Nadpisuje `DisplayInfo()`.

**`EBook` : `Book`**
Dodatkowo: `FileFormat` (np. PDF/EPUB). Nadpisuje `DisplayInfo()` (dodaje format).

---

### 2) Rezerwacje i interfejs

**Interfejs `IReservable`** – wspólne operacje rezerwacji.

```csharp
public interface IReservable
{
    void Reserve(string userEmail, DateTime from, DateTime to);
    void CancelReservation(string userEmail);
    bool IsAvailable();
}
```

**Klasa `Reservation`**

* `LibraryItem Item`
* `string UserEmail`
* `DateTime From`, `DateTime To`
* `bool IsActive`

Walidacje: `From < To`, item dostępny, brak konfliktu z aktywną rezerwacją.

---

### 3) Serwisy (kompozycja i logika)

**`LibraryService`** – agreguje stan i logikę.

* Kolekcje: `List<LibraryItem> _items`, `List<Reservation> _reservations`, `List<string> _users`
* Operacje:

  * `AddItem(LibraryItem item)`
  * `RegisterUser(string email)`
  * `IEnumerable<LibraryItem> ListAvailableItems()` (LINQ)
  * `Reservation CreateReservation(int itemId, string userEmail, DateTime from, DateTime to)`
  * `void CancelReservation(int reservationId)`
  * `IEnumerable<Reservation> GetUserReservations(string userEmail)`
* **Zdarzenia**:

  * `public event Action<Reservation> OnNewReservation;`
  * `public event Action<Reservation> OnReservationCancelled;`
* **Wyjątki**:

  * `InvalidOperationException` – rezerwacja niedostępnej pozycji
  * `ArgumentException` – błędne parametry
  * (własny) `ReservationConflictException` – kolizja terminów

**`AnalyticsService`** (kompozycja – przyjmuje `LibraryService` w konstruktorze):

* `double AverageLoanLengthDays()` – średnia liczba dni rezerwacji
* `int TotalLoans()`
* `string MostPopularItemTitle()` – najczęściej wypożyczany tytuł
* `double FulfillmentRate()` – odsetek zrealizowanych rezerwacji (nieanulowanych)
* (opcjonalnie) `double LogPopularityScore(string title)` – przykład funkcji „naukowej” z bezpieczną obsługą domeny

---

### 4) Metody rozszerzające

```csharp
public static class LibraryExtensions
{
    public static IEnumerable<T> Available<T>(this IEnumerable<T> items) where T : LibraryItem
        => items.Where(i => i.IsAvailable);

    public static IEnumerable<LibraryItem> Newest(this IEnumerable<LibraryItem> items, int take)
        => items.OrderByDescending(i => i.Id).Take(take);
}
```

---

### 5) Interfejs konsolowy (`Program`)

Proponowane menu:

```
1. Dodaj książkę / e-booka
2. Zarejestruj użytkownika
3. Pokaż dostępne pozycje (filtry: fraza w tytule/autor – lambda)
4. Zarezerwuj pozycję
5. Anuluj rezerwację
6. Moje rezerwacje
7. Statystyki (średnia długość, liczba wypożyczeń, najpopularniejszy tytuł)
0. Wyjście
```

Wymagania UI:

* Po utworzeniu rezerwacji wydruk komunikatu ze **zdarzenia** `OnNewReservation`.
* Staranna obsługa **wyjątków**.

---

## 🧪 Testy jednostkowe (xUnit/NUnit)

Minimalny zakres:

* **Model/Reguły**: poprawne tworzenie `Book/EBook`, polimorficzne `DisplayInfo()`.
* **LibraryService**:

  * Dodawanie pozycji i użytkowników.
  * Rezerwacja dostępnej pozycji vs. rezerwacja niedostępnej (wyjątek).
  * Kolizja terminów → `ReservationConflictException`.
  * Anulowanie rezerwacji i emisja `OnReservationCancelled`.
* **AnalyticsService**:

  * `AverageLoanLengthDays()` dla pustych i niepustych danych.
  * `MostPopularItemTitle()` – poprawny tytuł przy remisie i bez danych.
  * Funkcje „naukowe” – walidacja domeny (np. logarytm tylko dla dodatnich).
* **Metody rozszerzające**: `Available()` i `Newest()` działają poprawnie.

---

## ✅ Kryteria oceny

* Zastosowano **wszystkie** wymagane elementy C# (OOP, abstrakcja, interfejsy, polimorfizm, kompozycja, zdarzenia, wyjątki, lambda/LINQ, metody rozszerzające, testy).
* Spójna architektura i czytelny podział odpowiedzialności.
* Solidna obsługa błędów i walidacja.
* Testy obejmują scenariusze sukcesu i porażki.

---

## 🗂️ Proponowany układ projektu

```
LibraryApp/
  src/
    Domain/
      LibraryItem.cs
      Book.cs
      EBook.cs
      Reservation.cs
      IReservable.cs
    Services/
      LibraryService.cs
      AnalyticsService.cs
    Extensions/
      LibraryExtensions.cs
    Program.cs
  tests/
    LibraryApp.Tests/
      LibraryServiceTests.cs
      AnalyticsServiceTests.cs
      ExtensionsTests.cs
```



## 💻 Przykładowy plik `Program.cs`

```csharp
using System;
using LibraryApp.Services;
using LibraryApp.Domain;

class Program
{
    static void Main()
    {
        var library = new LibraryService();
        var analytics = new AnalyticsService(library);

        library.OnNewReservation += r => Console.WriteLine($"[INFO] Nowa rezerwacja: {r.Item.Title} dla {r.UserEmail}");

        while (true)
        {
            Console.WriteLine("\n=== System Biblioteczny ===");
            Console.WriteLine("1. Dodaj książkę");
            Console.WriteLine("2. Dodaj e-booka");
            Console.WriteLine("3. Zarejestruj użytkownika");
            Console.WriteLine("4. Zarezerwuj pozycję");
            Console.WriteLine("5. Pokaż dostępne pozycje");
            Console.WriteLine("6. Pokaż statystyki");
            Console.WriteLine("0. Wyjście");
            Console.Write("> ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.Write("Tytuł: "); var title = Console.ReadLine();
                    Console.Write("Autor: "); var author = Console.ReadLine();
                    Console.Write("ISBN: "); var isbn = Console.ReadLine();
                    library.AddItem(new Book(library.NextId(), title, author, isbn));
                    Console.WriteLine("Dodano książkę.");
                    break;
                case "2":
                    Console.Write("Tytuł: "); var t = Console.ReadLine();
                    Console.Write("Autor: "); var a = Console.ReadLine();
                    Console.Write("ISBN: "); var i = Console.ReadLine();
                    Console.Write("Format: "); var f = Console.ReadLine();
                    library.AddItem(new EBook(library.NextId(), t, a, i, f));
                    Console.WriteLine("Dodano e-booka.");
                    break;
                case "3":
                    Console.Write("Email użytkownika: "); var email = Console.ReadLine();
                    library.RegisterUser(email);
                    Console.WriteLine("Zarejestrowano użytkownika.");
                    break;
                case "4":
                    Console.Write("ID pozycji: "); int id = int.Parse(Console.ReadLine());
                    Console.Write("Email: "); var u = Console.ReadLine();
                    library.CreateReservation(id, u, DateTime.Now, DateTime.Now.AddDays(7));
                    break;
                case "5":
                    foreach (var item in library.ListAvailableItems())
                        item.DisplayInfo();
                    break;
                case "6":
                    Console.WriteLine($"Średni czas wypożyczenia: {analytics.AverageLoanLengthDays():F2} dni");
                    Console.WriteLine($"Najpopularniejszy tytuł: {analytics.MostPopularItemTitle()}");
                    Console.WriteLine($"Łączna liczba rezerwacji: {analytics.TotalLoans()}");
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Nieznana opcja.");
                    break;
            }
        }
    }
}
```
