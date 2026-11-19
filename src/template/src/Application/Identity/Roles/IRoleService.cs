using Genocs.Common.Dependency;

namespace Genocs.Microservice.Template.Application.Identity.Roles;

public interface IRoleService : ITransientDependency
{
    Task<List<RoleDto>> GetListAsync(CancellationToken cancellationToken = default);

    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string roleName, string? excludeId, CancellationToken cancellationToken = default);

    Task<RoleDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<RoleDto> GetByIdWithPermissionsAsync(string roleId, CancellationToken cancellationToken = default);

    Task<string> CreateOrUpdateAsync(CreateOrUpdateRoleRequest request, CancellationToken cancellationToken = default);

    Task<string> UpdatePermissionsAsync(UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default);

    Task<string> DeleteAsync(string id, CancellationToken cancellationToken = default);
}