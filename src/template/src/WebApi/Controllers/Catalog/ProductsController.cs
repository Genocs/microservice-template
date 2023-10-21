using Genocs.Microservice.Template.Application.Catalog.Products;
using Genocs.Microservice.Template.Application.Common.Models;
using Genocs.Microservice.Template.Infrastructure.Auth.Permissions;
using Genocs.Microservice.Template.Shared.Authorization;
using Genocs.Microservice.Template.WebApi.Controllers;

namespace Genocs.Microservice.Template.WebApi.Controllers.Catalog;

public class ProductsController : VersionedApiController
{
    [HttpPost("search")]
    [MustHavePermission(GNXAction.Search, GNXResource.Products)]
    [OpenApiOperation("Search products using available filters.", "")]
    public Task<PaginationResponse<ProductDto>> SearchAsync(SearchProductsRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpGet("{id:guid}")]
    [MustHavePermission(GNXAction.View, GNXResource.Products)]
    [OpenApiOperation("Get product details.", "")]
    public Task<ProductDetailsDto> GetAsync(DefaultIdType id)
    {
        return Mediator.Send(new GetProductRequest(id));
    }

    [HttpGet("dapper")]
    [MustHavePermission(GNXAction.View, GNXResource.Products)]
    [OpenApiOperation("Get product details via dapper.", "")]
    public Task<ProductDto> GetDapperAsync(DefaultIdType id)
    {
        return Mediator.Send(new GetProductViaDapperRequest(id));
    }

    [HttpPost]
    [MustHavePermission(GNXAction.Create, GNXResource.Products)]
    [OpenApiOperation("Create a new product.", "")]
    public Task<DefaultIdType> CreateAsync(CreateProductRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpPut("{id:guid}")]
    [MustHavePermission(GNXAction.Update, GNXResource.Products)]
    [OpenApiOperation("Update a product.", "")]
    public async Task<ActionResult<DefaultIdType>> UpdateAsync(UpdateProductRequest request, DefaultIdType id)
    {
        return id != request.Id
            ? BadRequest()
            : Ok(await Mediator.Send(request));
    }

    [HttpDelete("{id:guid}")]
    [MustHavePermission(GNXAction.Delete, GNXResource.Products)]
    [OpenApiOperation("Delete a product.", "")]
    public Task<DefaultIdType> DeleteAsync(DefaultIdType id)
    {
        return Mediator.Send(new DeleteProductRequest(id));
    }

    [HttpPost("export")]
    [MustHavePermission(GNXAction.Export, GNXResource.Products)]
    [OpenApiOperation("Export a products.", "")]
    public async Task<FileResult> ExportAsync(ExportProductsRequest filter)
    {
        var result = await Mediator.Send(filter);
        return File(result, "application/octet-stream", "ProductExports");
    }
}