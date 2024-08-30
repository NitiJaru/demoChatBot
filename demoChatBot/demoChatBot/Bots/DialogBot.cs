// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.18.1

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DemoEchoBot.Bots
{
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        private readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;

        public DialogBot(ConversationState conversationState
            , ConcurrentDictionary<string, ConversationReference> conversationReferences
            , UserState userState
            , T dialog)
        {
            ConversationState = conversationState;
            ConversationReferences = conversationReferences;
            Dialog = dialog;
            UserState = userState;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var lineUserId = member.Id;
                    //var response = MessageFactory.Text($"Welcome lineUser ID: {lineUserId}");
                    //await turnContext.SendActivityAsync(response, cancellationToken);
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            ConversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        public async Task MessageReceivedAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Check if the incoming activity is of type Invoke
            if (turnContext.Activity.Type == ActivityTypes.Invoke)
            {
                // Check the name of the invoke action
                if (turnContext.Activity.Name == "customAction")
                {
                    // Perform the custom action
                    await turnContext.SendActivityAsync("Performing custom action...", cancellationToken: cancellationToken);
                }
            }
        }
    }
}
