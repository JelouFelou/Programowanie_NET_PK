# 🚛 Zadanie: System Zarządzania Flotą i Zleceniami Transportowymi

---

## 🎯 Cel

Celem projektu jest stworzenie aplikacji REST API w technologii **ASP.NET Core** służącej do zarządzania **flotą pojazdów i zleceniami transportowymi** w firmie spedycyjnej.

Aplikacja ma umożliwiać rejestrowanie pojazdów, kierowców oraz zleceń przewozu towarów.
Projekt łączy **programowanie obiektowe w C#** (dziedziczenie, interfejsy, zdarzenia, metody rozszerzające) z **bazą danych (Entity Framework Core)** i **REST API**.

---

## 🧠 Zakres technologiczny

* .NET 8 (lub nowszy)
* ASP.NET Core (Minimal API)
* Entity Framework Core (SQLite)
* Programowanie obiektowe (dziedziczenie, interfejsy, zdarzenia, metody rozszerzające)
* Testy jednostkowe (NUnit lub xUnit)
* Testowanie API w Postmanie

---

## ✅ Wymagania funkcjonalne

| Funkcja                      | Metoda | Endpoint                    | Opis                                          |
| ---------------------------- | ------ | --------------------------- | --------------------------------------------- |
| Pobierz listę pojazdów       | GET    | `/api/vehicles`             | Zwraca wszystkie pojazdy                      |
| Dodaj nowy pojazd            | POST   | `/api/vehicles`             | Dodaje pojazd do floty                        |
| Pobierz listę kierowców      | GET    | `/api/drivers`              | Zwraca wszystkich kierowców                   |
| Dodaj kierowcę               | POST   | `/api/drivers`              | Rejestruje kierowcę                           |
| Utwórz zlecenie transportowe | POST   | `/api/orders`               | Tworzy zlecenie (pojazd + kierowca + ładunek) |
| Pobierz zlecenia             | GET    | `/api/orders`               | Zwraca wszystkie aktywne zlecenia             |
| Zakończ zlecenie             | PUT    | `/api/orders/{id}/complete` | Oznacza zlecenie jako zakończone              |

---

## 🧩 Model domenowy

### 1. Klasa abstrakcyjna `Vehicle`

```csharp
public abstract class Vehicle
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public double MaxLoadKg { get; set; }
    public bool IsAvailable { get; set; } = true;

    public abstract string GetInfo();
}
```

### 2. Klasy dziedziczące

* `Truck` – atrybut `TrailerLength`
* `Van` – atrybut `CargoVolume`

### 3. Interfejs `IReservable`

```csharp
public interface IReservable
{
    void AssignDriver(Driver driver);
    void StartOrder();
    void CompleteOrder();
}
```

### 4. Klasa `Driver`

```csharp
public class Driver
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}
```

### 5. Klasa `TransportOrder`

```csharp
public class TransportOrder
{
    public int Id { get; set; }
    public string CargoDescription { get; set; } = string.Empty;
    public double Weight { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsCompleted { get; set; } = false;

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int DriverId { get; set; }
    public Driver? Driver { get; set; }
}
```

### 6. Klasa `FleetManager`

```csharp
public event Action<string>? OnNewOrderCreated;
```

---

## ⚙️ Warstwa danych

* Baza danych: **SQLite**
* OR/M: **Entity Framework Core (Code First)**
* Konfiguracja w `Program.cs`:

```csharp
builder.Services.AddDbContext<FleetDbContext>(options =>
    options.UseSqlite("Data Source=fleet.db"));

db.Database.EnsureCreated();
```

---

## 📬 Przykładowe endpointy

### POST `/api/orders`

```json
{
  "cargoDescription": "Elektronika - ładunek 1.2t",
  "weight": 1200,
  "vehicleId": 2,
  "driverId": 5
}
```

### PUT `/api/orders/3/complete`

Zaznacza zlecenie jako zakończone i zwalnia pojazd oraz kierowcę.

---

## 🧪 Testy jednostkowe

✅ Dodanie pojazdu i kierowcy
✅ Utworzenie zlecenia transportowego
✅ Oznaczenie zlecenia jako zakończonego
✅ Zdarzenie `OnNewOrderCreated` wywoływane po utworzeniu zlecenia
✅ Metoda rozszerzająca `GetAvailableVehicles()`


---

## 🚀 Kryteria zaliczenia

✅ Projekt kompiluje się i działa.  
✅ Działa CRUD dla pojazdów, kierowców i zleceń.  
✅ Zastosowano OOP: dziedziczenie, interfejsy, zdarzenia, metody rozszerzające.  
✅ Baza SQLite tworzona automatycznie.  
✅ Testy jednostkowe przechodzą.  
✅ README zawiera instrukcję uruchomienia i przykłady API

---

## 📁 Struktura projektu

```
TransportApi/
├── Models/
│   ├── Vehicle.cs
│   ├── Truck.cs
│   ├── Van.cs
│   ├── Driver.cs
│   ├── TransportOrder.cs
├── Data/
│   └── FleetDbContext.cs
├── Program.cs
└── Tests/
    └── FleetTests.cs
```
