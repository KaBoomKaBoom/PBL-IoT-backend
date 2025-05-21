public static class MockData
{
    public static async Task SeedData(AppDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Users if none exist
        if (!context.Users.Any())
        {
            var users = new List<User>
            {
                new User { Id = 1, Name = "Alice", Email = "test", Password = "fdsagjdfslkadsvsa34rt362781923784" },
            };
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }

        // Seed SensorTypes if none exist
        if (!context.SensorTypes.Any())
        {
            var sensorTypes = new List<SensorType>
            {
                new SensorType { Id = 1, Name = "Temperature" },
                new SensorType { Id = 2, Name = "Humidity" },
                new SensorType { Id = 3, Name = "Luminosity" }
            };
            context.SensorTypes.AddRange(sensorTypes);
            await context.SaveChangesAsync();
        }

        // Seed Sensors if none exist
        if (!context.Sensors.Any())
        {
            var sensors = new List<Sensor>
            {
                new Sensor { Id = 1, UserId = 1, SensorTypeId = 1 }, // Ficus - Temperature
                new Sensor { Id = 2, UserId = 1, SensorTypeId = 2 }, // Ficus - Humidity
                new Sensor { Id = 3, UserId = 1, SensorTypeId = 3 }, // Ficus - Soil Moisture
                new Sensor { Id = 4, UserId = 1, SensorTypeId = 1 }, // Basil - Temperature
                new Sensor { Id = 5, UserId = 1, SensorTypeId = 2 }, // Basil - Humidity
                new Sensor { Id = 6, UserId = 1, SensorTypeId = 3 }, // Basil - Soil Moisture
                new Sensor { Id = 7, UserId = 1, SensorTypeId = 1 }, // Monstera - Temperature
                new Sensor { Id = 8, UserId = 1, SensorTypeId = 2 }, // Monstera - Humidity
                new Sensor { Id = 9, UserId = 1, SensorTypeId = 3 }  // Monstera - Soil Moisture
            };
            context.Sensors.AddRange(sensors);
            await context.SaveChangesAsync();
        }
        // Seed Plants if none exist
        if (!context.Plants.Any())
        {
            var plants = new List<Plant>
            {
                new Plant
                {
                    Id = 1,
                    Name = "Ficus",
                    Description = "Indoor ficus tree with glossy leaves",
                    UserId = 1,
                    SensorIds = new List<int> { 1, 2, 3 }
                },
                new Plant
                {
                    Id = 2,
                    Name = "Basil",
                    Description = "Herb plant in kitchen garden",
                    UserId = 1,
                    SensorIds = new List<int> { 4, 5, 6 }
                },
                new Plant
                {
                    Id = 3,
                    Name = "Monstera",
                    Description = "Large-leafed tropical plant",
                    UserId = 1,
                    SensorIds = new List<int> { 7, 8, 9 }
                }
            };
            context.Plants.AddRange(plants);
            await context.SaveChangesAsync();
        }


        // Seed SensorReadings if none exist
        if (!context.SensorReadings.Any())
        {
            var readings = new List<SensorReading>();
            int readingId = 1;
            var baseTime = new DateTime(2025, 5, 20, 8, 0, 0, DateTimeKind.Utc);

            for (int sensorId = 1; sensorId <= 9; sensorId++)
            {
                double minValue, maxValue;
                switch (sensorId % 3)
                {
                    case 1: // Temperature (°C)
                        minValue = 10.0; maxValue = 35.0; break;
                    case 2: // Humidity (%)
                        minValue = 40.0; maxValue = 80.0; break;
                    default: // Soil Moisture (%)
                        minValue = 20.0; maxValue = 60.0; break;
                }

                for (int i = 0; i < 5; i++)
                {
                    readings.Add(new SensorReading
                    {
                        Id = readingId++,
                        SensorId = sensorId,
                        Value = Math.Round(minValue + (maxValue - minValue) * new Random().NextDouble(), 2),
                        Timestamp = baseTime.AddHours(i * 4)
                    });
                }
            }
            context.SensorReadings.AddRange(readings);
            await context.SaveChangesAsync();
        }
    }
}