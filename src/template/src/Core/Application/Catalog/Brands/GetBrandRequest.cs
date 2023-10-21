using Genocs.Microservice.Template.Application.Common.Persistence;
using Genocs.Microservice.Template.Domain.Catalog;

namespace Genocs.Microservice.Template.Application.Catalog.Brands;

public class GetBrandRequest : IRequest<BrandDto>
{
    public DefaultIdType Id { get; set; }

    public GetBrandRequest(DefaultIdType id) => Id = id;
}

public class BrandByIdSpec : Specification<Brand, BrandDto>, ISingleResultSpecification
{
    public BrandByIdSpec(DefaultIdType id) =>
        Query.Where(p => p.Id == id);
}

public class GetBrandRequestHandler : IRequestHandler<GetBrandRequest, BrandDto>
{
    private readonly IRepository<Brand> _repository;
    private readonly IStringLocalizer _t;

    public GetBrandRequestHandler(IRepository<Brand> repository, IStringLocalizer<GetBrandRequestHandler> localizer) => (_repository, _t) = (repository, localizer);

    public async Task<BrandDto> Handle(GetBrandRequest request, CancellationToken cancellationToken) =>
        await _repository.FirstOrDefaultAsync(
            (ISpecification<Brand, BrandDto>)new BrandByIdSpec(request.Id), cancellationToken)
        ?? throw new NotFoundException(_t["Brand {0} Not Found.", request.Id]);
}