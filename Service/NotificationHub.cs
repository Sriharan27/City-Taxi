using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace City_Taxi.Service
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext.Session.GetInt32("UserID") is int userId)
            {
                // Add the connected user to a group based on their UserID
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext.Session.GetInt32("UserID") is int userId)
            {
                // Remove the user from the group when they disconnect
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Send notification to a specific driver
        public async Task NotifyDriver(int driverId, string message)
        {
            await Clients.Group(driverId.ToString()).SendAsync("ReceiveNotification", message);
        }
    }
}
