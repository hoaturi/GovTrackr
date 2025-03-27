using FluentResults;

namespace GovTrackr.Application.Common.Models.Errors;

public abstract class ApplicationError(string message) : Error(message);