using Finbuckle.MultiTenant.Abstractions;
using Genocs.Microservice.Template.Infrastructure.Identity;
using Genocs.Microservice.Template.Infrastructure.Multitenancy;
using Genocs.Microservice.Template.Infrastructure.Persistence.Context;
using Genocs.Microservice.Template.Shared.Authorization;
using Genocs.Microservice.Template.Shared.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genocs.Microservice.Template.Infrastructure.Persistence.Initialization;

internal class ApplicationDbSeeder
{
    private readonly GNXTenantInfo? _currentTenant;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CustomSeederRunner _seederRunner;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(IMultiTenantContextAccessor<GNXTenantInfo> multiTenantContextAccessor, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, CustomSeederRunner seederRunner, ILogger<ApplicationDbSeeder> logger)
    {
        if (multiTenantContextAccessor is null)
        {
            throw new ArgumentNullException(nameof(multiTenantContextAccessor));
        }

        if (multiTenantContextAccessor.MultiTenantContext is null)
        {
            throw new ArgumentNullException(nameof(multiTenantContextAccessor.MultiTenantContext));
        }

        if (multiTenantContextAccessor?.MultiTenantContext?.TenantInfo is null)
        {
            throw new ArgumentNullException(nameof(multiTenantContextAccessor.MultiTenantContext.TenantInfo));
        }

        _currentTenant = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _seederRunner = seederRunner ?? throw new ArgumentNullException(nameof(seederRunner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        await SeedRolesAsync(dbContext);
        await SeedAdminUserAsync();
        await _seederRunner.RunSeedersAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(ApplicationDbContext dbContext)
    {
        foreach (string roleName in GNXRoles.DefaultRoles)
        {
            if (await _roleManager.Roles.SingleOrDefaultAsync(r => r.Name == roleName)
                is not ApplicationRole role)
            {
                // Create the role
                _logger.LogInformation("Seeding {role} Role for '{tenantId}' Tenant.", roleName, _currentTenant.Id);
                role = new ApplicationRole(roleName, $"{roleName} Role for {_currentTenant.Id} Tenant");
                await _roleManager.CreateAsync(role);
            }

            // Assign permissions
            if (roleName == GNXRoles.Basic)
            {
                await AssignPermissionsToRoleAsync(dbContext, GNXPermissions.Basic, role);
            }
            else if (roleName == GNXRoles.Admin)
            {
                await AssignPermissionsToRoleAsync(dbContext, GNXPermissions.Admin, role);

                if (_currentTenant.Id == MultitenancyConstants.Root.Id)
                {
                    await AssignPermissionsToRoleAsync(dbContext, GNXPermissions.Root, role);
                }
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(ApplicationDbContext dbContext, IReadOnlyList<GNXPermission> permissions, ApplicationRole role)
    {
        var currentClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            if (!currentClaims.Any(c => c.Type == GNXClaims.Permission && c.Value == permission.Name))
            {
                _logger.LogInformation("Seeding {role} Permission '{permission}' for '{tenantId}' Tenant.", role.Name, permission.Name, _currentTenant.Id);
                dbContext.RoleClaims.Add(new ApplicationRoleClaim
                {
                    RoleId = role.Id,
                    ClaimType = GNXClaims.Permission,
                    ClaimValue = permission.Name,
                    CreatedBy = "ApplicationDbSeeder"
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentTenant.Id) || string.IsNullOrWhiteSpace(_currentTenant.AdminEmail))
        {
            return;
        }

        if (await _userManager.Users.FirstOrDefaultAsync(u => u.Email == _currentTenant.AdminEmail)
            is not ApplicationUser adminUser)
        {
            string adminUserName = $"{_currentTenant.Id.Trim()}.{GNXRoles.Admin}".ToLowerInvariant();
            adminUser = new ApplicationUser
            {
                FirstName = _currentTenant.Id.Trim().ToLowerInvariant(),
                LastName = GNXRoles.Admin,
                Email = _currentTenant.AdminEmail,
                UserName = adminUserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = _currentTenant.AdminEmail?.ToUpperInvariant(),
                NormalizedUserName = adminUserName.ToUpperInvariant(),
                IsActive = true
            };

            _logger.LogInformation("Seeding Default Admin User for '{tenantId}' Tenant.", _currentTenant.Id);
            var password = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = password.HashPassword(adminUser, MultitenancyConstants.DefaultPassword);
            await _userManager.CreateAsync(adminUser);
        }

        // Assign role to user
        if (!await _userManager.IsInRoleAsync(adminUser, GNXRoles.Admin))
        {
            _logger.LogInformation("Assigning Admin Role to Admin User for '{tenantId}' Tenant.", _currentTenant.Id);
            await _userManager.AddToRoleAsync(adminUser, GNXRoles.Admin);
        }
    }
}