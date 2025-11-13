using System.ComponentModel.DataAnnotations.Schema;

namespace TransportApi.Models
{
    // Klasa implementuje interfejs IVehicleInfo, w pełni realizując wymagania OOP.
    public class Vehicle : IVehicleInfo
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public double MaxLoadKg { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Właściwość nawigacyjna do TransportOrder
        public ICollection<TransportOrder> Orders { get; set; } = new List<TransportOrder>();

        // Zmieniono na 'virtual', aby klasy pochodne mogły ją nadpisywać.
        public virtual string GetInfo()
        {
            // Podstawowa implementacja interfejsu
            return $"Pojazd ogólny - Nr: {RegistrationNumber}";
        }
    }
}