using System;
using System.Collections.Generic;
using System.Linq;
using LibraryApp.Domain;
using LibraryApp.Extensions;

namespace LibraryApp.Services
{
    public class LibraryService
    {
        private int _nextId = 1;

        // Delegaty i Zdarzenia
        public event Action<Reservation> OnNewReservation;
        public event Action<Reservation> OnReservationCancelled;

        // Kolekcje przechowujące stan
        private readonly List<LibraryItem> _items = new List<LibraryItem>();
        private readonly List<Reservation> _reservations = new List<Reservation>();
        private readonly HashSet<string> _users = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Metoda pomocnicza dla generowania ID
        public int NextId() => _nextId++;

        // --- Operacje Zarządzania ---

        public void AddItem(LibraryItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            // Sprawdzenie unikalności ID, choć NextId() to zapewnia
            if (_items.Any(i => i.Id == item.Id))
            {
                throw new ArgumentException($"Pozycja z ID {item.Id} już istnieje.", nameof(item));
            }
            _items.Add(item);
        }

        public void RegisterUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email nie może być pusty.", nameof(email));
            }
            if (!_users.Add(email))
            {
                throw new ArgumentException($"Użytkownik o emailu '{email}' jest już zarejestrowany.");
            }
        }

        // Użycie LINQ i Metod Rozszerzających (Available() z LibraryExtensions)
        public IEnumerable<LibraryItem> ListAvailableItems(string filter = null)
        {
            var query = _items.Available(); // Metoda rozszerzająca

            if (!string.IsNullOrWhiteSpace(filter))
            {
                // Wyrażenie lambda i LINQ (Where)
                query = query.Where(i => i.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)
                                      || (i is Book b && b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase)));
            }

            return query.OrderBy(i => i.Title); // LINQ OrderBy
        }

        public LibraryItem GetItemById(int itemId)
        {
            return _items.FirstOrDefault(i => i.Id == itemId);
        }

        // --- Operacje Rezerwacji ---

        public Reservation CreateReservation(int itemId, string userEmail, DateTime from, DateTime to)
        {
            var item = GetItemById(itemId) ?? throw new ArgumentException($"Nie znaleziono pozycji o ID {itemId}.");

            if (!_users.Contains(userEmail, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Użytkownik o emailu '{userEmail}' nie jest zarejestrowany.");
            }

            // Sprawdzenie dostępności (IsAvailable ustawiane w SetItemAvailability)
            if (!item.IsAvailable)
            {
                throw new InvalidOperationException($"Pozycja '{item.Title}' (ID: {itemId}) jest już zarezerwowana.");
            }

            // Walidacja: data 'od' musi być późniejsza niż teraz (lub równa)
            if (from < DateTime.Now.Date)
            {
                throw new ArgumentException("Nie można rezerwować wstecz.");
            }

            // Walidacja: From < To (robiona w konstruktorze Reservation, ale lepiej ją mieć też tutaj)
            if (from >= to)
            {
                throw new ArgumentException("Data 'od' musi być wcześniejsza niż data 'do'.");
            }

            // Logika kolizji rezerwacji
            // Dla demonstarcji ReservationConflictException, dodajemy jawne sprawdzenie:
            if (_reservations.Any(r => r.Item.Id == itemId && r.IsActive && r.ConflictsWith(new Reservation(item, userEmail, from, to))))
            {
                throw new ReservationConflictException($"Pozycja '{item.Title}' ma już kolidującą aktywną rezerwację.");
            }

            // Tworzenie rezerwacji
            var reservation = new Reservation(item, userEmail, from, to);
            _reservations.Add(reservation);
            SetItemAvailability(item, false); // Ustawienie niedostępności

            // Emisja zdarzenia (Delegat i Zdarzenie)
            OnNewReservation?.Invoke(reservation);

            return reservation;
        }

        public void CancelReservation(int reservationId)
        {
            var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);

            if (reservation == null)
            {
                throw new ArgumentException($"Nie znaleziono rezerwacji o ID {reservationId}.");
            }

            if (!reservation.IsActive)
            {
                throw new InvalidOperationException($"Rezerwacja o ID {reservationId} jest już anulowana.");
            }

            reservation.Cancel(); // Anulowanie
            SetItemAvailability(reservation.Item, true); // Ustawienie dostępności

            // Emisja zdarzenia
            OnReservationCancelled?.Invoke(reservation);
        }

        public IEnumerable<Reservation> GetUserReservations(string userEmail)
        {
            // LINQ Where
            return _reservations.Where(r => r.UserEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase))
                                .OrderByDescending(r => r.From);
        }

        // Metoda umożliwiająca zmianę dostępności (z użyciem refleksji/enkapsulacji)
        private void SetItemAvailability(LibraryItem item, bool isAvailable)
        {
            // Używamy "protected set" we właściwości, aby umożliwić jej zmianę tylko w klasie bazowej
            // lub dziedziczących, co spełnia zasadę enkapsulacji.
            var propertyInfo = typeof(LibraryItem).GetProperty(nameof(LibraryItem.IsAvailable));
            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(item, isAvailable);
            }
        }

        // Właściwość tylko do odczytu dla AnalyticsService
        public IReadOnlyList<Reservation> AllReservations => _reservations;
        public IReadOnlyList<LibraryItem> AllItems => _items;
        public IReadOnlySet<string> AllUsers => _users;
    }
}