using HiveMQtt.MQTT5.Types;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

public class AlertService
{
    private readonly AppDbContext _context;

    public AlertService(AppDbContext context)
    {
        _context = context;
    }

    // Method to create a new alert
    public async Task<Alert> CreateAlertAsync(int sensorId, float value, string message)
    {
        if (sensorId <= 0)
        {
            throw new ArgumentException("Sensor ID must be greater than zero.", nameof(sensorId));
        }

        switch(sensorId)
        {
            case 10:
                if(value < 17)
                {
                    message = $"Temperature is too low";
                }
                else if(value > 30)
                {
                    message = $"Temperature is too high";
                }
                message = "Temperature alert: " + message;
                break;
            case 11:
                if(value < 65)
                {
                    message = $"Humidity is too low";
                }
                else if(value > 85)
                {
                    message = $"Humidity is too high";
                }
                message = "Humidity alert: " + message;
                break;
        }
        var alert = new Alert
        {
            SensorId = sensorId,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        return alert;
    }

    // // Method to get all alerts for a user
    // public async Task<List<Alert>> GetAlertsByUserIdAsync(int userId)
    // {
    //     return await _context.Alerts
    //         .Where(a => a.UserId == userId)
    //         .ToListAsync();
    // }
}