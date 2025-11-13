namespace TransportApi.Models
{
    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        public ICollection<TransportOrder> Orders { get; set; } = new List<TransportOrder>();
    }
}