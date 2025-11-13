using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models
{
    public class TransportOrder
    {
        public int Id { get; set; }
        public string CargoDescription { get; set; } = string.Empty;
        public double Weight { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; } = false;

        // Klucze obce
        public int VehicleId { get; set; }
        public int DriverId { get; set; }

        // Właściwości nawigacyjne
        public Vehicle? Vehicle { get; set; }
        public Driver? Driver { get; set; }
    }
}