using Memento.Bot;
using Memento.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using System;
using System.Collections.Generic;
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

        public BotController(IDecksService decksService)
        {
            this.decksService = decksService;
        }

        // POST: api/Bot
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                if (message.From.ChannelId == "skype" || true)
                {
                    var decks = await decksService.GetDecksAsync(message.Text);
                    var dialog = new Dialog { Decks = decks.ToList() };

                    message.BotConversationData = decks;

                    return await Conversation.SendAsync(message, () => dialog);
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
