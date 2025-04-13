using System.ComponentModel.DataAnnotations;

namespace GovTrackr.DocumentNotification.Worker.Configurations.Options;

public class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; init; } = null!;

    [Required] public string AzureServiceBus { get; init; } = null!;
}