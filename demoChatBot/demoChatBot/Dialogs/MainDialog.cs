using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Linq;
using demoChatBot.Models;
using Newtonsoft.Json.Linq;
using DemoEchoBot.Services;

namespace DemoEchoBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private string APIBaseUrl = "https://delivery-3rd-test-api.azurewebsites.net";
        private RestaurantShortResponse _restaurantDetails;

        private readonly IRestClientService _restClientService;
        private readonly IBotStateService _botStateService;

        public MainDialog(LinkAccountDialog linkAccountDialog, FoodDialog foodDialog, PaymentDialog paymentDialog, OpenRestaurantDialog openRestaurantDialog, CloseRestaurantDialog closeRestaurantDialog, OrderRestaurantDialog orderRestaurantDialog,
            IBotStateService botStateService, IRestClientService restClientService) : base(nameof(MainDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(foodDialog);
            AddDialog(paymentDialog);
            AddDialog(openRestaurantDialog);
            AddDialog(closeRestaurantDialog);
            AddDialog(orderRestaurantDialog);
            AddDialog(linkAccountDialog);

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
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);

            if (restaurantDetails.IsLinkedAccount)
            {
                if (_restaurantDetails is null)
                {
                    var resturnonAPI = $"{APIBaseUrl}/api/Restaurant/GetRestaurantInfo/{restaurantDetails.BaId}";
                    _restaurantDetails = await _restClientService.Get<RestaurantShortResponse>(resturnonAPI);
                }
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                await TryGetUserDetail();
                return await stepContext.NextAsync(null, cancellationToken);


            }
            async Task TryGetUserDetail()
            {
                var userId = stepContext.Context.Activity.From.Id;
                var resturnonAPI = $"{APIBaseUrl}/api/Restaurant/GetRestaurantInfoWithChatBotId/{userId}";
                var rspdata = await _restClientService.Get<RestaurantShortResponse>(resturnonAPI);
                if (rspdata is null) return;
                var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);
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
            var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(stepContext.Context, () => new RestaurantDetails(), cancellationToken);

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            var data = stepContext.Context.Activity.Text;
            if (restaurantDetails.IsLinkedAccount)
            {
                switch (data)
                {
                    case "เปิดร้าน":
                        return await stepContext.BeginDialogAsync(nameof(OpenRestaurantDialog), new PaymentInfo { Operation = data }, cancellationToken);
                    case "ปิดร้าน":
                        return await stepContext.BeginDialogAsync(nameof(CloseRestaurantDialog), new PaymentInfo { Operation = data }, cancellationToken);
                    case null:
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    //case "ออเดอร์":
                    //    return await stepContext.BeginDialogAsync(nameof(OrderRestaurantDialog), new PaymentInfo { Operation = data }, cancellationToken);
                    //case "อัพเดท":
                    //    var heroCard = new HeroCard
                    //    {
                    //        Title = "มีการอัพเดทเมนู",
                    //        Images = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/update.png") },
                    //        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "ดูรายละเอียด", value: "http:www.google.com") }
                    //    };
                    //    attachments.Add(heroCard.ToAttachment());
                    //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
                    //case "เมนูร้าน":
                    //    var mainMenuApi = $"https://devster-delivery-test.onmana.space/apprestaurant/index.html#/menu-main";
                    //    var mainMenuheroCard = new HeroCard
                    //    {
                    //        Title = "เมนูร้าน",
                    //        Text = $"ดูผ่านจากลิงค์นี้ {mainMenuApi}",
                    //        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิดลิงค์", value: mainMenuApi) }
                    //    };
                    //    reply.Attachments.Add(mainMenuheroCard.ToAttachment());
                    //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
                    //case "เมนูอัพเดท":
                    //    var menuUpdateApi = $"https://devster-delivery-test.onmana.space/apprestaurant/index.html#/menuupdate-main";
                    //    var menuupdateheroCard = new HeroCard
                    //    {
                    //        Title = "เมนูอัพเดท",
                    //        Text = $"ดูผ่านจากลิงค์นี้ {menuUpdateApi}",
                    //        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิดลิงค์", value: menuUpdateApi) }
                    //    };
                    //    reply.Attachments.Add(menuupdateheroCard.ToAttachment());
                    //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
                    //case "ออเดอร์วันนี้":
                    //    var orderNowApi = $"https://devster-delivery-test.onmana.space/apprestaurant/index.html#/order-main";
                    //    var orderNowheroCard = new HeroCard
                    //    {
                    //        Title = "ออเดอร์วันนี้",
                    //        Text = $"ดูผ่านจากลิงค์นี้ {orderNowApi}",
                    //        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิดลิงค์", value: orderNowApi) }
                    //    };
                    //    reply.Attachments.Add(orderNowheroCard.ToAttachment());
                    //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
                    //case "ออเดอร์ย้อนหลัง":
                    //    var orderHistoryApi = $"https://devster-delivery-test.onmana.space/apprestaurant/index.html#/history-main";
                    //    var orderHistoryheroCard = new HeroCard
                    //    {
                    //        Title = "ประวัติออเดอร์ย้อนหลัง",
                    //        Text = $"ดูผ่านจากลิงค์นี้ {orderHistoryApi}",
                    //        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิดลิงค์", value: orderHistoryApi) }
                    //    };
                    //    reply.Attachments.Add(orderHistoryheroCard.ToAttachment());
                    //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
                    //case "จัดการร้าน":
                    //    var settingApi = $"https://devster-delivery-test.onmana.space/apprestaurant/index.html#/setting-main";
                    //    var settingheroCard = new HeroCard
                    //    {
                    //        Title = "จัดการร้าน",
                    //        Text = $"ดูผ่านจากลิงค์นี้ {settingApi}",
                    //        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิดลิงค์", value: settingApi) }
                    //    };
                    //    reply.Attachments.Add(settingheroCard.ToAttachment());
                    //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
                    //case "บริษัทดิลิเวอรี่":
                    //    var contractApi = $"https://devster-delivery-test.onmana.space/apprestaurant/index.html#/contract-main";
                    //    var contractheroCard = new HeroCard
                    //    {
                    //        Title = "บริษัทดิลิเวอรี่",
                    //        Text = $"ดูผ่านจากลิงค์นี้ {contractApi}",
                    //        Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิดลิงค์", value: contractApi) }
                    //    };
                    //    reply.Attachments.Add(contractheroCard.ToAttachment());
                    //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
                    default:
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("ระบบไม่เข้าใจ ลองใช้คำอื่นแทนดูไหม? ❤️\""));
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                }
            }
            else
            {
                var linkAccountDetails = new RestaurantDetails();
                return await stepContext.BeginDialogAsync(nameof(LinkAccountDialog), linkAccountDetails, cancellationToken);
            }
            //return await stepContext.ContinueDialogAsync(cancellationToken);

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "", cancellationToken);
        }
    }
}
