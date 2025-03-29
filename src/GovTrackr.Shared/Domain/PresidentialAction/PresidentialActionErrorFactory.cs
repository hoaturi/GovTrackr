using Shared.Models.Errors;

namespace Shared.Domain.PresidentialAction;

public static class PresidentialActionErrorFactory
{
    public static NotFoundError PresidentialActionNotFound(Guid id)
    {
        return new NotFoundError($"Presidential Action with ID '{id}' was not found",
            ErrorCodes.PresidentialActionNotFound);
    }
}