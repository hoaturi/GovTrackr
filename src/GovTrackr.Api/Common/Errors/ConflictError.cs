using Shared.Common.Errors;

namespace GovTrackr.Api.Common.Errors;

public class ConflictError : ResultError
{

    public ConflictError(string message) : base(message)
    {
    }

    public ConflictError(string message, string code) : base(message, code)
    {
    }
}