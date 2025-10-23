using System;

namespace LibraryApp.Domain
{
    // Własny wyjątek dla kolizji terminów rezerwacji.
    public class ReservationConflictException : InvalidOperationException
    {
        public ReservationConflictException()
            : base("Wystąpił konflikt terminów rezerwacji.")
        {
        }

        public ReservationConflictException(string message)
            : base(message)
        {
        }

        public ReservationConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}