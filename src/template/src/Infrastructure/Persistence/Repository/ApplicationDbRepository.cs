using Ardalis.Specification.EntityFrameworkCore;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Microservice.Template.Infrastructure.Persistence.Context;

namespace Genocs.Microservice.Template.Infrastructure.Persistence.Repository;

public class ApplicationDbRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    public ApplicationDbRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }
}
