using Genocs.Core.Domain.Entities.Auditing;
using Genocs.Microservice.Template.Infrastructure.Persistence.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Microservice.Template.Infrastructure.Auditing;

public class AuditService(ApplicationDbContext context) : IAuditService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<List<AuditDto>> GetUserTrailsAsync(DefaultIdType userId, CancellationToken cancellationToken = default)
    {
        var trails = await _context.AuditTrails
                                                .Where(a => a.UserId == userId)
                                                .OrderByDescending(a => a.DateTime)
                                                .Take(250)
                                                .ToListAsync(cancellationToken);

        return trails.Adapt<List<AuditDto>>();
    }
}