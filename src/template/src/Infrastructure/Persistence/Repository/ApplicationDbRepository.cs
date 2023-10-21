using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Genocs.Microservice.Template.Application.Common.Persistence;
using Genocs.Microservice.Template.Domain.Common.Contracts;
using Genocs.Microservice.Template.Infrastructure.Persistence.Context;
using Mapster;

namespace Genocs.Microservice.Template.Infrastructure.Persistence.Repository;

/// <summary>
/// Inherited from Ardalis.Specification's RepositoryBase of T.
/// </summary>
/// <typeparam name="T">Type of entity.</typeparam>
public class ApplicationDbRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    public ApplicationDbRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    // We override the default behavior when mapping to a dto.
    // We're using Mapster's ProjectToType here to immediately map the result from the database.
    // This is only done when no Selector is defined, so regular specifications with a selector also still work.
    protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
        => specification.Selector is not null
            ? base.ApplySpecification(specification)
            : ApplySpecification(specification, false)
                .ProjectToType<TResult>();
}