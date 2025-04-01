using System.ComponentModel.DataAnnotations;

namespace GovTrackr.Application.Configurations.Options;

internal class ConnectionStringsOption
{
    internal const string SectionName = "ConnectionStrings";

    [Required] public string GovTrackrDb { get; init; } = null!;
}