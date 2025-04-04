using System.ComponentModel.DataAnnotations;

namespace GovTrackr.DocumentScraping.Worker.Configurations.Options;

internal class ConnectionStringsOptions
{
    internal const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; init; } = null!;
    [Required] public string AzureServiceBus { get; init; } = null!;
}