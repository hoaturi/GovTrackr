namespace Shared.Models.Errors;

public sealed class NotFoundError : ApplicationError
{
    public NotFoundError(string message = "Resource not found") : base(message)
    {
    }

    public NotFoundError(string message, string code) : base(message, code)
    {
    }
}