namespace GovTrackr.Digest.Functions.Configurations.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string SenderEmail { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}