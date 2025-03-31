using System.ComponentModel.DataAnnotations;

namespace GovTrackr.ScraperService.Configurations.Options;

internal class ConnectionStringsOptions
{
    internal const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; init; } = null!;
}