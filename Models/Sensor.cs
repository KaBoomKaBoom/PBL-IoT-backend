public class Sensor
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public int SensorTypeId { get; set; }
    public SensorType? SensorType { get; set; }
}