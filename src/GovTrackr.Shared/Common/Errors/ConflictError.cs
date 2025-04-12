using Shared.Abstractions.Errors;

namespace Shared.Common.Errors;

public class ConflictError : ApplicationError
{

    public ConflictError(string message) : base(message)
    {
    }

    public ConflictError(string message, string code) : base(message, code)
    {
    }
}