using System;
using System.Linq;
using LibraryApp.Domain;

namespace LibraryApp.Services
{
    // Kompozycja: AnalyticsService wymaga instancji LibraryService
    public class AnalyticsService
    {
        private readonly LibraryService _libraryService;

        public AnalyticsService(LibraryService libraryService)
        {
            _libraryService = libraryService ?? throw new ArgumentNullException(nameof(libraryService));
        }

        // Średnia długość wypożyczenia (w dniach)
        public double AverageLoanLengthDays()
        {
            var completedLoans = _libraryService.AllReservations.Where(r => !r.IsActive).ToList();

            if (!completedLoans.Any())
            {
                return 0;
            }

            // Operacje na kolekcjach (Select i Average)
            return completedLoans
                .Select(r => (r.To - r.From).TotalDays)
                .Average();
        }

        // Łączna liczba wszystkich rezerwacji (aktywnych i anulowanych/zakończonych)
        public int TotalLoans()
        {
            // Operacje na kolekcjach (Count)
            return _libraryService.AllReservations.Count;
        }

        // Najpopularniejszy tytuł (najczęściej rezerwowany)
        public string MostPopularItemTitle()
        {
            var reservations = _libraryService.AllReservations;

            if (!reservations.Any())
            {
                return "Brak danych";
            }

            // Operacje na kolekcjach (GroupBy, OrderByDescending, Select, FirstOrDefault)
            var mostPopular = reservations
                .GroupBy(r => r.Item.Title) // Grupowanie po tytule
                .OrderByDescending(g => g.Count()) // Sortowanie malejąco po liczbie rezerwacji
                .Select(g => g.Key) // Wybór samego tytułu
                .FirstOrDefault();

            return mostPopular ?? "Brak danych";
        }

        // Odsetek zrealizowanych rezerwacji (nieanulowanych)
        public double FulfillmentRate()
        {
            var total = _libraryService.AllReservations.Count;

            if (total == 0)
            {
                return 0.0;
            }

            var fulfilled = _libraryService.AllReservations.Count(r => r.IsActive); // Aktywne + zakończone

            // W tym uproszczonym modelu: zakładamy, że tylko aktywne są "zrealizowane/w trakcie"
            // Prawidłowa metryka powinna wymagać flagi 'Returned' lub sprawdzać czy 'To' minął.
            // Tutaj liczymy te, które nie zostały ręcznie anulowane.
            var notCancelled = _libraryService.AllReservations.Count(r => r.IsActive); // LICZYMY TYLKO AKTYWNE

            // Lepszy wskaźnik: (Liczba_Anulowanych) / (Liczba_Wszystkich)
            // Zmieniamy na wskaźnik zrealizowanych (aktywnych + zakończonych 'z sukcesem'):
            var successful = _libraryService.AllReservations.Count(r => r.IsActive || r.To < DateTime.Now);

            return (double)successful / total * 100.0;
        }

        // Przykład funkcji "naukowej" z bezpieczną obsługą domeny (wyjątki)
        // Oblicza logarytm naturalny z liczby rezerwacji danego tytułu + 1 (dla uniknięcia log(0))
        public double LogPopularityScore(string title)
        {
            var reservations = _libraryService.AllReservations.Count(r =>
                r.Item.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (reservations < 0)
            {
                // Choć Count nigdy nie jest < 0, to jest przykład walidacji domeny
                throw new ArgumentException("Liczba rezerwacji nie może być ujemna.");
            }

            // Zabezpieczenie przed log(0) poprzez dodanie 1, co jest standardową praktyką
            // w metrykach bazujących na logarytmie z liczników.
            return Math.Log(reservations + 1);
        }
    }
}