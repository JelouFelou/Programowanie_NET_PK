using Xunit;
using System;
using System.Linq;
using LibraryApp.Domain;
using LibraryApp.Services;

namespace LibraryApp.Tests
{
    public class LibraryServiceTests
    {
        // --- Testy Modelu Domenowego ---

        [Fact]
        public void Book_Creation_Succeeds()
        {
            var book = new Book(1, "Test Title", "Test Author", "12345");
            Assert.Equal(1, book.Id);
            Assert.Equal("Test Title", book.Title);
            Assert.True(book.IsAvailable);
        }

        [Fact]
        public void EBook_Creation_PolymorphicInfo_Succeeds()
        {
            var ebook = new EBook(2, "E-Test", "E-Author", "67890", "PDF");
            Assert.Equal("PDF", ebook.FileFormat);

            // Sprawdzenie, czy DisplayInfo działa bez błędu
            Assert.Throws<ArgumentNullException>(() => new EBook(3, "X", "Y", "Z", null));
        }

        [Fact]
        public void Reservation_Creation_WithInvalidDates_ThrowsArgumentException()
        {
            var item = new Book(10, "Test", "A", "B");
            var now = DateTime.Now;

            // From >= To
            Assert.Throws<ArgumentException>(() => new Reservation(item, "user@test.pl", now.AddDays(5), now.AddDays(1)));
            Assert.Throws<ArgumentException>(() => new Reservation(item, "user@test.pl", now.AddDays(5), now.AddDays(5)));
        }

        // --- Testy Serwisu Biblioteki ---

        [Fact]
        public void AddItem_And_RegisterUser_Succeeds()
        {
            var service = new LibraryService();
            var book = new Book(service.NextId(), "Test Book", "A", "B");
            string userEmail = "test@user.pl";

            service.AddItem(book);
            service.RegisterUser(userEmail); // Użytkownik zarejestrowany

            Assert.Single(service.AllItems);

            Assert.Contains(userEmail, service.AllUsers);
        }

        [Fact]
        public void CreateReservation_AvailableItem_Succeeds_AndEmitsEvent()
        {
            var service = new LibraryService();
            var book = new Book(service.NextId(), "Test Book", "A", "B");
            service.AddItem(book);
            service.RegisterUser("test@user.pl");

            Reservation createdReservation = null;
            service.OnNewReservation += r => createdReservation = r;

            var res = service.CreateReservation(book.Id, "test@user.pl", DateTime.Now.AddDays(1), DateTime.Now.AddDays(8));

            Assert.NotNull(res);
            Assert.True(res.IsActive);
            Assert.False(book.IsAvailable); // Sprawdzenie dostępności
            Assert.Equal(res, createdReservation); // Sprawdzenie zdarzenia
        }

        [Fact]
        public void CreateReservation_UnavailableItem_ThrowsInvalidOperationException()
        {
            var service = new LibraryService();
            var book = new Book(service.NextId(), "Test Book", "A", "B");
            service.AddItem(book);
            service.RegisterUser("user1@test.pl");
            service.RegisterUser("user2@test.pl");

            // 1. Pierwsza rezerwacja (ustawia IsAvailable=false)
            service.CreateReservation(book.Id, "user1@test.pl", DateTime.Now.AddDays(1), DateTime.Now.AddDays(8));

            // 2. Druga rezerwacja
            Assert.Throws<InvalidOperationException>(() =>
                service.CreateReservation(book.Id, "user2@test.pl", DateTime.Now.AddDays(2), DateTime.Now.AddDays(9)));
        }

        [Fact]
        public void CreateReservation_ConflictException_IsThrown()
        {
            var service = new LibraryService();
            var book = new Book(service.NextId(), "Test Book", "A", "B");
            service.AddItem(book);
            service.RegisterUser("user1@test.pl");
            service.RegisterUser("user2@test.pl");

            var futureDate = DateTime.Now.AddDays(30);

            service.CreateReservation(book.Id, "user1@test.pl", futureDate, futureDate.AddDays(7));

            var propInfo = typeof(LibraryItem).GetProperty(nameof(LibraryItem.IsAvailable), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            propInfo.SetValue(book, true);
            Assert.Throws<ReservationConflictException>(() =>
                service.CreateReservation(book.Id, "user2@test.pl", futureDate.AddDays(1), futureDate.AddDays(8)));
        }

        [Fact]
        public void CancelReservation_Succeeds_UpdatesAvailability_AndEmitsEvent()
        {
            var service = new LibraryService();
            var book = new Book(service.NextId(), "Test Book", "A", "B");
            service.AddItem(book);
            service.RegisterUser("test@user.pl");

            var res = service.CreateReservation(book.Id, "test@user.pl", DateTime.Now.AddDays(1), DateTime.Now.AddDays(8));

            Reservation cancelledReservation = null;
            service.OnReservationCancelled += r => cancelledReservation = r;

            service.CancelReservation(res.Id);

            Assert.False(res.IsActive);
            Assert.True(book.IsAvailable); // Powrót dostępności
            Assert.Equal(res, cancelledReservation); // Sprawdzenie zdarzenia
        }

        [Fact]
        public void CancelReservation_Twice_ThrowsInvalidOperationException()
        {
            var service = new LibraryService();
            var book = new Book(service.NextId(), "Test Book", "A", "B");
            service.AddItem(book);
            service.RegisterUser("test@user.pl");

            var res = service.CreateReservation(book.Id, "test@user.pl", DateTime.Now.AddDays(1), DateTime.Now.AddDays(8));

            service.CancelReservation(res.Id);

            Assert.Throws<InvalidOperationException>(() => service.CancelReservation(res.Id));
        }
    }
}