using System.ComponentModel.DataAnnotations;

namespace GovTrackr.ScraperService.Configurations.Options;

internal class ScrapersOptions
{
    internal const string SectionName = "Scrapers";

    [Required] public int MaxConcurrentPages { get; init; } = 5;
}