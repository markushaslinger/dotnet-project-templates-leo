namespace LeoWebApi.Core.Util;

public sealed class Settings
{
    public const string SectionKey = "General";
    public double MaxPayloadWeight { get; set; }
    public List<string> ValidLaunchSites { get; set; } = [];
    public required string ClientOrigin { get; set; }
}
