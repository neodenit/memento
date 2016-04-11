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
        public List<IDeck> Decks { get; set; }
        
        public async Task StartAsync(IDialogContext context)
        {
            await Task.Run(() => context.Wait(MessageReceivedAsync));
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;

#if DEBUG
            var userName = message.Text;
#else
            var userName = message.From.Address.Split(':').Last();
#endif

            var titles = Decks.Select((d, i) => $"{i + 1}) {d.Title}");

            var delimeter = Environment.NewLine + Environment.NewLine;
            var decksList = string.Join(delimeter, titles);

            await context.PostAsync(decksList);

            context.Wait(MessageReceivedAsync);
        }
    }
}
