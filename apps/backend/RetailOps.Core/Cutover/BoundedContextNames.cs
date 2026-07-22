namespace RetailOps.Core.Cutover;

public static class BoundedContextNames
{
    public const string Platform = "platform";
    public const string Identity = "auth";
    public const string Settings = "settings";
    public const string Catalog = "catalog";
    public const string Crm = "crm";
    public const string Sales = "sales";
    public const string Finance = "finance";
    public const string Returns = "returns";
    public const string Reporting = "reporting";
    public const string Notifications = "notifications";

    public static readonly string[] MigrationOrder =
    [
        Platform,
        Identity,
        Settings,
        Catalog,
        Crm,
        Sales,
        Finance,
        Returns
    ];

    public static readonly string[] CutoverModuleFlags =
    [
        Identity,
        Settings,
        Catalog,
        Crm,
        Sales,
        Finance,
        Returns,
        Reporting,
        Notifications
    ];
}
