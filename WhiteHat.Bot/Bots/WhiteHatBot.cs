// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.16.0

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhiteHat.Bot.Bots.Conversation;
using WhiteHat.Bot.CogServices;
using static WhiteHat.Bot.CogServices.ConversationRecongnizer;

namespace WhiteHat.Bot.Bots
{
    public class WhiteHatBot : ActivityHandler, ICallingBot
    {
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly ILogger<WhiteHatBot> _logger;
        private readonly LuisHelper _luisHelper;
        private readonly MicrosoftCognitiveSpeechService speechService = new();

        public ICallingBotService CallingBotService { get; }

        public WhiteHatBot(ConversationState conversationState, UserState userState, ILogger<WhiteHatBot> logger, LuisHelper luisHelper, ICallingBotService callingBotService)
        {
            _luisHelper = luisHelper;
            _conversationState = conversationState;
            _userState = userState;
            _logger = logger;

            this.CallingBotService = callingBotService;

            this.CallingBotService.OnIncomingCallReceived += this.OnIncomingCallReceived;
            this.CallingBotService.OnRecordCompleted += this.OnRecordCompleted;
            this.CallingBotService.OnHangupCompleted += OnHangupCompleted;
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
            if (result.Text == null) return String.Empty;
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

        public void Dispose()
        {
            if (this.CallingBotService != null)
            {
                this.CallingBotService.OnIncomingCallReceived -= this.OnIncomingCallReceived;
                this.CallingBotService.OnRecordCompleted -= this.OnRecordCompleted;
                this.CallingBotService.OnHangupCompleted -= OnHangupCompleted;
            }
        }

        private static Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Male };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var record = new Record
            {
                OperationId = Guid.NewGuid().ToString(),
                PlayPrompt = new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Please leave a message" } } },
                RecordingFormat = RecordingFormat.Wav
            };

            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase> {
                new Answer { OperationId = Guid.NewGuid().ToString() },
                record
            };

            return Task.FromResult(true);
        }

        private async Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            List<ActionBase> actions = new List<ActionBase>();

            var spokenText = string.Empty;
            if (recordOutcomeEvent.RecordOutcome.Outcome == Outcome.Success)
            {
                var record = await recordOutcomeEvent.RecordedContent;
                spokenText = await this.speechService.GetTextFromAudioAsync(record);
                actions.Add(new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Thanks for leaving the message." }, new Prompt { Value = "You said... " + spokenText } } });
            }
            else
            {
                actions.Add(new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { new Prompt { Value = "Sorry, there was an issue. " } } });
            }

            //actions.Add(new Hangup { OperationId = Guid.NewGuid().ToString() }); // hang up the call

            recordOutcomeEvent.ResultingWorkflow.Actions = actions;
            recordOutcomeEvent.ResultingWorkflow.Links = null;
        }

    }
}