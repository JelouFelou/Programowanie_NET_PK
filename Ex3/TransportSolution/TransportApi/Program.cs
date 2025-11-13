using Microsoft.EntityFrameworkCore;
using TransportApi.Data;
using TransportApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Konfiguracja Us³ug ---

// U¿ywamy ConfigureHttpJsonOptions do globalnej konfiguracji serializatora JSON.
// TO JEST KLUCZOWA ZMIANA, KTÓRA NAPRAWIA B£¥D CYKLICZNYCH REFERENCJI.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Pozwala na poprawn¹ serializacjê enumów
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

    // NAPRAWA: Ignorowanie cykli serializacji (np. Order -> Vehicle -> Orders -> Order)
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Dodaj Entity Framework Core i SQLite
builder.Services.AddDbContext<FleetDbContext>(options =>
    // U¿ycie Data Source=fleet.db jako domyœlnej wartoœci, jeœli connection string jest pusty
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=fleet.db"));

// Dodaj FleetManager jako Singleton (aby zdarzenie by³o zachowane)
builder.Services.AddSingleton<FleetManager>();

// Dodaj Swagger/OpenAPI dla ³atwego testowania
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 2. Inicjalizacja Bazy Danych ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
    // Tworzy bazê danych, jeœli nie istnieje
    db.Database.EnsureCreated();
}

// --- 3. Konfiguracja Middleware ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- 4. Mapowanie Endpointów ---

// ----------------------------------------------------
// DRIVER ENDPOINTS
// ----------------------------------------------------

// GET /api/drivers - Pobierz listê kierowców
app.MapGet("/api/drivers", async (FleetDbContext db) =>
{
    return Results.Ok(await db.Drivers.ToListAsync());
})
.WithName("GetDrivers");

// POST /api/drivers - Dodaj kierowcê
app.MapPost("/api/drivers", async (FleetDbContext db, Driver driver) =>
{
    // Prosta walidacja unikalnoœci na podstawie regu³y w DbContext
    try
    {
        db.Drivers.Add(driver);
        await db.SaveChangesAsync();
        return Results.Created($"/api/drivers/{driver.Id}", driver);
    }
    catch (DbUpdateException ex)
    {
        return Results.Conflict($"Nie mo¿na dodaæ kierowcy. Prawdopodobnie numer licencji ({driver.LicenseNumber}) jest ju¿ u¿ywany. Szczegó³y: {ex.InnerException?.Message}");
    }
})
.WithName("CreateDriver");

// ----------------------------------------------------
// VEHICLE ENDPOINTS
// ----------------------------------------------------

// GET /api/vehicles - Pobierz listê pojazdów
app.MapGet("/api/vehicles", async (FleetDbContext db) =>
{
    // Pamiêtaj: Vehicles jest DbSet<Vehicle>, wiêc zwraca Truck i Van
    return Results.Ok(await db.Vehicles
        .Include(v => v.Orders)
        .ToListAsync());
})
.WithName("GetVehicles");

// GET /api/vehicles/available - Pobierz listê dostêpnych pojazdów (Metoda Rozszerzaj¹ca)
app.MapGet("/api/vehicles/available", async (FleetDbContext db) =>
{
    // U¿ycie metody rozszerzaj¹cej
    return Results.Ok(await db.Vehicles.GetAvailableVehicles().ToListAsync());
})
.WithName("GetAvailableVehicles");

// POST /api/vehicles - Dodaj pojazd
app.MapPost("/api/vehicles", async (FleetDbContext db, Vehicle vehicle) =>
{
    // Walidacja ³adunku
    if (vehicle.MaxLoadKg <= 0)
    {
        return Results.BadRequest("Maksymalny ³adunek musi byæ wiêkszy ni¿ 0.");
    }

    // Prosta walidacja unikalnoœci
    if (await db.Vehicles.AnyAsync(v => v.RegistrationNumber == vehicle.RegistrationNumber))
    {
        return Results.Conflict($"Pojazd o numerze rejestracyjnym {vehicle.RegistrationNumber} ju¿ istnieje.");
    }

    db.Vehicles.Add(vehicle);
    await db.SaveChangesAsync();
    return Results.Created($"/api/vehicles/{vehicle.Id}", vehicle);
})
.WithName("CreateVehicle");


