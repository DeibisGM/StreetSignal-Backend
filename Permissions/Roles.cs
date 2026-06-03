namespace StreetSignalApi.Permissions;

public static class Roles
{
    public const string Citizen = "Citizen";
    public const string Staff   = "Staff";
    public const string Admin   = "Admin";

    public const string StaffOrAdmin = Staff + "," + Admin;
}
