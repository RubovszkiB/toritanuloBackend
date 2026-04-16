namespace toritanulo.Helpers;

public static class RoleHelper
{
    public const string Admin = "Admin";
    public const string Student = "Student";

    public static bool IsValidRole(string? role)
    {
        return role == Admin || role == Student;
    }

    public static string NormalizeRole(string? role)
    {
        if (string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase))
        {
            return Admin;
        }

        if (string.Equals(role, Student, StringComparison.OrdinalIgnoreCase))
        {
            return Student;
        }

        return string.Empty;
    }
}