// ----------------------------------------------------
// ORDER ENDPOINTS
// ----------------------------------------------------

// POST /api/orders - Utwórz nowe zlecenie transportowe
app.MapPost("/api/orders", async (
    FleetDbContext db,
    CreateOrderDto orderDto,
    FleetManager fleetManager) =>
{
    // 1. Walidacja
    var vehicle = await db.Vehicles.GetAvailableVehicles()
        .FirstOrDefaultAsync(v => v.Id == orderDto.VehicleId);

    var driver = await db.Drivers
        .Where(d => d.IsAvailable)
        .FirstOrDefaultAsync(d => d.Id == orderDto.DriverId);

    if (vehicle == null)
    {
        return Results.NotFound($"Pojazd o ID {orderDto.VehicleId} nie zosta³ znaleziony lub jest niedostêpny.");
    }
    if (driver == null)
    {
        return Results.NotFound($"Kierowca o ID {orderDto.DriverId} nie zosta³ znaleziony lub jest niedostêpny.");
    }
    if (orderDto.Weight > vehicle.MaxLoadKg)
    {
        return Results.BadRequest($"£adunek ({orderDto.Weight}kg) przekracza maksymalny udŸwig pojazdu ({vehicle.MaxLoadKg}kg).");
    }

    // 2. Tworzenie i rezerwacja zasobów
    var order = new TransportOrder
    {
        CargoDescription = orderDto.CargoDescription,
        Weight = orderDto.Weight,
        VehicleId = orderDto.VehicleId,
        DriverId = orderDto.DriverId,
        Vehicle = vehicle, // Ustawienie referencji
        Driver = driver    // Ustawienie referencji
    };

    // Rezerwacja
    vehicle.IsAvailable = false;
    driver.IsAvailable = false;

    // Dodanie do kontekstu
    db.Orders.Add(order);
    db.Vehicles.Update(vehicle);
    db.Drivers.Update(driver);
    await db.SaveChangesAsync();

    // 3. Wywo³anie zdarzenia
    fleetManager.PublishNewOrder(order);

    // 4. Zwrot obiektu (teraz z IgnoreCycles powinno dzia³aæ)
    // Musimy ponownie za³adowaæ obiekt z relacjami, aby mieæ pe³ny kontekst do serializacji
    var createdOrder = await db.Orders
        .Include(o => o.Vehicle)
        .Include(o => o.Driver)
        .FirstOrDefaultAsync(o => o.Id == order.Id);

    return Results.Created($"/api/orders/{createdOrder!.Id}", createdOrder);
})
.WithName("CreateOrder");

// GET /api/orders - Pobierz wszystkie aktywne zlecenia
app.MapGet("/api/orders", async (FleetDbContext db) =>
{
    return Results.Ok(await db.Orders
        .Where(o => !o.IsCompleted)
        .Include(o => o.Vehicle)
        .Include(o => o.Driver)
        .ToListAsync());
})
.WithName("GetActiveOrders");


// PUT /api/orders/{id}/complete - Zakoñcz zlecenie
app.MapPut("/api/orders/{id}/complete", async (FleetDbContext db, int id) =>
{
    var order = await db.Orders
        .Include(o => o.Vehicle)
        .Include(o => o.Driver)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null || order.Vehicle == null || order.Driver == null)
    {
        return Results.NotFound($"Zlecenie o ID {id} nie zosta³o znalezione lub jest niekompletne.");
    }

    if (order.IsCompleted)
    {
        return Results.Conflict("Zlecenie jest ju¿ zakoñczone.");
    }

    // Oznacz zlecenie jako zakoñczone
    order.IsCompleted = true;

    // Zwolnij zasoby
    order.Vehicle.IsAvailable = true;
    order.Driver.IsAvailable = true;

    db.Orders.Update(order);
    db.Vehicles.Update(order.Vehicle);
    db.Drivers.Update(order.Driver);
    await db.SaveChangesAsync();

    return Results.Ok(order);
})
.WithName("CompleteOrder");

app.Run();