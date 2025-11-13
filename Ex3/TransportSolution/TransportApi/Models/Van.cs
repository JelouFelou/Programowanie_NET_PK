namespace TransportApi.Models
{
    public class Van : Vehicle
    {
        public double CargoVolume { get; set; }

        public override string GetInfo()
        {
            return $"Van - Nr: {RegistrationNumber}, Max Ładunek: {MaxLoadKg}kg, Objętość: {CargoVolume}m³";
        }
    }
}