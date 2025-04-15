namespace GovTrackr.Digest.Functions.Application.Interfaces;

public interface IDigestEmailService
{
    Task SendDigestEmailAsync(
        CancellationToken cancellationToken);
}