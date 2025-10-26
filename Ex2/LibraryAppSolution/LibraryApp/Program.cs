using System;
using System.Linq;
using LibraryApp.Services;
using LibraryApp.Domain;
using LibraryApp.Extensions; // Import dla metod rozszerzających

class Program
{
    static void Main()
    {
        // Inicjalizacja serwisów
        var library = new LibraryService();
        var analytics = new AnalyticsService(library);

        // Subskrypcja zdarzenia (Delegat i Zdarzenie)
        library.OnNewReservation += r => Console.WriteLine($"\n✅ [INFO] Nowa rezerwacja: {r.Item.Title} (ID: {r.Item.Id}) dla {r.UserEmail}");
        library.OnReservationCancelled += r => Console.WriteLine($"\n❌ [INFO] Anulowano rezerwację: {r.Item.Title} (ID: {r.Item.Id}) dla {r.UserEmail}");

        // Wstępne dane dla łatwiejszego testowania
        SetupInitialData(library);

        while (true)
        {
            Console.WriteLine("\n\n=== SYSTEM ZARZĄDZANIA BIBLIOTEKĄ ===");
            Console.WriteLine("1. Dodaj pozycję (książka/e-book)");
            Console.WriteLine("2. Zarejestruj użytkownika");
            Console.WriteLine("3. Pokaż dostępne pozycje (wyszukiwanie/filtry)");
            Console.WriteLine("4. Zarezerwuj pozycję");
            Console.WriteLine("5. Anuluj rezerwację");
            Console.WriteLine("6. Moje rezerwacje");
            Console.WriteLine("7. Statystyki");
            Console.WriteLine("0. Wyjście");
            Console.Write("\nWybierz opcję: ");

            var choice = Console.ReadLine();
            try
            {
                switch (choice)
                {
                    case "1":
                        HandleAddItem(library);
                        break;
                    case "2":
                        HandleRegisterUser(library);
                        break;
                    case "3":
                        HandleListAvailable(library);
                        break;
                    case "4":
                        HandleCreateReservation(library);
                        break;
                    case "5":
                        HandleCancelReservation(library);
                        break;
                    case "6":
                        HandleMyReservations(library);
                        break;
                    case "7":
                        HandleStatistics(analytics);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("\n[BŁĄD] Nieznana opcja. Spróbuj ponownie.");
                        break;
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is ReservationConflictException)
            {
                // Obsługa wyjątków biznesowych (ArgumentException, InvalidOperationException, ReservationConflictException)
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[BŁĄD ZASAD BIZNESOWYCH] {ex.Message}");
                Console.ResetColor();
            }
            catch (FormatException)
            {
                // Obsługa wyjątków wejścia (np. wpisanie tekstu zamiast ID)
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[BŁĄD WEJŚCIA] Wprowadzono nieprawidłowy format. Sprawdź, czy używasz liczb tam, gdzie to wymagane.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                // Ogólna obsługa nieprzewidzianych wyjątków
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\n[NIEPRZEWIDZIANY BŁĄD] {ex.Message}");
                Console.ResetColor();
            }
        }
    }

    // --- Metody pomocnicze dla interfejsu ---

    static void SetupInitialData(LibraryService library)
    {
        // Rejestracja użytkowników
        library.RegisterUser("user1@example.com");
        library.RegisterUser("user2@example.com");

        // Dodawanie pozycji
        library.AddItem(new Book(library.NextId(), "Władca Pierścieni", "J.R.R. Tolkien", "978-8381165596"));
        library.AddItem(new EBook(library.NextId(), "Czysty Kod", "Robert C. Martin", "978-8328320091", "PDF"));
        library.AddItem(new Book(library.NextId(), "Wiedźmin: Krew Elfów", "Andrzej Sapkowski", "978-8375780662"));
        library.AddItem(new EBook(library.NextId(), "Wiedźmin: Czas Pogardy", "Andrzej Sapkowski", "978-8375780839", "EPUB"));
        library.AddItem(new Book(library.NextId(), "Zbrodnia i Kara", "Fiodor Dostojewski", "978-8377793442"));

        // Przykładowa rezerwacja (żeby statystyki nie były puste)
        library.CreateReservation(1, "user1@example.com", DateTime.Now, DateTime.Now.AddDays(14));
        // Anulowana rezerwacja
        var rId = library.CreateReservation(3, "user2@example.com", DateTime.Now.AddDays(1), DateTime.Now.AddDays(15));
        library.CancelReservation(rId.Id);
    }

    static void HandleAddItem(LibraryService library)
    {
        Console.Write("Typ (1-Książka, 2-E-book): ");
        var typeChoice = Console.ReadLine();
        Console.Write("Tytuł: "); var title = Console.ReadLine();
        Console.Write("Autor: "); var author = Console.ReadLine();
        Console.Write("ISBN: "); var isbn = Console.ReadLine();

        switch (typeChoice)
        {
            case "1":
                library.AddItem(new Book(library.NextId(), title, author, isbn));
                Console.WriteLine("\n✅ Dodano nową książkę.");
                break;
            case "2":
                Console.Write("Format (np. PDF/EPUB): "); var format = Console.ReadLine();
                library.AddItem(new EBook(library.NextId(), title, author, isbn, format));
                Console.WriteLine("\n✅ Dodano nowego e-booka.");
                break;
            default:
                Console.WriteLine("\n[BŁĄD] Nieznany typ pozycji.");
                break;
        }
    }

    static void HandleRegisterUser(LibraryService library)
    {
        Console.Write("Email użytkownika: ");
        var email = Console.ReadLine();
        library.RegisterUser(email);
        Console.WriteLine($"\n✅ Zarejestrowano użytkownika: {email}");
    }

    static void HandleListAvailable(LibraryService library)
    {
        Console.Write("Filtruj (Tytuł/Autor lub Enter dla wszystkich): ");
        var filter = Console.ReadLine();

        // Użycie LINQ + Metody Rozszerzającej
        var availableItems = library.ListAvailableItems(filter).ToList();

        if (!availableItems.Any())
        {
            Console.WriteLine("\nBrak dostępnych pozycji spełniających kryteria.");
            return;
        }

        Console.WriteLine("\n--- Dostępne Pozycje ---");
        // Metoda rozszerzająca 'Newest' (opcjonalnie)
        foreach (var item in availableItems.Newest(10))
        {
            // Polimorfizm: wywołanie odpowiedniego DisplayInfo()
            item.DisplayInfo();
        }
        Console.WriteLine($"\nŁącznie: {availableItems.Count} pozycji.");
    }

    static void HandleCreateReservation(LibraryService library)
    {
        Console.Write("ID pozycji do rezerwacji: ");
        int itemId = int.Parse(Console.ReadLine());
        Console.Write("Email użytkownika: ");
        var userEmail = Console.ReadLine();

        // Uproszczone daty: teraz + 7 dni
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(7);

        // Wywołanie logiki biznesowej z obsługą wyjątków (try-catch na zewnątrz)
        library.CreateReservation(itemId, userEmail, from, to);
    }

    static void HandleCancelReservation(LibraryService library)
    {
        Console.Write("ID rezerwacji do anulowania: ");
        int reservationId = int.Parse(Console.ReadLine());

        // Wywołanie logiki biznesowej z obsługą wyjątków (try-catch na zewnątrz)
        library.CancelReservation(reservationId);
    }

    static void HandleMyReservations(LibraryService library)
    {
        Console.Write("Email użytkownika: ");
        var userEmail = Console.ReadLine();

        var reservations = library.GetUserReservations(userEmail).ToList();

        if (!reservations.Any())
        {
            Console.WriteLine($"\nBrak rezerwacji dla {userEmail}.");
            return;
        }

        Console.WriteLine($"\n--- Rezerwacje dla {userEmail} ---");
        foreach (var r in reservations)
        {
            Console.WriteLine($"- ID: {r.Id}, Tytuł: {r.Item.Title}, Od: {r.From:d} Do: {r.To:d}, Aktywna: {r.IsActive}");
        }
    }

    static void HandleStatistics(AnalyticsService analytics)
    {
        Console.WriteLine("\n=== STATYSTYKI BIBLIOTECZNE ===");

        // Wywołanie modułu analitycznego (Kompozycja)
        Console.WriteLine($"Średni czas wypożyczenia: {analytics.AverageLoanLengthDays():F2} dni");
        Console.WriteLine($"Najpopularniejszy tytuł: {analytics.MostPopularItemTitle()}");
        Console.WriteLine($"Łączna liczba rezerwacji: {analytics.TotalLoans()}");
        Console.WriteLine($"Wskaźnik zrealizowanych rezerwacji: {analytics.FulfillmentRate():F2}%");

        try
        {
            string popularTitle = analytics.MostPopularItemTitle();
            double score = analytics.LogPopularityScore(popularTitle);
            Console.WriteLine($"Logarytmiczny Wskaźnik Popularności dla '{popularTitle}': {score:F4}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BŁĄD W STATYSTYKACH] {ex.Message}");
        }
    }
}