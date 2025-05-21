using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public HomeController(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAppPlants([FromRoute] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("Invalid user ID.");
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return BadRequest("Invalid user ID.");
        }

        var plants = await _context.Plants
            .Where(p => p.UserId == userId)
            .ToListAsync();

        if (plants == null || !plants.Any())
        {
            return NotFound("No plants found for the user.");
        }

        return Ok(plants);
    }

    [HttpGet("{userId}/getPlants")]
    public async Task<IActionResult> GetPlants([FromRoute] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("Invalid user ID.");
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return BadRequest("Invalid user ID.");
        }

        var plants = await _context.Plants
            .Where(p => p.UserId == userId)
            .ToListAsync();

        if (plants == null || !plants.Any())
        {
            return NotFound("No plants found for the user.");
        }

        return Ok(plants);
    }

    [HttpGet("getPlant/{plantId}")]
    public async Task<IActionResult> GetPlant([FromRoute] int plantId)
    {
        if (plantId <= 0)
        {
            return BadRequest("Invalid plant ID.");
        }

        var plant = await _context.Plants
            .FirstOrDefaultAsync(p => p.Id == plantId);

        if (plant == null)
        {
            return NotFound("Plant not found.");
        }
        var plantToReturn = new PlantDTO
        {
            Id = plant.Id,
            Name = plant.Name,
            Description = plant.Description,
            UserId = plant.UserId,
            SensorIds = plant.SensorIds
        };
        var lastSensorReadings = new List<SensorReadingDTO>();
        foreach (var sensorId in plant.SensorIds)
        {
            var lastSensorReading = await _context.SensorReadings
                .Where(sr => sr.SensorId == sensorId)
                .OrderByDescending(sr => sr.Timestamp)
                .FirstOrDefaultAsync();
            if (lastSensorReading != null)
            {
                var sensorType = await _context.SensorTypes
                    .Where(st => st.Id == _context.Sensors.Where(s => s.Id == sensorId).Select(s => s.SensorTypeId).FirstOrDefault())
                    .FirstOrDefaultAsync();

                if (sensorType != null)
                {
                    lastSensorReadings.Add(new SensorReadingDTO
                    {
                        SensorReading = lastSensorReading,
                        SensorType = sensorType
                    });
                }
            }
        }
        plantToReturn.LastSensorReadings = lastSensorReadings;
        if (lastSensorReadings == null || !lastSensorReadings.Any())
        {
            return NotFound("No sensor readings found for the specified plant.");
        }

        return Ok(plantToReturn);
    }

    [HttpGet("getPlant/report/{specificSensor}/{period}")]
    public async Task<IActionResult> GetPlantReport([FromRoute] int specificSensor, [FromRoute] int period)
    {
        if (specificSensor <= 0)
        {
            return BadRequest("Invalid sensor ID.");
        }

        var sensorExists = await _context.Sensors.AnyAsync(s => s.Id == specificSensor);
        if (!sensorExists)
        {
            return BadRequest("Invalid sensor ID.");
        }

        var sensorReadings = new List<SensorReadingDTO>();

        var sensorReading = new SensorReadingDTO();
        Console.WriteLine("Processing sensor ID: " + specificSensor);
        var readings = await _context.SensorReadings
            .Where(sr => sr.SensorId == specificSensor && sr.Timestamp >= DateTime.UtcNow.AddDays(-period))
            .ToListAsync();

        if (readings != null && readings.Any())
        {
            Console.WriteLine("Found " + readings.Count + " readings for sensor ID: " + specificSensor);
            foreach (var reading in readings)
            {
                Console.WriteLine("Processing reading with ID: " + reading.Id);
                var sensorType = await _context.SensorTypes
                .Where(st => st.Id == _context.Sensors.Where(s => s.Id == specificSensor).Select(s => s.SensorTypeId).FirstOrDefault())
                .FirstOrDefaultAsync();

                Console.WriteLine("Sensor type found: " + (sensorType != null ? sensorType.Name : "null"));
                if (sensorType != null)
                {
                    sensorReading.SensorReading = reading;
                    sensorReading.SensorType = sensorType;
                    sensorReadings.Add(sensorReading);
                }
            }
        }

        if (sensorReadings == null || !sensorReadings.Any())
        {
            return NotFound("No sensor readings found for the specified plant and sensor.");
        }

        return Ok(sensorReadings);
    }

    [HttpPost("getPlant/report/allSensors")]
    public async Task<IActionResult> GetPlantReportAllSensors([FromBody] SensorsForReportDTO sensorsForReportDTO)
    {
        if (sensorsForReportDTO == null)
        {
            return BadRequest("Invalid report data.");
        }

        var sensorReadings = new List<SensorReadingDTO>();
        if (sensorsForReportDTO.SensorIds == null || !sensorsForReportDTO.SensorIds.Any())
        {
            return BadRequest("No sensor IDs provided.");
        }
        if (sensorsForReportDTO.Days <= 0)
        {
            return BadRequest("Invalid number of days.");
        }
        Console.WriteLine(sensorsForReportDTO.SensorIds.Count + " sensor IDs provided.");
        foreach (var sensorId in sensorsForReportDTO.SensorIds)
        {
            var sensorReading = new SensorReadingDTO();
            Console.WriteLine("Processing sensor ID: " + sensorId);
            var readings = await _context.SensorReadings
                .Where(sr => sr.SensorId == sensorId && sr.Timestamp >= DateTime.UtcNow.AddDays(-sensorsForReportDTO.Days))
                .ToListAsync();

            if (readings != null && readings.Any())
            {
                Console.WriteLine("Found " + readings.Count + " readings for sensor ID: " + sensorId);
                foreach (var reading in readings)
                {
                    Console.WriteLine("Processing reading with ID: " + reading.Id);
                    var sensorType = await _context.SensorTypes
                    .Where(st => st.Id == _context.Sensors.Where(s => s.Id == sensorId).Select(s => s.SensorTypeId).FirstOrDefault())
                    .FirstOrDefaultAsync();

                    Console.WriteLine("Sensor type found: " + (sensorType != null ? sensorType.Name : "null"));
                    if (sensorType != null)
                    {
                        sensorReading.SensorReading = reading;
                        sensorReading.SensorType = sensorType;
                        sensorReadings.Add(sensorReading);
                    }
                }
            }
        }
        Console.WriteLine("Found " + sensorReadings.Count + " readings for the specified sensors.");
        if (sensorReadings == null || !sensorReadings.Any())
        {
            return NotFound("No sensor readings found for the specified plant and sensors.");
        }

        return Ok(sensorReadings);
    }
}