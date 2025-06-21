public class Alert
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }


    // Constructor to initialize default values
    public Alert()
    {
        UserId = 34; // Default user ID, can be set later
        CreatedAt = DateTime.UtcNow;
        Message = string.Empty; // Default message, can be set later
    }
}