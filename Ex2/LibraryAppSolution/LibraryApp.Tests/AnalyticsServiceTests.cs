using Xunit;
using System;
using System.Linq;
using LibraryApp.Domain;
using LibraryApp.Services;

namespace LibraryApp.Tests
{
    public class AnalyticsServiceTests
    {
        private LibraryService GetTestLibraryService()
        {
            var service = new LibraryService();
            service.RegisterUser("u1");
            service.RegisterUser("u2");

            // Pozycje
            var item1 = new Book(service.NextId(), "Tytul A", "Autor X", "1");
            var item2 = new Book(service.NextId(), "Tytul B", "Autor Y", "2");
            var item3 = new Book(service.NextId(), "Tytul C", "Autor Z", "3");

            service.AddItem(item1);
            service.AddItem(item2);
            service.AddItem(item3);

            // Rezerwacje
            var now = DateTime.Now;

            // 1. Zakończona (3 dni)
            var res1 = new Reservation(item1, "u1", now.AddDays(-10), now.AddDays(-7));
            var prop1 = typeof(LibraryService).GetProperty("AllReservations", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            var list1 = prop1.GetValue(service) as System.Collections.Generic.List<Reservation>;
            list1.Add(res1);
            res1.Cancel(); // Używamy Cancel() dla symulacji "zakończenia" (IsActive=false)

            // 2. Aktywna (7 dni)
            service.CreateReservation(item2.Id, "u2", now.AddDays(1), now.AddDays(8));

            // 3. Anulowana (przez CancelReservation w serwisie)
            var res3 = service.CreateReservation(item1.Id, "u2", now.AddDays(10), now.AddDays(13));
            service.CancelReservation(res3.Id);

            // 4. Aktywna, inny użytkownik (2 dni)
            service.CreateReservation(item3.Id, "u1", now.AddDays(15), now.AddDays(17));

            return service;
        }

        [Fact]
        public void AverageLoanLengthDays_ForEmptyData_ReturnsZero()
        {
            var service = new LibraryService();
            var analytics = new AnalyticsService(service);

            Assert.Equal(0, analytics.AverageLoanLengthDays());
        }

        [Fact]
        public void AverageLoanLengthDays_CalculatesCorrectly()
        {
            var service = GetTestLibraryService();
            var analytics = new AnalyticsService(service);

            // Jedyna zakończona rezerwacja ma 3 dni
            // (10 dni przed now do 7 dni przed now)
            Assert.Equal(3.0, analytics.AverageLoanLengthDays());
        }

        [Fact]
        public void TotalLoans_CalculatesCorrectly()
        {
            var service = GetTestLibraryService();
            var analytics = new AnalyticsService(service);

            // 4 rezerwacje (1 zakończona, 2 aktywne, 1 anulowana)
            Assert.Equal(4, analytics.TotalLoans());
        }

        [Fact]
        public void MostPopularItemTitle_ReturnsCorrectTitle()
        {
            var service = GetTestLibraryService();
            var analytics = new AnalyticsService(service);

            // Tytuł A ma 2 rezerwacje, Tytuł B ma 1, Tytuł C ma 1
            Assert.Equal("Tytul A", analytics.MostPopularItemTitle());
        }

        [Fact]
        public void MostPopularItemTitle_ReturnsCorrectTitle_OnTie()
        {
            var service = new LibraryService();
            service.RegisterUser("u1");
            var item1 = new Book(service.NextId(), "Tytul A", "Autor X", "1");
            var item2 = new Book(service.NextId(), "Tytul B", "Autor Y", "2");
            service.AddItem(item1);
            service.AddItem(item2);

            // Obydwa mają po jednej rezerwacji - powinien zwrócić pierwszy z nich (Tytul A, ze względu na kolejność)
            service.CreateReservation(item1.Id, "u1", DateTime.Now.AddDays(1), DateTime.Now.AddDays(8));
            service.CreateReservation(item2.Id, "u1", DateTime.Now.AddDays(10), DateTime.Now.AddDays(17));

            var analytics = new AnalyticsService(service);

            Assert.Equal("Tytul A", analytics.MostPopularItemTitle());
        }

        [Fact]
        public void FulfillmentRate_CalculatesCorrectly()
        {
            var service = GetTestLibraryService();
            var analytics = new AnalyticsService(service);

            // 4 rezerwacje ogółem. 
            // 1 zakończona (nieanulowana) + 2 aktywne = 3 "zrealizowane/w trakcie"
            // 1 anulowana
            // Fulfillment = (3/4) * 100 = 75.0%

            Assert.Equal(75.0, analytics.FulfillmentRate());
        }

        [Fact]
        public void LogPopularityScore_CalculatesCorrectly_AndHandlesDomain()
        {
            var service = GetTestLibraryService();
            var analytics = new AnalyticsService(service);

            // Tytul A ma 2 rezerwacje, log(2+1) = log(3)
            Assert.Equal(Math.Log(3), analytics.LogPopularityScore("Tytul A"), 4);

            // Tytul B ma 1 rezerwację, log(1+1) = log(2)
            Assert.Equal(Math.Log(2), analytics.LogPopularityScore("Tytul B"), 4);

            // Tytul X ma 0 rezerwacji, log(0+1) = log(1) = 0
            Assert.Equal(0.0, analytics.LogPopularityScore("Tytul X"), 4);
        }
    }
}