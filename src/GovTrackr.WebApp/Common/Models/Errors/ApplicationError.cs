using FluentResults;

namespace GovTrackr.Application.Common.Models.Errors;

public abstract class ApplicationError : Error
{
    protected ApplicationError(string message) : base(message)
    {
    }

    protected ApplicationError(string message, string code) : base(message)
    {
        Metadata.Add("Code", code);
    }
}