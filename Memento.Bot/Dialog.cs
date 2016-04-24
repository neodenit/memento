using Memento.Interfaces;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Bot
{
    [Serializable]
    public class Dialog : IDialog
    {
        private Tuple<int, string>[] decksCache;

        public async Task StartAsync(IDialogContext context)
        {
            await Task.Run(() => context.Wait(MessageReceivedAsync));
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;

            decksCache = message.GetBotPerUserInConversationData<Tuple<int, string>[]>("decks");
            
            var titles = from x in decksCache select x.Item2;
            var menuItems = titles.Select((d, i) => $"{i + 1}) {d}");

            var delimeter = Environment.NewLine + Environment.NewLine;
            var decksList = string.Join(delimeter, menuItems);

            await context.PostAsync(decksList);

            context.Wait(MessageReceivedAsync);
        }
    }
}
