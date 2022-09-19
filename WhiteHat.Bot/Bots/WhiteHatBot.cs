// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.16.0

using EchoBotTest.Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhiteHat.Bot.Bots.Conversation;
using WhiteHat.Bot.Luis;
using static WhiteHat.Bot.Luis.ConversationRecongnizer;

namespace WhiteHat.Bot.Bots
{
    public class WhiteHatBot : ActivityHandler
    {
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly ILogger<WhiteHatBot> _logger;
        private readonly LuisHelper _luisHelper;

        public WhiteHatBot(ConversationState conversationState, UserState userState, ILogger<WhiteHatBot> logger, LuisHelper luisHelper)
        {
            _luisHelper = luisHelper;
            _conversationState = conversationState;
            _userState = userState;
            _logger = logger;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<Message>(nameof(Message));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new Message());

            if (conversationData.SessionId == null)
            {
                conversationData.SessionId = Guid.NewGuid().ToString();
                _logger.LogInformation($"Create and set session id = {conversationData.SessionId}");
            }

            var conversationSender = new ConversationSender(conversationData.SessionId);
            var result = await _luisHelper.RecognizeAsync<ConversationRecongnizer>(turnContext, cancellationToken);
            var replyText = BotAction(result, conversationSender);
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        private string BotAction(ConversationRecongnizer result, ConversationSender conversationSender)
        {
            var (intent, score) = result.TopIntent();
            var replyText = string.Empty;
            try
            {
                var note = result.Text.Replace("WHITEHATBOT", "").Replace("WHITEHAT", "").Replace("WHITEHATECHOBOT","").Replace(nameof(intent), "");
                switch (intent)
                {
                    case Intent.Note_Create:
                    case Intent.Note_AddToNote:
                        conversationSender.AddNote(note);
                        replyText = "Ok";
                        break;
                    case Intent.Note_Clear:
                    case Intent.Note_Delete:
                        conversationSender.DeleteNode(1);
                        replyText = "Deleted";
                        break;
                    case Intent.Note_ChangeTitle:
                        var val = conversationSender.GetMinutesOfMeeting();
                        replyText = "Title added";
                        break;
                    case Intent.Note_ReadAloud:
                        var mom = conversationSender.GetMinutesOfMeeting();
                        replyText = $"Notes added so far {Environment.NewLine}:{string.Join(Environment.NewLine, mom)}";
                        break;
                    case Intent.Email_SendEmail:
                        conversationSender.AddParticipants(note);
                        conversationSender.SendMail();
                        replyText = $"Email sent to {note}";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, null);
                replyText = ex.Message;
            }

            return replyText;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome, I'm listening!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == "conversationUpdate")
            {
                var conversationStateAccessors = _conversationState.CreateProperty<Message>(nameof(Message));
                var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new Message());

                if (conversationData.SessionId == null)
                {
                    conversationData.SessionId = Guid.NewGuid().ToString();
                }
            }
            await base.OnTurnAsync(turnContext, cancellationToken);

            //// Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);    
        }
    }
}