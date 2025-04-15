namespace GovTrackr.Digest.Functions.Application.Interfaces;

public interface IDigestService
{
    Task SendDigestEmailAsync(
        CancellationToken cancellationToken);
}