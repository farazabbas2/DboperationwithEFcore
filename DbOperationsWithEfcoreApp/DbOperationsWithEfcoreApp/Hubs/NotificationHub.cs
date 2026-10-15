using Microsoft.AspNetCore.SignalR;

namespace DbOperationsWithEfcoreApp.Hubs
{
    public class NotificationHub : Hub
    {


        // Jab koi frontend user connect karega (Optional, tracking ke liye)
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        // Jab koi disconnect karega
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
