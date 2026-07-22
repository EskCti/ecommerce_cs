namespace RetailOps.Identity.Core.Application;

public static class AuthErrors
{
    public const string InvalidCredentials = "Invalid credentials.";
    public const string InactiveAccount = "User account is inactive.";
    public const string MissingPermissions = "User has no permissions assigned.";
    public const string TrialExpired = "Trial period has expired.";
    public const string LegacyPasswordResetRequired =
        "Password must be reset. Contact your administrator to migrate to a secure password.";
}
