using System.ComponentModel.DataAnnotations;

namespace GovTrackr.MigrationService;

public class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; set; } = null!;
}