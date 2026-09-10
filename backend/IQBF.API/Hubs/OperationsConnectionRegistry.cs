using System.Collections.Concurrent;
using IQBF.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace IQBF.API.Hubs;

public sealed class OperationsConnectionRegistry : IUserSessionRevoker
{
    private readonly IHubContext<OperationsHub> _hubContext;
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, HubCallerContext>> _connections = new();

    public OperationsConnectionRegistry(IHubContext<OperationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public void Register(Guid userId, string connectionId, HubCallerContext context)
    {
        var userConnections = _connections.GetOrAdd(
            userId,
            _ => new ConcurrentDictionary<string, HubCallerContext>());

        userConnections[connectionId] = context;
    }

    public void Unregister(Guid userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var userConnections))
            return;

        userConnections.TryRemove(connectionId, out _);

        if (userConnections.IsEmpty)
            _connections.TryRemove(userId, out _);
    }

    public async Task RevokeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryRemove(userId, out var userConnections))
            return;

        var connectionIds = userConnections.Keys.ToArray();

        if (connectionIds.Length > 0)
        {
            try
            {
                await _hubContext.Clients.Clients(connectionIds).SendAsync(
                    "SessionRevoked",
                    cancellationToken);
            }
            catch
            {
                // La revocación continúa aunque una conexión ya no esté disponible.
            }
        }

        foreach (var context in userConnections.Values)
        {
            try
            {
                context.Abort();
            }
            catch
            {
                // La conexión puede haberse cerrado entre la lectura y la revocación.
            }
        }
    }
}
