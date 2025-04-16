using FluentResults;
using MediatR;
using Shared.Domain.Common;

namespace GovTrackr.Api.Features.Notifications.RegisterPushPreferences;

public class RegisterPushPreferencesCommand : IRequest<Result<Unit>>
{
    public string Token { get; set; } = null!;
    public List<DocumentSubCategoryType> DocumentType { get; set; } = [];
}