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

namespace DemoEchoBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public MainDialog(FoodDialog foodDialog, PaymentDialog paymentDialog, OpenRestaurantDialog openRestaurantDialog, CloseRestaurantDialog closeRestaurantDialog, OrderRestaurantDialog orderRestaurantDialog) : base(nameof(MainDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(foodDialog);
            AddDialog(paymentDialog);
            AddDialog(openRestaurantDialog);
            AddDialog(closeRestaurantDialog);
            AddDialog(orderRestaurantDialog);
            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var messageText = "สวัสดี, ต้องการอะไรหรือเปล่าヽ(✿ﾟ▽ﾟ)ノ";
            var messageText = stepContext.Context.Activity.Text;
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            //await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
            return await stepContext.ContinueDialogAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (isConversationRestaurant() != null)
            {
                var data = isConversationRestaurant();
                switch (data)
                {
                    case "เปิดร้าน":
                        return await stepContext.BeginDialogAsync(nameof(OpenRestaurantDialog), new PaymentInfo { Operation = stepContext.Result.ToString() }, cancellationToken);
                    //return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);

                    case "ปิดร้าน":
                        return await stepContext.BeginDialogAsync(nameof(CloseRestaurantDialog), new PaymentInfo { Operation = stepContext.Result.ToString() }, cancellationToken);
                    case "ออเดอร์":
                        return await stepContext.BeginDialogAsync(nameof(OrderRestaurantDialog), new PaymentInfo { Operation = stepContext.Result.ToString() }, cancellationToken);
                    case "อัพเดท":
                        var attachments = new List<Attachment>();
                        var heroCard = new HeroCard
                        {
                            Title = "มีการอัพเดทเมนู",
                            Images = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/update.png") },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "ดูรายละเอียด", value: "http:www.google.com") }
                        };

                        attachments.Add(heroCard.ToAttachment());
                        var reply = MessageFactory.Attachment(attachments);
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
                    default:
                        break;
                }
            }
            else
            {
                var didntUnderstandMessageText = "ระบบไม่เข้าใจ ลองใช้คำอื่นแทนดูไหม? ❤️";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            string isConversationRestaurant()
            {
                var keyword = new[] { "ปิดร้าน", "เปิดร้าน", "ออเดอร์", "ประวัติ", "ตั้งค่า", "delevery", "อัพเดท" };
                var data = keyword.FirstOrDefault(it => it == stepContext.Result.ToString());
                return data;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if (stepContext.Result is not null)
            //{
            //    var postUrl = stepContext.Result is FoodInfo ? "delivery/foods" : "payment?tx=1234567890";
            //    var url = $"https://mlink.com/{postUrl}";

            //    var preText = stepContext.Result is FoodInfo ? "เพลิดเพลินกับอาหาร" : "ดำเนินธุรกรรม";
            //    var messageText = $"คุณสามารถ{preText}ด้วย Url นี้{Environment.NewLine}{url}";

            //    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            //    await stepContext.Context.SendActivityAsync(message, cancellationToken);
            //}

            // TODO: Not any changed
            var promptMessage = "มีอะไรให้ช่วยเหลืออีกไหม?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, "", cancellationToken);
        }
    }
}
