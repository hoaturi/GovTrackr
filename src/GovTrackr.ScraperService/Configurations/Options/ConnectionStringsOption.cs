using System.ComponentModel.DataAnnotations;

namespace GovTrackr.ScraperService.Configurations.Options;

internal class ConnectionStringsOption
{
    internal const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; init; } = null!;
    [Required] public string AzureServiceBus { get; init; } = null!;
}