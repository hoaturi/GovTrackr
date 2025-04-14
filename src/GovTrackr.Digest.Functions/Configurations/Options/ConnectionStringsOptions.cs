using System.ComponentModel.DataAnnotations;

namespace GovTrackr.Digest.Functions.Configurations.Options;

public class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; set; } = null!;
}