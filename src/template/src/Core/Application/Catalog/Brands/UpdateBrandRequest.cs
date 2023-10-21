namespace Genocs.Microservice.Template.Application.Catalog.Brands;

public class UpdateBrandRequest : IRequest<DefaultIdType>
{
    public DefaultIdType Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateBrandRequestValidator : CustomValidator<UpdateBrandRequest>
{
    public UpdateBrandRequestValidator(IRepository<Brand> repository, IStringLocalizer<UpdateBrandRequestValidator> T) =>
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(75)
            .MustAsync(async (brand, name, ct) =>
                    await repository.FirstOrDefaultAsync(new BrandByNameSpec(name), ct)
                        is not Brand existingBrand || existingBrand.Id == brand.Id)
                .WithMessage((_, name) => T["Brand {0} already Exists.", name]);
}

public class UpdateBrandRequestHandler : IRequestHandler<UpdateBrandRequest, DefaultIdType>
{
    // Add Domain Events automatically by using IRepositoryWithEvents
    private readonly IRepositoryWithEvents<Brand> _repository;
    private readonly IStringLocalizer _t;

    public UpdateBrandRequestHandler(IRepositoryWithEvents<Brand> repository, IStringLocalizer<UpdateBrandRequestHandler> localizer) =>
        (_repository, _t) = (repository, localizer);

    public async Task<DefaultIdType> Handle(UpdateBrandRequest request, CancellationToken cancellationToken)
    {
        var brand = await _repository.GetByIdAsync(request.Id, cancellationToken);

        _ = brand
        ?? throw new NotFoundException(_t["Brand {0} Not Found.", request.Id]);

        brand.Update(request.Name, request.Description);

        await _repository.UpdateAsync(brand, cancellationToken);

        return request.Id;
    }
}