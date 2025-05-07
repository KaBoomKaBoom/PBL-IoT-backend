public class Plant
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public List<int> SensorIds { get; set; } = new List<int>();
}