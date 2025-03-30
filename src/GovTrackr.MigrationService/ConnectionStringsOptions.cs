using System.ComponentModel.DataAnnotations;

namespace GovTrackr.MigrationService;

internal class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; set; } = null!;
}