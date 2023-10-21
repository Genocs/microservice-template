using Genocs.Microservice.Template.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Genocs.Microservice.Template.Infrastructure.Auth.Permissions;

public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string action, string resource) =>
        Policy = FSHPermission.NameFor(action, resource);
}