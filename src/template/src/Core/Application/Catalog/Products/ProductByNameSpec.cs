using Genocs.Microservice.Template.Domain.Catalog;

namespace Genocs.Microservice.Template.Application.Catalog.Products;

public class ProductByNameSpec : Specification<Product>, ISingleResultSpecification
{
    public ProductByNameSpec(string name) =>
        Query.Where(p => p.Name == name);
}