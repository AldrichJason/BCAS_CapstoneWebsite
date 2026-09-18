namespace BCAS.Api.Model;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string AcademicHead = "AcademicHead";
    public const string AdminOfficeRegistrar = "AdminOfficeRegistrar";
    public const string VpOfOperations = "VpOfOperations";
}
