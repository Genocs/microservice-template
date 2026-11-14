using Genocs.Microservice.Template.Application.Dashboard;
using Genocs.Microservice.Template.Contracts.Authorization;

namespace Genocs.Microservice.Template.WebApi.Controllers.Dashboard;

public class DashboardController : VersionedApiController
{
    [HttpGet]
    [MustHavePermission(GNXAction.View, GNXResource.Dashboard)]
    [OpenApiOperation("Get statistics for the dashboard.", "")]
    public Task<StatsDto> GetAsync()
    {
        return Mediator.Send(new GetStatsRequest());
    }
}