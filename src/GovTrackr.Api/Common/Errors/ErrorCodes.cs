namespace GovTrackr.Api.Common.Errors;

public static class ErrorCodes

{
    // General Error Codes
    public const string InternalServerError = "GEN001";
    public const string ValidationError = "GEN002";

    // Presidential Action Error Codes
    public const string PresidentialActionNotFound = "PA001";

    // Subscription Error Codes
    public const string EmailAlreadySubscribed = "SUB001";
}