using System;

namespace LibraryApp.Domain
{
    // Interfejs dla zasobów, które można rezerwować.
    public interface IReservable
    {
        void Reserve(string userEmail, DateTime from, DateTime to);
        void CancelReservation(string userEmail);
        bool IsAvailable { get; }
    }
}