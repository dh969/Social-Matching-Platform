using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using udemyCourse.Extensions;

namespace udemyCourse.SignalR
{
    public class PresenceHub : Hub//have to add no nuget packages for this

    {
        private readonly PresenceTracker tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            this.tracker = tracker;
        }
        [Authorize]//websockets are used for connectig to signalr
        public override async Task OnConnectedAsync()
        {
            await tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());//everyone who is connected to the hub except the caller
            var currentUsers = await tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            var currentUsers = await tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
