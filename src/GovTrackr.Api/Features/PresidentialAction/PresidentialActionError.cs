using GovTrackr.Api.Common.Errors;

namespace GovTrackr.Api.Features.PresidentialAction;

public static class PresidentialActionError
{
    public static NotFoundError PresidentialActionNotFound(Guid id)
    {
        return new NotFoundError($"Presidential Action with ID '{id}' was not found",
            ErrorCodes.PresidentialActionNotFound);
    }
}