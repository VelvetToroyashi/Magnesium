namespace Magnesium.Data;

/// <summary>
/// Represents a channel to track pings for.
/// </summary>
public class TrackedChannel
{
    public int Id { get; set; }
    public required bool Global { get; set; }
    public required ulong GuildID { get; set; }
    public required ulong ChannelID { get; set; }
    public required ulong? TrackingUserID { get; set; }

    public UserPreference? TrackingUserPreference { get; set; }
}