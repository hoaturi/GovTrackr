using GovTrackr.Application.Common.Models.Errors;

namespace GovTrackr.Application.Domain.PresidentialAction;

public static class PresidentialActionErrorFactory
{
    public static NotFoundError PresidentialActionNotFound(Guid id)
    {
        return new NotFoundError($"Presidential Action with ID '{id}' was not found",
            ErrorCodes.PresidentialActionNotFound);
    }
}