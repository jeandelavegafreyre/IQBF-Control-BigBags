using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IQBF.API.Hubs;

[Authorize]
public class OperationsHub : Hub
{
}
