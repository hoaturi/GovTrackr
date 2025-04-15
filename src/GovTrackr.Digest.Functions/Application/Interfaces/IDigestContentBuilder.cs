namespace GovTrackr.Digest.Functions.Application.Interfaces;

public interface IDigestContentBuilder
{
    Task<string?> BuildMarkdownDigestContentAsync(
        CancellationToken cancellationToken);
}