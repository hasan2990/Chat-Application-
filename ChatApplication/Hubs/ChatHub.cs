using Microsoft.AspNetCore.SignalR;

namespace ChatApplication.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IDictionary<string,UserRoomConnection> _connections;

        public ChatHub(IDictionary<string, UserRoomConnection> connections)
        {
            _connections = connections;
        }
        public async Task JoinRoom(UserRoomConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room!);
            _connections[Context.ConnectionId] = userConnection;
            await Clients.Group(userConnection.Room!)
                .SendAsync("ReceiveMessage","Lets Program Bot",$"{userConnection.User} has joined the group.");
            await SendConnectedUser(userConnection.Room!);
        }

        public async Task SendMessage(string message)
        {
            if(_connections.TryGetValue(Context.ConnectionId, out UserRoomConnection userRoomConnection)) 
            {
                await Clients.Group(userRoomConnection.Room!)
                    .SendAsync("ReceiveMessage",userRoomConnection.User,message,DateTime.Now);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (!_connections.TryGetValue(Context.ConnectionId, out UserRoomConnection roomConnection))
            {
                return base.OnDisconnectedAsync(exception);
            }
            Clients.Group(roomConnection.Room!)
                .SendAsync("ReceiveMessage", "Lets Program Bot", $"{roomConnection.User} has left the group.");
            SendConnectedUser(roomConnection.Room!);
             
            return base.OnDisconnectedAsync(exception);
        }

        public Task SendConnectedUser(string room)
        {
            var users = _connections.Values
                .Where(u => u.Room == room)
                .Select(x => x.User);
            return Clients.Group(room).SendAsync("ConnectedUser", users);
        }
    }
}
