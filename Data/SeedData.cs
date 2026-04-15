using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        try
        {
            // Check if the database has been seeded
            if (context.Vehicles.Any())
            {
                return;
            }

            // Seed Vehicles
            var vehicles = new Vehicle[]
            {
                new Vehicle
                {
                    Make = "Toyota",
                    Model = "Camry",
                    Year = 2023,
                    PricePerDay = 75.00m,
                    IsAvailable = false
                },
                new Vehicle
                {
                    Make = "Honda",
                    Model = "CR-V",
                    Year = 2023,
                    PricePerDay = 85.00m,
                    IsAvailable = false
                },
                new Vehicle
                {
                    Make = "Ford",
                    Model = "Mustang",
                    Year = 2022,
                    PricePerDay = 95.00m,
                    IsAvailable = false
                },
                new Vehicle
                {
                    Make = "Chevrolet",
                    Model = "Silverado",
                    Year = 2023,
                    PricePerDay = 105.00m,
                    IsAvailable = true
                },
                new Vehicle
                {
                    Make = "BMW",
                    Model = "X5",
                    Year = 2022,
                    PricePerDay = 125.00m,
                    IsAvailable = true
                }
            };

            context.Vehicles.AddRange(vehicles);
            context.SaveChanges();

            // Seed Customers
            var customers = new Customer[]
            {
                new Customer
                {
                    FullName = "John Smith",
                    Email = "john.smith@email.com",
                    Phone = "403-555-0101"
                },
                new Customer
                {
                    FullName = "Sarah Johnson",
                    Email = "sarah.johnson@email.com",
                    Phone = "403-555-0102"
                },
                new Customer
                {
                    FullName = "Michael Brown",
                    Email = "michael.brown@email.com",
                    Phone = "403-555-0103"
                },
                new Customer
                {
                    FullName = "Emily Davis",
                    Email = "emily.davis@email.com",
                    Phone = "403-555-0104"
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();

            // Seed Reservations
            var reservations = new Reservation[]
            {
                new Reservation
                {
                    VehicleId = 1,
                    CustomerId = 1,
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(4),
                    TotalCost = 225.00m
                },
                new Reservation
                {
                    VehicleId = 2,
                    CustomerId = 2,
                    StartDate = DateTime.Now.AddDays(2),
                    EndDate = DateTime.Now.AddDays(5),
                    TotalCost = 255.00m
                },
                new Reservation
                {
                    VehicleId = 3,
                    CustomerId = 3,
                    StartDate = DateTime.Now.AddDays(3),
                    EndDate = DateTime.Now.AddDays(6),
                    TotalCost = 285.00m
                }
            };

            context.Reservations.AddRange(reservations);
            context.SaveChanges();

            // Seed Billings with percentage-based tax (e.g., 12.5% tax rate)
            decimal taxPercentage = 0.125m; // 12.5% tax rate
            
            var billings = new Billing[]
            {
                new Billing
                {
                    ReservationId = 1,
                    Tax = reservations[0].TotalCost * taxPercentage,
                    ExtraCharges = 0m,
                    FinalAmount = reservations[0].TotalCost + (reservations[0].TotalCost * taxPercentage)
                },
                new Billing
                {
                    ReservationId = 2,
                    Tax = reservations[1].TotalCost * taxPercentage,
                    ExtraCharges = 15.00m,
                    FinalAmount = reservations[1].TotalCost + (reservations[1].TotalCost * taxPercentage) + 15.00m
                },
                new Billing
                {
                    ReservationId = 3,
                    Tax = reservations[2].TotalCost * taxPercentage,
                    ExtraCharges = 0m,
                    FinalAmount = reservations[2].TotalCost + (reservations[2].TotalCost * taxPercentage)
                }
            };

            context.Billings.AddRange(billings);
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Seed data initialization failed: {ex.Message}");
        }
    }
}