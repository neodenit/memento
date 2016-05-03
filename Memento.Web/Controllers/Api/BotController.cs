using Memento.Bot;
using Memento.Interfaces;
using Memento.Models;
using Memento.Models.ViewModels;
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
        private enum DialogStates
        {
            Start,
            Menu,
            Question,
        }

        private const string Ruler = "---";

        private readonly string delimeter;

        private readonly IDecksService decksService;
        private readonly ICardsService cardsService;
        private readonly ISchedulerService schedulerService;

        private readonly ApplicationDbContext context;

        public BotController(IDecksService decksService, ICardsService cardsService, ISchedulerService schedulerService)
        {
            this.decksService = decksService;
            this.cardsService = cardsService;
            this.schedulerService = schedulerService;

            context = ApplicationDbContext.Create();

            delimeter = Environment.NewLine + Environment.NewLine;
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

                            switch (state)
                            {
                                case DialogStates.Start:
                                    var userName = user.UserName;
                                    var decks = await decksService.GetDecksAsync(userName);
                                    var data = decks.Select((d, i) => Tuple.Create(i + 1, d.ID, d.Title)).ToArray();
                                    var menuItems = data.Select(d => $"{d.Item1}) {d.Item3}");

                                    var pleaseChooseDeckMessage = "Please enter the deck number:";
                                    var decksList = string.Join(delimeter, menuItems);
                                    var menuText = string.Join(delimeter, pleaseChooseDeckMessage, decksList);

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

                                            var question = await GetNextQuestion(message);

                                            var pleseFillMessage = "Please fill in the blank.";

                                            var firstQuestionText = string.Join(delimeter, backMessage, Ruler, firstQuestionMessage, question, pleseFillMessage);


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
                                    var deck = await GetCurrentDeck(message);
                                    var card = deck.GetNextCard();

                                    var answerCard = new AnswerCardViewModel(card) { UserAnswer = message.Text };

                                    var evaluatedCard = await cardsService.EvaluateCard(answerCard);

                                    switch (evaluatedCard.Mark)
                                    {
                                        case Mark.Correct:
                                            var rightResponse = await GetResponseForRightAnswer(message, deck, evaluatedCard);
                                            message.SetBotPerUserInConversationData("response", rightResponse);
                                            break;
                                        case Mark.Incorrect:
                                            var wrongResponse = await GetResponseForWrongAnswer(message, deck, evaluatedCard);
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

                        return await Conversation.SendAsync(message, () => new Dialog());
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

        private async Task<string> GetResponseForRightAnswer(Message message, IDeck deck, IAnswerCardViewModel card)
        {
            var rightMessage = "**You are right!**";
            var fullAnswerMessage = "Full answer:";
            var answer = string.IsNullOrWhiteSpace(card.Comment) ?
                card.FullAnswer :
                string.Join(delimeter, card.FullAnswer, card.Comment);

            var correctAnswerMessage = "Correct answer: " + card.ShortAnswer;
            var userAnswerMessage = "Your answer: " + card.UserAnswer;

            await schedulerService.PromoteCloze(deck, Delays.Next);

            var nextQuestionMessage = "**Next question:**";
            var nextQuestion = await GetNextQuestion(message);

            var response = string.Join(delimeter, rightMessage, Ruler, fullAnswerMessage, answer, Ruler, correctAnswerMessage, userAnswerMessage, Ruler, nextQuestionMessage, nextQuestion);

            return response;
        }

        private async Task<string> GetResponseForWrongAnswer(Message message, IDeck deck, IAnswerCardViewModel card)
        {
            var wrongMessage = "**You are wrong!**";
            var fullAnswerMessage = "Full answer:";
            var answer = string.IsNullOrWhiteSpace(card.Comment) ?
                card.FullAnswer :
                string.Join(delimeter, card.FullAnswer, card.Comment);

            var correctAnswerMessage = "Correct answer: " + card.ShortAnswer;
            var userAnswerMessage = "Your answer: " + card.UserAnswer;

            await schedulerService.PromoteCloze(deck, Delays.Previous);

            var nextQuestionMessage = "**Next question:**";
            var nextQuestion = await GetNextQuestion(message);

            var response = string.Join(delimeter, wrongMessage, Ruler, fullAnswerMessage, answer, Ruler, correctAnswerMessage, userAnswerMessage, Ruler, nextQuestionMessage, nextQuestion);

            return response;
        }

        private async Task<ICard> GetNextCard(Message message)
        {
            var dbDeck = await GetCurrentDeck(message);
            var card = dbDeck.GetNextCard();

            return card;
        }

        private async Task<string> GetNextQuestion(Message message)
        {
            var card = await GetNextCard(message);
            var cloze = card.GetNextCloze();
            var cardWithQuestion = await cardsService.GetCardWithQuestion(card.ID);

            return cardWithQuestion.Question;
        }

        private async Task<IAnswerCardViewModel> GetAnswer(Message message)
        {
            var card = await GetNextCard(message);
            var cloze = card.GetNextCloze();
            var cardWithAnswer = await cardsService.GetCardWithAnswer(card.ID);

            return cardWithAnswer;
        }

        private async Task<IDeck> GetCurrentDeck(Message message)
        {
            var deckID = message.GetBotPerUserInConversationData<int>("deckID");
            var dbDeck = await decksService.FindDeckAsync(deckID);

            return dbDeck;
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
