using demoChatBot.Models;
using DemoEchoBot.Services;
using Flurl.Util;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoEchoBot.Dialogs
{
    public class OpenRestaurantDialog : ComponentDialog
    {
        private readonly string APIBaseUrl = "https://delivery-3rd-test-api.azurewebsites.net";
        private readonly IBotStateService _botStateService;
        private readonly IRestClientService _restClientService;
        private RestaurantShortResponse _restaurantDetail;


        public OpenRestaurantDialog(IBotStateService botStateService, IRestClientService restClientService) : base(nameof(OpenRestaurantDialog))
        {

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            var waterfallSteps = new WaterfallStep[]
            {
                OpenRestaurant,
                CheckstatusRestaurant,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
            _restClientService = restClientService;
            _botStateService = botStateService;
        }

        private async Task<DialogTurnResult> OpenRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);
            var resturnonAPI = $"{APIBaseUrl}/api/Restaurant/GetRestaurantInfo/{restaurantDetails.BaId}";
            var respon = await _restClientService.Get<RestaurantShortResponse>(resturnonAPI);
            restaurantDetails.StatusRestaurant = respon.IsStandby;
            await _botStateService.SaveChangesAsync(stepContext.Context);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("คุณต้องการเปิดร้านใช่หรือไม่"),
                Choices = new[]
                   {
                        new Choice { Value = "ยืนยัน" },
                        new Choice { Value = "ยกเลิก" }
                    }
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckstatusRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);
            var data = stepContext.Context.Activity.Text;
            var message = "";

            if (restaurantDetails.StatusRestaurant)
            {
                message = "ร้านเปิดร้านอยู่แล้ว";
                var promptMessage = MessageFactory.Text(message, message, InputHints.IgnoringInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage });
            }
            else
            {
                switch (data)
                {
                    case "ยืนยัน":
                        message = "เปิดร้านเรียบร้อยแล้ว";
                        var confirmMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
                        var resturnonAPI = $"{APIBaseUrl}/api/Restaurant/RestaurantStandbyTurnOn/{restaurantDetails.RestaurantId}";
                        await _restClientService.Post(resturnonAPI, string.Empty);
                        restaurantDetails.StatusRestaurant = true;
                        await _botStateService.SaveChangesAsync(stepContext.Context);
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = confirmMessage }, cancellationToken);
                    case "ยกเลิก":
                        message = "ยกเลิกการเปิดร้าน";
                        var cancleMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = cancleMessage }, cancellationToken);
                    default:
                        break;
                }
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

}
