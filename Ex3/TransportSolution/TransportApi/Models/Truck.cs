namespace TransportApi.Models
{
    public class Truck : Vehicle
    {
        public double TrailerLength { get; set; }

        public override string GetInfo()
        {
            return $"Ciężarówka - Nr: {RegistrationNumber}, Max Ładunek: {MaxLoadKg}kg, Długość Naczepy: {TrailerLength}m";
        }
    }
}