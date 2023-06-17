namespace Magnesium.Data;

public class UserPreference 
{
    public required ulong ID { get; set; }
    public bool Notify { get; set; } = true;

    public List<TrackedChannel> PersonallyTrackedChannels { get; set; } = new();
}