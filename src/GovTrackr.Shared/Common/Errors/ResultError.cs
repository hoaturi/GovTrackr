using FluentResults;

namespace Shared.Common.Errors;

public abstract class ResultError : Error
{
    protected ResultError(string message) : base(message)
    {
    }

    protected ResultError(string message, string code) : base(message)
    {
        Metadata.Add("Code", code);
    }
}