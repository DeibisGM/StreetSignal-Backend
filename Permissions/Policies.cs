namespace StreetSignalApi.Permissions;

public static class Policies
{
    public const string StaffOnly       = "StaffOnly";        // Staff or Admin
    public const string CitizenOnly     = "CitizenOnly";      // Citizen role only
    public const string AuthenticatedUser = "AuthenticatedUser";
}
