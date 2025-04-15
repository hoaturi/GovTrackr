using GovTrackr.Digest.Functions.Application.Dtos;

namespace GovTrackr.Digest.Functions.Application.Interfaces;

public interface IDigestMarkdownBuilder
{
    string BuildMarkdown(List<PresidentialActionDto> presidentialActions, DateTime startDate);
}