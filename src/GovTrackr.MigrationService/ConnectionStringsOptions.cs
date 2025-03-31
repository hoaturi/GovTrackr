using System.ComponentModel.DataAnnotations;

namespace GovTrackr.MigrationService;

internal class ConnectionStringsOptions
{
    internal const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; init; } = null!;
}