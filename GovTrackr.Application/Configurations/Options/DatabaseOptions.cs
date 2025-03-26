using System.ComponentModel.DataAnnotations;

namespace GovTrackr.Application.Configurations.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required] public string ConnectionString { get; set; } = default!;
}