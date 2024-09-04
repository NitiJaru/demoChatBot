using demoChatBot.Models;
using DemoEchoBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DemoEchoBot.Dialogs
{
    public class CloseRestaurantDialog : ComponentDialog
    {
        private readonly string APIBaseUrl = "https://delivery-3rd-test-api.azurewebsites.net";
        private RestaurantShortResponse _restaurantDetail;
        private readonly IBotStateService _botStateService;
        private readonly IRestClientService _restClientService;
        public CloseRestaurantDialog(IBotStateService botStateService, IRestClientService restClientService) : base(nameof(CloseRestaurantDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            var waterfallSteps = new WaterfallStep[]
            {
                CloseRestaurant,
                CheckstatusRestaurant,
                FinalStepAsync

            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
            _restClientService = restClientService;
            _botStateService = botStateService;
        }

        private async Task<DialogTurnResult> CloseRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity.From.Id;
            var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);
            var resturnonAPI = $"{APIBaseUrl}/api/Restaurant/GetRestaurantInfo/{restaurantDetails.BaId}";
            var respon = await _restClientService.Get<RestaurantShortResponse>(resturnonAPI, userId);
            restaurantDetails.StatusRestaurant = respon.IsStandby;
            await _botStateService.SaveChangesAsync(stepContext.Context);
            var data = (PaymentInfo)stepContext.Options;
            var attachments = new List<Attachment>();

            var heroCard = new HeroCard
            {
                Title = "ปิดร้าน",
                Text = "คุณต้องการปิดร้านใช่หรือไม่",
                Images = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/closeResturent_Rich_Message.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "ยืนยัน", value: "ยืนยันการปิดร้าน"), new CardAction(ActionTypes.ImBack, "ยกเลิก", value: "ยกเลิกการปิดร้าน") }
            };

            attachments.Add(heroCard.ToAttachment());
            var reply = MessageFactory.Attachment(attachments);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });

        }

        private async Task<DialogTurnResult> CheckstatusRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity.From.Id;
            var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);
            var data = stepContext.Result.ToString();
            var message = "";
            if (!restaurantDetails.StatusRestaurant)
            {
                var messageText = "คุณปิดร้านอยู่แล้ว";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else
            {
                switch (data)
                {
                    case "ยืนยันการปิดร้าน":
                        message = "ปิดร้านเรียบร้อยแล้ว";
                        var confirmMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
                        var resturnonAPI = $"{APIBaseUrl}/api/Restaurant/RestaurantStandbyTurnOff/{restaurantDetails.RestaurantId}/?permanently=true";
                        await _restClientService.Post(resturnonAPI, string.Empty, userId);
                        restaurantDetails.StatusRestaurant = false;
                        await _botStateService.SaveChangesAsync(stepContext.Context);
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = confirmMessage }, cancellationToken);
                    case "ยกเลิกการปิดร้าน":
                        message = "ยกเลิกการปิดร้าน";
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
