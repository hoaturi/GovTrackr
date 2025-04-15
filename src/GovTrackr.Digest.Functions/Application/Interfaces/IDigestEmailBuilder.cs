using GovTrackr.Digest.Functions.Application.Dtos;

namespace GovTrackr.Digest.Functions.Application.Interfaces;

public interface IDigestEmailBuilder
{
    Task<string> GetDigestContentAsync(DigestEmailTemplateDto dto, CancellationToken cancellationToken);

    string InjectUnsubscribeLink(string template, string url);
}