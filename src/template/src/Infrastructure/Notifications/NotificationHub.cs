using Finbuckle.MultiTenant.Abstractions;
using Genocs.Microservice.Template.Application.Common.Exceptions;
using Genocs.Microservice.Template.Application.Common.Interfaces;
using Genocs.Microservice.Template.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Genocs.Microservice.Template.Infrastructure.Notifications;

[Authorize]
public class NotificationHub : Hub, ITransientService
{
    private readonly ITenantInfo? _currentTenant;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(IMultiTenantContextAccessor<GNXTenantInfo> multiTenantContextAccessor, ILogger<NotificationHub> logger)
    {
        if (multiTenantContextAccessor is null)
        {
            throw new ArgumentNullException(nameof(multiTenantContextAccessor));
        }

        _currentTenant = multiTenantContextAccessor?.MultiTenantContext?.TenantInfo;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentTenant is null)
        {
            throw new UnauthorizedException("Authentication Failed.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"GroupTenant-{_currentTenant.Id}");

        await base.OnConnectedAsync();

        _logger.LogInformation("A client connected to NotificationHub: {connectionId}", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"GroupTenant-{_currentTenant!.Id}");

        await base.OnDisconnectedAsync(exception);

        _logger.LogInformation("A client disconnected from NotificationHub: {connectionId}", Context.ConnectionId);
    }
}