using Genocs.Microservice.Template.Application.Common.Models;

namespace Genocs.Microservice.Template.Application.Identity.Users;

public class UserListFilter : PaginationFilter
{
    public bool? IsActive { get; set; }
}