using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Reflection.Metadata.Ecma335;
using udemyCourse.Data;
using udemyCourse.DTOs;
using udemyCourse.Entities;
using udemyCourse.Extensions;
using udemyCourse.Interfaces;

namespace udemyCourse.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IMessageRepository message;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IHubContext<PresenceHub> presenceHub;

        public MessageHub(IMessageRepository message, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHub /*grants access to a particular hub*/)
        {
            this.message = message;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.presenceHub = presenceHub;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await AddToGroup(groupName);
            var messages = await message.GetMessageThread(Context.User.GetUsername(), otherUser);
            await Clients.Group(groupName).SendAsync("RecieveMessageThread", messages);

        }
        public async Task SendMessage(CreateMessageDto messageDto)
        {
            var username = Context.User.GetUsername();
            if (username.ToLower() == messageDto.RecipientUsername.ToLower())
            {
                throw new HubException("You cannot send mesage to yourself");
            }
            var sender = await userRepository.GetUserByUsernameAsync(username);
            var recipient = await userRepository.GetUserByUsernameAsync(messageDto.RecipientUsername);
            if (recipient == null) throw new HubException("Not found user");
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = messageDto.Content
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await this.message.GetMessageGroup(groupName);
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else//if the user is in another hub i.e connected to the application then he will recieve this 
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageRecieved", new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }
            this.message.AddMessage(message);
            if (await this.message.SaveAllAsync())
            {

                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemovefromMessageGroup();
            await base.OnDisconnectedAsync(exception);
        }
        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await message.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                this.message.AddGroup(group);
            }
            group.Connections.Add(connection);
            return await message.SaveAllAsync();
        }
        private async Task RemovefromMessageGroup()
        {
            var connection = await message.GetConnection(Context.ConnectionId);
            message.RemoveConnection(connection);
            await message.SaveAllAsync();
        }

    }
}
