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
        public MainDialog(FoodDialog foodDialog, PaymentDialog paymentDialog) : base(nameof(MainDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(foodDialog);
            AddDialog(paymentDialog);

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
            if (isConversationFood())
            {
                return await stepContext.BeginDialogAsync(nameof(FoodDialog), new FoodInfo { FoodType = stepContext.Result.ToString() }, cancellationToken);
            }
            else if (isConversationPayment())
            {
                return await stepContext.BeginDialogAsync(nameof(PaymentDialog), new PaymentInfo { Operation = stepContext.Result.ToString() }, cancellationToken);
            }
            else
            {
                var didntUnderstandMessageText = "ระบบไม่เข้าใจ ลองใช้คำอื่นแทนดูไหม? ❤️";
                var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
            }

            bool isConversationFood()
            {
                var keyword = new[] { "หิว", "กิน", "ทาน", "ข้าว", "อาหาร" };
                return keyword.Any(it => stepContext.Result.ToString().Contains(it));
            }
            bool isConversationPayment()
            {
                var keyword = new[] { "โอน", "ถอน", "จ่าย", "เงิน" };
                return keyword.Any(it => stepContext.Result.ToString().Contains(it));
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is not null)
            {
                var postUrl = stepContext.Result is FoodInfo ? "delivery/foods" : "payment?tx=1234567890";
                var url = $"https://mlink.com/{postUrl}";

                var preText = stepContext.Result is FoodInfo ? "เพลิดเพลินกับอาหาร" : "ดำเนินธุรกรรม";
                var messageText = $"คุณสามารถ{preText}ด้วย Url นี้{Environment.NewLine}{url}";

                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // TODO: Not any changed
            var promptMessage = "มีอะไรให้ช่วยเหลืออีกไหม?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
