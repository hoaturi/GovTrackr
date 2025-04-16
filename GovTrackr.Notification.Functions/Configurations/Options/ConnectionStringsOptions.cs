namespace GovTrackr.Notification.Functions.Configurations.Options;

public class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    public string GovTrackrDb { get; set; } = null!;
    public string AzureServiceBus { get; set; } = null!;
}