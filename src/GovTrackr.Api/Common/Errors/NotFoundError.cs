using Shared.Common.Errors;

namespace GovTrackr.Api.Common.Errors;

public sealed class NotFoundError : ResultError
{
    public NotFoundError(string message = "Resource not found") : base(message)
    {
    }

    public NotFoundError(string message, string code) : base(message, code)
    {
    }
}