using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;
using Neodenit.Memento.Common;
using Neodenit.Memento.Interfaces;
using Neodenit.Memento.Models.Enums;
using Neodenit.Memento.Models.ViewModels;
using Neodenit.Memento.Web.Data;

namespace Neodenit.Memento.Web.Controllers.Api
{
    [BotAuthentication("appId", "appSecret")]
    public class BotController : ApiController
    {
        private enum DialogStates
        {
            Start,
            Menu,
            Question,
        }

        private const string Ruler = "---";

        private readonly string delimiter;

        private readonly IDecksService decksService;
        private readonly ICardsService cardsService;
        private readonly ISchedulerService schedulerService;

        private readonly IdentityContext context;
        private readonly IDialog dialog;

        public BotController(IDecksService decksService, ICardsService cardsService, ISchedulerService schedulerService, IdentityContext context, IDialog dialog)
        {
            this.decksService = decksService;
            this.cardsService = cardsService;
            this.schedulerService = schedulerService;

            this.context = context;
            this.dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            delimiter = Environment.NewLine + Environment.NewLine;
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
                        try
                        {
                            var state = message.Text == "/back" ? DialogStates.Start : message.GetBotPerUserInConversationData<DialogStates>("dialogState");

                            var userName = user.UserName;

                            switch (state)
                            {
                                case DialogStates.Start:
                                    var decks = await decksService.GetDecksAsync(userName);
                                    var data = decks.Select((d, i) => Tuple.Create(i + 1, d.ID, d.Title)).ToArray();
                                    var menuItems = data.Select(d => $"{d.Item1}) {d.Item3}");

                                    var pleaseChooseDeckMessage = "Please enter the deck number:";
                                    var decksList = string.Join(delimiter, menuItems);
                                    var menuText = string.Join(delimiter, pleaseChooseDeckMessage, decksList);

                                    message.SetBotPerUserInConversationData("response", menuText);
                                    message.SetBotPerUserInConversationData("decks", data);
                                    message.SetBotPerUserInConversationData("dialogState", DialogStates.Menu);

                                    break;
                                case DialogStates.Menu:
                                    int deckNumber;
                                    var isInt = int.TryParse(message.Text, out deckNumber);

                                    if (isInt)
                                    {
                                        var cachedDecks = message.GetBotPerUserInConversationData<Tuple<int, int, string>[]>("decks");

                                        var deckID = cachedDecks.SingleOrDefault(x => x.Item1 == deckNumber);

                                        if (deckID != null)
                                        {
                                            message.SetBotPerUserInConversationData("deckID", deckID.Item2);

                                            var backMessage = "Type /back to return to menu.";
                                            var firstQuestionMessage = "**First question:**";

                                            var question = await GetNextQuestion(message, userName);

                                            var pleseFillMessage = "Please fill in the blank.";

                                            var firstQuestionText = string.Join(delimiter, backMessage, Ruler, firstQuestionMessage, question, pleseFillMessage);


                                            message.SetBotPerUserInConversationData("dialogState", DialogStates.Question);
                                            message.SetBotPerUserInConversationData("response", firstQuestionText);
                                        }
                                        else
                                        {
                                            message.SetBotPerUserInConversationData("response", $"Deck number {deckNumber} is not found.");
                                        }
                                    }
                                    else
                                    {
                                        message.SetBotPerUserInConversationData("response", "Please enter the deck number.");
                                    }

                                    break;
                                case DialogStates.Question:
                                    Guid id = GetDeckId(message);
                                    var deck = await decksService.FindDeckAsync(id);
                                    var card = await cardsService.GetNextCardAsync(id, userName);

                                    var evaluatedCard = await cardsService.EvaluateCardAsync(card.ID, message.Text, userName);

                                    switch (evaluatedCard.Mark)
                                    {
                                        case Mark.Correct:
                                            var rightResponse = await GetResponseForRightAnswer(message, evaluatedCard, userName);
                                            message.SetBotPerUserInConversationData("response", rightResponse);
                                            break;
                                        case Mark.Incorrect:
                                            var wrongResponse = await GetResponseForWrongAnswer(message, evaluatedCard, userName);
                                            message.SetBotPerUserInConversationData("response", wrongResponse);
                                            break;
                                        case Mark.Typo:
                                            message.SetBotPerUserInConversationData("response", "Please try again.");
                                            break;
                                    }
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            message.SetBotPerUserInConversationData("dialogState", DialogStates.Start);
                        }

                        return await Conversation.SendAsync(message, () => dialog);
                    }
                    else
                    {
                        return message.CreateReplyMessage($"Your Skype account is '{message.From.Address}'. You have no Memento account. Please register at website first.");
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

        private async Task<string> GetResponseForRightAnswer(Message message, AnswerCardViewModel card, string userName)
        {
            var rightMessage = "**You are right!**";
            var commentMessage = "Comment:";
            var fullAnswerMessage = "Full answer:";
            var answer = string.IsNullOrWhiteSpace(card.Comment) ?
                card.FullAnswer :
                string.Join(delimiter, card.FullAnswer, commentMessage, card.Comment);

            var correctAnswerMessage = "Correct answer: " + card.ShortAnswer;
            var userAnswerMessage = "Your answer: " + card.UserAnswer;

            await schedulerService.PromoteClozeAsync(card.ID, Delays.Next, userName);

            var nextQuestionMessage = "**Next question:**";
            var nextQuestion = await GetNextQuestion(message, userName);

            var response = string.Join(delimiter, rightMessage, Ruler, fullAnswerMessage, answer, Ruler, correctAnswerMessage, userAnswerMessage, Ruler, nextQuestionMessage, nextQuestion);

            return response;
        }

        private async Task<string> GetResponseForWrongAnswer(Message message, AnswerCardViewModel card, string userName)
        {
            var wrongMessage = "**You are wrong!**";
            var fullAnswerMessage = "Full answer:";
            var commentMessage = "Comment:";

            var answer = string.IsNullOrWhiteSpace(card.Comment) ?
                card.FullAnswer :
                string.Join(delimiter, card.FullAnswer, commentMessage, card.Comment);

            var correctAnswerMessage = "Correct answer: " + card.ShortAnswer;
            var userAnswerMessage = "Your answer: " + card.UserAnswer;

            await schedulerService.PromoteClozeAsync(card.ID, Delays.Previous, userName);

            var nextQuestionMessage = "**Next question:**";
            var nextQuestion = await GetNextQuestion(message, userName);

            var response = string.Join(delimiter, wrongMessage, Ruler, fullAnswerMessage, answer, Ruler, correctAnswerMessage, userAnswerMessage, Ruler, nextQuestionMessage, nextQuestion);

            return response;
        }

        private async Task<string> GetNextQuestion(Message message, string userName)
        {
            Guid deckID = GetDeckId(message);
            var card = await cardsService.GetNextCardAsync(deckID, userName);

            var cardWithQuestion = await cardsService.GetCardWithQuestionAsync(card.ID, userName);

            return cardWithQuestion.Question;
        }

        private async Task<AnswerCardViewModel> GetAnswer(Message message, string userName)
        {
            Guid deckID = GetDeckId(message);
            var card = await cardsService.GetNextCardAsync(deckID, userName);

            var cardWithAnswer = await cardsService.GetCardWithAnswerAsync(card.ID, userName);

            return cardWithAnswer;
        }

        private static Guid GetDeckId(Message message) =>
            message.GetBotPerUserInConversationData<Guid>("deckID");

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
