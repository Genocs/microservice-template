using System.Collections.ObjectModel;

namespace Genocs.Microservice.Template.Shared.Authorization;

public static class GNXAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class GNXResource
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Products = nameof(Products);
    public const string Brands = nameof(Brands);
}

public static class GNXPermissions
{
    private static readonly GNXPermission[] _all = new GNXPermission[]
    {
        new("View Dashboard", GNXAction.View, GNXResource.Dashboard),
        new("View Hangfire", GNXAction.View, GNXResource.Hangfire),
        new("View Users", GNXAction.View, GNXResource.Users),
        new("Search Users", GNXAction.Search, GNXResource.Users),
        new("Create Users", GNXAction.Create, GNXResource.Users),
        new("Update Users", GNXAction.Update, GNXResource.Users),
        new("Delete Users", GNXAction.Delete, GNXResource.Users),
        new("Export Users", GNXAction.Export, GNXResource.Users),
        new("View UserRoles", GNXAction.View, GNXResource.UserRoles),
        new("Update UserRoles", GNXAction.Update, GNXResource.UserRoles),
        new("View Roles", GNXAction.View, GNXResource.Roles),
        new("Create Roles", GNXAction.Create, GNXResource.Roles),
        new("Update Roles", GNXAction.Update, GNXResource.Roles),
        new("Delete Roles", GNXAction.Delete, GNXResource.Roles),
        new("View RoleClaims", GNXAction.View, GNXResource.RoleClaims),
        new("Update RoleClaims", GNXAction.Update, GNXResource.RoleClaims),
        new("View Products", GNXAction.View, GNXResource.Products, IsBasic: true),
        new("Search Products", GNXAction.Search, GNXResource.Products, IsBasic: true),
        new("Create Products", GNXAction.Create, GNXResource.Products),
        new("Update Products", GNXAction.Update, GNXResource.Products),
        new("Delete Products", GNXAction.Delete, GNXResource.Products),
        new("Export Products", GNXAction.Export, GNXResource.Products),
        new("View Brands", GNXAction.View, GNXResource.Brands, IsBasic: true),
        new("Search Brands", GNXAction.Search, GNXResource.Brands, IsBasic: true),
        new("Create Brands", GNXAction.Create, GNXResource.Brands),
        new("Update Brands", GNXAction.Update, GNXResource.Brands),
        new("Delete Brands", GNXAction.Delete, GNXResource.Brands),
        new("Generate Brands", GNXAction.Generate, GNXResource.Brands),
        new("Clean Brands", GNXAction.Clean, GNXResource.Brands),
        new("View Tenants", GNXAction.View, GNXResource.Tenants, IsRoot: true),
        new("Create Tenants", GNXAction.Create, GNXResource.Tenants, IsRoot: true),
        new("Update Tenants", GNXAction.Update, GNXResource.Tenants, IsRoot: true),
        new("Upgrade Tenant Subscription", GNXAction.UpgradeSubscription, GNXResource.Tenants, IsRoot: true)
    };

    public static IReadOnlyList<GNXPermission> All { get; } = new ReadOnlyCollection<GNXPermission>(_all);
    public static IReadOnlyList<GNXPermission> Root { get; } = new ReadOnlyCollection<GNXPermission>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<GNXPermission> Admin { get; } = new ReadOnlyCollection<GNXPermission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<GNXPermission> Basic { get; } = new ReadOnlyCollection<GNXPermission>(_all.Where(p => p.IsBasic).ToArray());
}

public record GNXPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}
