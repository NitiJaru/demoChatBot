using demoChatBot.Models;
using demoChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace demoChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly IRestClientService _restClientService;
        private readonly IBotStateService _botStateService;
        private readonly ConnectionSettings _connectionSettings;

        public MainDialog(LinkAccountDialog linkAccountDialog, FoodDialog foodDialog, PaymentDialog paymentDialog, OpenRestaurantDialog openRestaurantDialog, CloseRestaurantDialog closeRestaurantDialog, OrderRestaurantDialog orderRestaurantDialog,
            IBotStateService botStateService, IRestClientService restClientService, ConnectionSettings connectionSetting) : base(nameof(MainDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(foodDialog);
            AddDialog(paymentDialog);
            AddDialog(openRestaurantDialog);
            AddDialog(closeRestaurantDialog);
            AddDialog(orderRestaurantDialog);
            AddDialog(linkAccountDialog);
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
            _botStateService = botStateService;
            _restClientService = restClientService;
            _connectionSettings = connectionSetting;
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity.From.Id;
            var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);
            if (restaurantDetails.IsLinkedAccount)
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                await TryGetUserDetail();
                return await stepContext.NextAsync(null, cancellationToken);
            }
            async Task TryGetUserDetail()
            {
                var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);
                var resturnonAPI = $"{_connectionSettings.DeliveryAPIBaseUrl}/api/Restaurant/GetRestaurantInfoWithChatBotId";
                var rspdata = await _restClientService.Get<RestaurantShortResponse>(resturnonAPI, userId);
                if (rspdata is null) return;
                restaurantDetails.IsLinkedAccount = true;
                restaurantDetails.StatusRestaurant = rspdata.IsStandby;
                restaurantDetails.RestaurantId = rspdata._id;
                restaurantDetails.RestaurantName = rspdata.Name;
                restaurantDetails.BaId = rspdata.BusinessAccountId;
                await _botStateService.SaveChangesAsync(stepContext.Context);
            }
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var userId = stepContext.Context.Activity.From.Id;
            var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            var data = stepContext.Context.Activity.Text;
            if (data?.ToLower() == "reset")
            {
                restaurantDetails.IsLinkedAccount = false;
                restaurantDetails.RestaurantId = null;
                await _botStateService.SaveChangesAsync(stepContext.Context);
                var resetApi = $"{_connectionSettings.DeliveryAPIBaseUrl}/api/Restaurant/LinkedRemove/{userId}";
                await _restClientService.Post(resetApi, string.Empty, userId);
                stepContext.Context.Activity.Text = null;
                return await stepContext.ReplaceDialogAsync(InitialDialogId, "Reset BOT", cancellationToken);
            }
            else
            {
                if (restaurantDetails.IsLinkedAccount)
                {
                    switch (data?.ToLower())
                    {
                        case "เปิดร้าน":
                            return await stepContext.BeginDialogAsync(nameof(OpenRestaurantDialog), new PaymentInfo { Operation = data }, cancellationToken);
                        case "ปิดร้าน":
                            return await stepContext.BeginDialogAsync(nameof(CloseRestaurantDialog), new PaymentInfo { Operation = data }, cancellationToken);
                        case null:
                            return await stepContext.EndDialogAsync(null, cancellationToken);
                        case "ติดต่อ":
                            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Admin DeliveryName deilvery{Environment.NewLine}  PhoneNumber"));
                            return await stepContext.EndDialogAsync();
                        default:
                            if (restaurantDetails.StatusRestaurant)
                            {
                                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"สถานะร้าน เปิด"));
                                return await stepContext.EndDialogAsync(null, cancellationToken);
                            }
                            else
                            {

                                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"สถานะร้าน ปิด"));
                                return await stepContext.EndDialogAsync(null, cancellationToken);
                            }
                    }
                }
                else if (!restaurantDetails.IsLinkedAccount && (data?.ToLower() == "เริ่มผูกบัญชีใหม่" || data?.ToLower() == null))
                {
                    var linkAccountDetails = new RestaurantDetails();
                    return await stepContext.BeginDialogAsync(nameof(LinkAccountDialog), linkAccountDetails, cancellationToken);
                }
                else
                {
                    var messageText = "คุณยังไม่ได้ผูก line account กับ mana";
                    var promptMessage = MessageFactory.Text(messageText, messageText);
                    await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
                    var linkAccountDetails = new RestaurantDetails();
                    return await stepContext.BeginDialogAsync(nameof(LinkAccountDialog), linkAccountDetails, cancellationToken);
                }
            }

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "", cancellationToken);
        }
    }
}
