using System.ComponentModel.DataAnnotations;

namespace GovTrackr.Digest.Functions.Configurations.Options;

public class AwsOptions
{
    public const string SectionName = "AWS";

    [Required] public string Region { get; set; } = null!;
    [Required] public string AccessKey { get; set; } = null!;
    [Required] public string SecretKey { get; set; } = null!;
}