using Microsoft.AspNetCore.SignalR;

namespace CheckList.Hubs
{
    public class ChecklistHub : Hub
    {
        // Los clientes se unen al grupo "checklist" al conectarse
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "checklist");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "checklist");
            await base.OnDisconnectedAsync(exception);
        }
    }
}