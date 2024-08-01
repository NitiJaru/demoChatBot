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
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
                CheckstatusRestaurant
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetOrderRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = (PaymentInfo)stepContext.Options;
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);

            var heroCard = new HeroCard
            { };
            //var heroCard = new HeroCard
            //{
            //    Title = "ออเดอร์ ",
            //    Text = "ราคา 10 ฿",
            //    Images = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/closeResturent_Rich_Message.png") },
            //    Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "ดูรายละเอียด", value: "ดูรายละเอียด"), new CardAction(ActionTypes.ImBack, "อาหารเสร็จแล้ว", value: "อาหารเสร็จแล้ว") }
            //};
            var img = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/ka.jpg") };

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            var rnd = new Random();
            for (int i = 1; i <= 10; i++)
            {
                heroCard.Title = $"ออเดอร์ {i}";
                heroCard.Text = $"{Environment.NewLine}ราคา {rnd.Next(30, 99)}฿";
                heroCard.Images = img;
                heroCard.Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "ดูรายละเอียด", value: "http://www.google.com"), new CardAction(ActionTypes.ImBack, "อาหารเสร็จแล้ว", value: "อาหารเสร็จแล้ว") };
                reply.Attachments.Add(heroCard.ToAttachment());
                heroCard = new HeroCard { };
            }
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)reply });

        }

        private async Task<DialogTurnResult> CheckstatusRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = stepContext.Result.ToString();
            var message = "";
            message = data == "ดูรายละเอียด" ? "ดูรายละเอียด" : "ออเดอร์ที่กดอาหารเสร็จแล้วหายไป";

            var promptMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

        }
    }

}
