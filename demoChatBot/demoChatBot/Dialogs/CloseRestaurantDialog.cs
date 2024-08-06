using demoChatBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoEchoBot.Dialogs
{
    public class CloseRestaurantDialog : ComponentDialog
    {
        public CloseRestaurantDialog() : base(nameof(CloseRestaurantDialog))
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
        }

        private async Task<DialogTurnResult> CloseRestaurant(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
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
            var data = stepContext.Result.ToString();
            var message = "";
            switch (data)
            {
                case "ยืนยันการปิดร้าน":
                    message = "ยืนยันการปิดร้าน";
                    var confirmMessage = MessageFactory.Text(message, message, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = confirmMessage }, cancellationToken);
                case "ยกเลิกการปิดร้าน":
                    message = "ยกเลิกการปิดร้าน";
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
