using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Neodenit.Memento.Bot
{
    [Serializable]
    public class Dialog : IDialog
    {
        public async Task StartAsync(IDialogContext context)
        {
            await Task.Run(() => context.Wait(MessageReceivedAsync));
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;

            var response = message.GetBotPerUserInConversationData<string>("response");

            await context.PostAsync(response);

            context.Wait(MessageReceivedAsync);
        }
    }
}
