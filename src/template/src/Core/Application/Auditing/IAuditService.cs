using Genocs.Microservice.Template.Application.Common.Interfaces;

namespace Genocs.Microservice.Template.Application.Auditing;

public interface IAuditService : ITransientService
{
    Task<List<AuditDto>> GetUserTrailsAsync(DefaultIdType userId);
}