namespace TransportApi.Models
{
    public class CreateOrderDto
    {
        public string CargoDescription { get; set; } = string.Empty;
        public double Weight { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
    }
}