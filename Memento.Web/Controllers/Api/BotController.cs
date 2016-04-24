using Memento.Bot;
using Memento.Interfaces;
using Memento.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Memento.Web.Controllers.Api
{
    [BotAuthentication]
    public class BotController : ApiController
    {
        private readonly IDecksService decksService;
        private readonly ApplicationDbContext context;

        public BotController(IDecksService decksService)
        {
            this.decksService = decksService;
            context = ApplicationDbContext.Create();
        }

        // POST: api/Bot
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                if (message.From.ChannelId == "skype" || message.From.ChannelId == "emulator")
                {
                    var skypeName = message.From.Address.Split(':').Last();

                    var user = await context.Users.SingleOrDefaultAsync(x => x.Skype == skypeName);

                    if (user != null)
                    {
                        var userName = user.UserName;
                        var decks = await decksService.GetDecksAsync(userName);
                        var data = decks.Select(x => Tuple.Create(x.ID, x.Title)).ToArray();

                        message.SetBotPerUserInConversationData("decks", data);

                        return await Conversation.SendAsync(message, () => new Dialog());
                    }
                    else
                    {
                        return message.CreateReplyMessage($"Your Skype account is '{message.From.Address}'. You are not registered yet.");
                    }
                }
                else
                {
                    return message.CreateReplyMessage("Only Skype accounts are currently supported.");
                }
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                var reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}
