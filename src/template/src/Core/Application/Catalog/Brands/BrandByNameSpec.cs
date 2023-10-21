using Genocs.Microservice.Template.Domain.Catalog;

namespace Genocs.Microservice.Template.Application.Catalog.Brands;

public class BrandByNameSpec : Specification<Brand>, ISingleResultSpecification
{
    public BrandByNameSpec(string name) =>
        Query.Where(b => b.Name == name);
}