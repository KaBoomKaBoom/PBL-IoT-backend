public class SensorReading
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public Sensor? Sensor { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}