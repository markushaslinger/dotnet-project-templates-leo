namespace LeoWebApi.Persistence.Model;

public class Launch
{
    public Guid Id { get; set; }
    public LocalDate PlannedLaunchDate { get; set; }
    public Instant? LaunchDate { get; set; }
    public required string LaunchSite { get; set; }
    public required string Customer { get; set; }
    public int RocketId { get; set; }
    public required Rocket Rocket { get; set; }
    public required List<Payload> Payloads { get; set; } = [];
}
