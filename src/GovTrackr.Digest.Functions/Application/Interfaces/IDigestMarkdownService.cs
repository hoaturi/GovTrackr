namespace GovTrackr.Digest.Functions.Application.Interfaces;

public interface IDigestMarkdownService
{
    Task CreateDigestAsync(CancellationToken cancellationToken);
}