using demoChatBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Attachment = Microsoft.Bot.Schema.Attachment;

namespace DemoEchoBot.Dialogs
{
    public class OrderRestaurantDialog : ComponentDialog
    {
        public OrderRestaurantDialog() : base(nameof(OrderRestaurantDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            var waterfallSteps = new WaterfallStep[]
            {
                GetOrderRestaurant,
                CheckstatusRestaurant,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetOrderRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = (PaymentInfo)stepContext.Options;
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            var resOrderApi = $"https://devster-delivery-test.onmana.space/apprestaurant/index.html#/order-main";
            var heroCard = new HeroCard
            {
                Title = "ดูข้อมูลออเดอร์หรืออัพเดทสถานะออเดอร์",
                Text = $"ผ่านจากลิงค์นี้ {resOrderApi}",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิดลิงค์", value: resOrderApi) }
            };
            reply.Attachments.Add(heroCard.ToAttachment());
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });
        }

        private async Task<DialogTurnResult> CheckstatusRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Result.ToString();
            var message = "";
            switch (data)
            {
                case "ดูรายละเอียด":
                    message = "ดูรายละเอียด";
                    var confirmMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = confirmMessage }, cancellationToken);
                case "อาหารเสร็จแล้ว":
                    message = "ออเดอร์ที่กดอาหารเสร็จแล้วหายไป";
                    var cancleMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = cancleMessage }, cancellationToken);
                default:
                    break;
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

}
