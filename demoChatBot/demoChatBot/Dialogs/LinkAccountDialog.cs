using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using demoChatBot.Models;
using DemoEchoBot.Services;

namespace DemoEchoBot.Dialogs
{
    public class LinkAccountDialog : ComponentDialog
    {
        private readonly string APIBaseUrl = "https://delivery-3rd-test-api.azurewebsites.net";
        private readonly IRestClientService _restClientService;
        private readonly IBotStateService _botStateService;

        public LinkAccountDialog(IBotStateService botStateService, IRestClientService restClientService)
            : base(nameof(LinkAccountDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            var waterfallSteps = new WaterfallStep[] {
                LinkingStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
            _restClientService = restClientService;
            _botStateService = botStateService;


        }

        private async Task<DialogTurnResult> LinkingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var userId = stepContext.Context.Activity.From.Id;
            var userName = stepContext.Context.Activity.From.Name;
            var sessionRequest = $"{APIBaseUrl}/api/Restaurant/LinkRequest/line/{userId}/{userName}";
            var session = await _restClientService.Get<Session>(sessionRequest);

            var reply = MessageFactory.Attachment(new Attachment
            {
                ContentType = "image/png",
                ContentUrl = session.Url,
            });

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            var card = new HeroCard
            {
                Title = "กรุณาแสกน qr ผูกบัญชีกับมานะ เพื่อเข้าใช้งานระบบ",
                Buttons = new List<CardAction> {
                    new(ActionTypes.OpenUrl, title: "เปิดแอพ มานะ", value: "https://www.google.com/")
                }
            };

            var promptOptions = new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(card.ToAttachment()),
                Style = ListStyle.HeroCard,
            };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}
