using Genocs.Microservice.Template.Application.Catalog.Brands;
using Genocs.Microservice.Template.Application.Common.Models;
using Genocs.Microservice.Template.Infrastructure.Auth.Permissions;
using Genocs.Microservice.Template.Shared.Authorization;

namespace Genocs.Microservice.Template.WebApi.Controllers.Catalog;

public class BrandsController : VersionedApiController
{
    [HttpPost("search")]
    [MustHavePermission(GNXAction.Search, GNXResource.Brands)]
    [OpenApiOperation("Search brands using available filters.", "")]
    public Task<PaginationResponse<BrandDto>> SearchAsync(SearchBrandsRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpGet("{id:guid}")]
    [MustHavePermission(GNXAction.View, GNXResource.Brands)]
    [OpenApiOperation("Get brand details.", "")]
    public Task<BrandDto> GetAsync(DefaultIdType id)
    {
        return Mediator.Send(new GetBrandRequest(id));
    }

    [HttpPost]
    [MustHavePermission(GNXAction.Create, GNXResource.Brands)]
    [OpenApiOperation("Create a new brand.", "")]
    public Task<DefaultIdType> CreateAsync(CreateBrandRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpPut("{id:guid}")]
    [MustHavePermission(GNXAction.Update, GNXResource.Brands)]
    [OpenApiOperation("Update a brand.", "")]
    public async Task<ActionResult<DefaultIdType>> UpdateAsync(UpdateBrandRequest request, DefaultIdType id)
    {
        return id != request.Id
            ? BadRequest()
            : Ok(await Mediator.Send(request));
    }

    [HttpDelete("{id:guid}")]
    [MustHavePermission(GNXAction.Delete, GNXResource.Brands)]
    [OpenApiOperation("Delete a brand.", "")]
    public Task<DefaultIdType> DeleteAsync(DefaultIdType id)
    {
        return Mediator.Send(new DeleteBrandRequest(id));
    }

    [HttpPost("generate-random")]
    [MustHavePermission(GNXAction.Generate, GNXResource.Brands)]
    [OpenApiOperation("Generate a number of random brands.", "")]
    public Task<string> GenerateRandomAsync(GenerateRandomBrandRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpDelete("delete-random")]
    [MustHavePermission(GNXAction.Clean, GNXResource.Brands)]
    [OpenApiOperation("Delete the brands generated with the generate-random call.", "")]
    [ApiConventionMethod(typeof(GNXApiConventions), nameof(GNXApiConventions.Search))]
    public Task<string> DeleteRandomAsync()
    {
        return Mediator.Send(new DeleteRandomBrandRequest());
    }
}