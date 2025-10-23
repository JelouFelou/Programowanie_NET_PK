using System;

namespace LibraryApp.Domain
{
    public class Reservation
    {
        private static int _nextReservationId = 1;

        public int Id { get; }
        public LibraryItem Item { get; }
        public string UserEmail { get; }
        public DateTime From { get; }
        public DateTime To { get; }
        public bool IsActive { get; private set; } = true;

        public Reservation(LibraryItem item, string userEmail, DateTime from, DateTime to)
        {
            // Walidacja reguł biznesowych
            if (from >= to)
            {
                throw new ArgumentException("Data 'od' musi być wcześniejsza niż data 'do'.");
            }

            // Sprawdzenie dostępności
            if (item == null || string.IsNullOrEmpty(userEmail))
            {
                throw new ArgumentException("Pozycja i email użytkownika są wymagane.");
            }

            Id = _nextReservationId++;
            Item = item;
            UserEmail = userEmail;
            From = from;
            To = to;
        }

        public void Cancel()
        {
            IsActive = false;
        }

        // Metoda sprawdzająca kolizję z inną aktywną rezerwacją
        // Tutaj weryfikujemy:
        // CZY rezerwacja na ten sam przedmiot w ogóle jest możliwa, gdy przedmiot JEST rezerwowany.
        public bool ConflictsWith(Reservation other)
        {
            if (!other.IsActive || other.Item.Id != Item.Id)
            {
                return false;
            }

            // Kolizja w uproszczonym modelu: dwa aktywne wypożyczenia na ten sam przedmiot
            // są zawsze konfliktem, jeśli są nałożone.
            // Zakładamy, że zasób jest niedostępny, gdy jest aktywna rezerwacja.
            return From < other.To && To > other.From;
        }
    }
}