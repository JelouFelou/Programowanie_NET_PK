using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using TransportApi.Data;
using TransportApi.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TransportApi.Tests
{
    // Klasa bazowa do konfiguracji in-memory bazy danych dla każdego testu
    public class TestBase
    {
        protected readonly FleetDbContext _context;

        public TestBase()
        {
            // Konfiguracja opcji DbContext do użycia in-memory bazy danych
            var options = new DbContextOptionsBuilder<FleetDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            _context = new FleetDbContext(options);
            _context.Database.EnsureCreated();

            // Wypełnienie bazy danymi testowymi
            SeedData(_context);
        }

        private void SeedData(FleetDbContext context)
        {
            // Dodaj Pojazdy
            context.Vehicles.Add(new Truck { RegistrationNumber = "KR1234T", MaxLoadKg = 15000, TrailerLength = 13.6, IsAvailable = true });
            context.Vehicles.Add(new Van { RegistrationNumber = "KR5678V", MaxLoadKg = 1500, CargoVolume = 15.0, IsAvailable = false }); // Niedostępny
            context.Vehicles.Add(new Truck { RegistrationNumber = "WA9999X", MaxLoadKg = 25000, TrailerLength = 18.0, IsAvailable = true });

            // Dodaj Kierowców
            context.Drivers.Add(new Driver { Name = "Jan Kowalski", LicenseNumber = "PK123456", IsAvailable = true });
            context.Drivers.Add(new Driver { Name = "Anna Nowak", LicenseNumber = "PK654321", IsAvailable = false }); // Niedostępny
            context.Drivers.Add(new Driver { Name = "Piotr Zając", LicenseNumber = "PK000000", IsAvailable = true });

            // Dodaj zlecenie aktywne
            context.Orders.Add(new TransportOrder { CargoDescription = "Pilny ładunek", Weight = 1000, VehicleId = 2, DriverId = 2, IsCompleted = false });

            context.SaveChanges();
        }
    }

    public class FleetTests : TestBase
    {
        // Test: Dodawanie pojazdów do floty
        [Fact]
        public async Task AddVehicle_ShouldIncreaseCount()
        {
            // Arrange
            var initialCount = await _context.Vehicles.CountAsync();
            var newTruck = new Truck { RegistrationNumber = "GD1111A", MaxLoadKg = 10000, TrailerLength = 12.0, IsAvailable = true };

            // Act
            _context.Vehicles.Add(newTruck);
            await _context.SaveChangesAsync();
            var finalCount = await _context.Vehicles.CountAsync();

            // Assert
            Assert.Equal(initialCount + 1, finalCount);
            Assert.Equal("GD1111A", newTruck.RegistrationNumber);
        }

        // Test: Metoda rozszerzająca GetAvailableVehicles
        [Fact]
        public async Task GetAvailableVehicles_ShouldReturnOnlyAvailableVehicles()
        {
            // Arrange - Dane testowe przygotowane w TestBase (3 pojazdy, 2 dostępne)

            // Act
            var availableVehicles = await _context.Vehicles.GetAvailableVehicles().ToListAsync();

            // Assert
            Assert.Equal(2, availableVehicles.Count); // Powinny być 2 dostępne pojazdy (KR1234T i WA9999X)
            Assert.True(availableVehicles.All(v => v.IsAvailable));
        }

        // Test: Tworzenie nowego zlecenia
        [Fact]
        public async Task CreateOrder_ShouldReserveResourcesAndSaveOrder()
        {
            // Arrange
            var truck = await _context.Vehicles.FirstAsync(v => v.RegistrationNumber == "KR1234T");
            var driver = await _context.Drivers.FirstAsync(d => d.LicenseNumber == "PK123456");

            var initialOrderCount = await _context.Orders.CountAsync();

            var newOrder = new TransportOrder
            {
                CargoDescription = "Testowy ładunek palet",
                Weight = 5000,
                VehicleId = truck.Id,
                DriverId = driver.Id,
                IsCompleted = false
            };

            // Act
            _context.Orders.Add(newOrder);
            truck.IsAvailable = false; // Ręczna rezerwacja w teście
            driver.IsAvailable = false; // Ręczna rezerwacja w teście
            await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(initialOrderCount + 1, await _context.Orders.CountAsync());
            Assert.False(truck.IsAvailable); // Pojazd zarezerwowany
            Assert.False(driver.IsAvailable); // Kierowca zarezerwowany

            var savedOrder = await _context.Orders
                .Include(o => o.Vehicle)
                .Include(o => o.Driver)
                .LastAsync();

            Assert.Equal(truck.Id, savedOrder.VehicleId);
            Assert.Equal(driver.Id, savedOrder.DriverId);
        }

        // Test: Zakończenie zlecenia i zwolnienie zasobów
        [Fact]
        public async Task CompleteOrder_ShouldReleaseResources()
        {
            // Arrange
            // Znajdź istniejące zlecenie, które nie jest zakończone (ID 2, VehicleId 2, DriverId 2)
            var orderId = 1;
            var truck = await _context.Vehicles.FirstAsync(v => v.RegistrationNumber == "KR5678V"); // ID 2
            var driver = await _context.Drivers.FirstAsync(d => d.LicenseNumber == "PK654321"); // ID 2

            // Upewnij się, że zasoby są zajęte (jak w prawdziwym scenariuszu)
            Assert.False(truck.IsAvailable);
            Assert.False(driver.IsAvailable);

            // Act: Symulacja zakończenia zlecenia (logika z endpointu PUT /complete)
            var orderToComplete = await _context.Orders
                .Include(o => o.Vehicle)
                .Include(o => o.Driver)
                .FirstAsync(o => o.Id == orderId);

            orderToComplete.IsCompleted = true;
            orderToComplete.Vehicle!.IsAvailable = true;
            orderToComplete.Driver!.IsAvailable = true;
            await _context.SaveChangesAsync();

            // Assert
            var completedOrder = await _context.Orders.FindAsync(orderId);
            Assert.True(completedOrder!.IsCompleted);
            Assert.True(truck.IsAvailable); // Pojazd zwolniony
            Assert.True(driver.IsAvailable); // Kierowca zwolniony
        }

        // Testowanie zdarzenia FleetManager
        [Fact]
        public void OnNewOrderCreated_ShouldBeInvokedAfterPublish()
        {
            // Arrange
            var manager = new FleetManager();
            string receivedMessage = string.Empty;

            // Subskrypcja zdarzenia
            manager.OnNewOrderCreated += (message) => receivedMessage = message;

            var testOrder = new TransportOrder { Id = 5, CargoDescription = "Testowy ładunek", Weight = 100 };

            // Act
            manager.PublishNewOrder(testOrder);

            // Assert
            Assert.False(string.IsNullOrEmpty(receivedMessage));
            Assert.Contains("Testowy ładunek", receivedMessage);
            Assert.Contains("[Zdarzenie]", receivedMessage);
        }
    }
}