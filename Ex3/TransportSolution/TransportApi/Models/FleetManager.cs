using Microsoft.EntityFrameworkCore;

namespace TransportApi.Models
{
    // Klasa do obsługi zdarzeń (Event Bus)
    public class FleetManager
    {
        public event Action<string>? OnNewOrderCreated;

        public void PublishNewOrder(TransportOrder order)
        {
            var message = $"[Zdarzenie] Utworzono nowe zlecenie ID: {order.Id} (Ładunek: {order.CargoDescription})";
            // Wywołanie zdarzenia
            OnNewOrderCreated?.Invoke(message);
        }
    }

    // Klasa zawierająca metody rozszerzające
    public static class FleetExtensions
    {
        public static IQueryable<Vehicle> GetAvailableVehicles(this IQueryable<Vehicle> vehicles)
        {
            return vehicles.Where(v => v.IsAvailable);
        }
    }
}