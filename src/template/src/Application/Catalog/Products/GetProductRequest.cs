﻿namespace Genocs.Microservice.Template.Application.Catalog.Products;

public class GetProductRequest : IRequest<ProductDetailsDto>
{
    public DefaultIdType Id { get; set; }

    public GetProductRequest(DefaultIdType id) => Id = id;
}

public class GetProductRequestHandler : IRequestHandler<GetProductRequest, ProductDetailsDto>
{
    private readonly IRepository<Product> _repository;
    private readonly IStringLocalizer _t;

    public GetProductRequestHandler(IRepository<Product> repository, IStringLocalizer<GetProductRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<ProductDetailsDto> Handle(GetProductRequest request, CancellationToken cancellationToken) =>
        await _repository.FirstOrDefaultAsync(
            (ISpecification<Product, ProductDetailsDto>)new ProductByIdWithBrandSpec(request.Id), cancellationToken)
        ?? throw new NotFoundException(_t["Product {0} Not Found.", request.Id]);
}