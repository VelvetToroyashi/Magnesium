namespace Magnesium.Data;

public class UserState 
{
    public required ulong ID { get; set; }
    public required ulong ChannelID { get; set; }
    public required ulong LastMessageID { get; set; }
}