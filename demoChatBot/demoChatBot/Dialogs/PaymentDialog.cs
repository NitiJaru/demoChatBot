using demoChatBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoEchoBot.Dialogs
{
    public class PaymentDialog : ComponentDialog
    {
        public PaymentDialog() : base(nameof(PaymentDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            var waterfallSteps = new WaterfallStep[]
            {
                AskOperation,
                AskMethod,
                AskAddress,
                AskAmount,
                ConfirmOperationAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskOperation(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = (PaymentInfo)stepContext.Options;

            if (data.Operation is "โอนเงิน" or "ถอนเงิน")
            {
                return await stepContext.NextAsync(new FoundChoice { Value = data.Operation }, cancellationToken);
            }

            var messageText = "คุณต้องการดำเนินธรุกรรมการเงินด้านใด?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = promptMessage,
                Choices = new[]
                {
                    new Choice { Value = "โอนเงิน"},
                    new Choice { Value = "ถอนเงิน"},
                    new Choice { Value = "จัดการบัญชี"},
                }
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskMethod(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = (PaymentInfo)stepContext.Options;
            var result = (FoundChoice)stepContext.Result;
            data.Operation = result.Value;

            if (data.Operation is "โอนเงิน" or "ถอนเงิน")
            {

                var messageText = $"ต้องการ{data.Operation}ด้วยช่องทางใด?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                var choices = new List<Choice>()
                    {
                        new Choice { Value = "คิวอาร์โค้ด"},
                        new Choice { Value = "พร้อมเพย์"},
                        new Choice { Value = "ธนาคาร"},
                    };

                var availableChoices = data.Operation is "โอนเงิน" ? choices :
                    data.Operation is "ถอนเงิน" ? choices.Skip(1).ToList() : new List<Choice>();
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = promptMessage,
                    Choices = availableChoices
                }, cancellationToken);
            }
            else
            {
                var didntCompleteFlowMsgText = $"ขออภัย {data.Operation}ยังไม่เปิดให้บริการ เมื่อเปิดให้บริการจะแจ้งให้ทราบในภายหลัง";
                var didntCompleteFlowMsg = MessageFactory.Text(didntCompleteFlowMsgText, didntCompleteFlowMsgText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(didntCompleteFlowMsg, cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AskAddress(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = (PaymentInfo)stepContext.Options;
            var result = (FoundChoice)stepContext.Result;
            data.Method = result.Value;

            if (data.Method is "คิวอาร์โค้ด")
            {
                return await stepContext.NextAsync(null, cancellationToken);

            }
            var preText = data.Method is "พร้อมเพย์" ? $"หมายเลขพ{data.Method}" : "หมายเลขบัญชี";

            var messageText = $"ต้องการ{data.Operation}ด้วย{data.Method} {preText}ใด?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

        }

        private async Task<DialogTurnResult> AskAmount(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = (PaymentInfo)stepContext.Options;
            data.Address = (string)stepContext.Result;

            var messageText = $"ต้องการ{data.Operation}ด้วย{data.Method} จำนวนกี่บาท?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmOperationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = (PaymentInfo)stepContext.Options;
            data.Amount = (string)stepContext.Result;

            var qrCodeText = data.Method is not "คิวอาร์โค้ด" ? $"หมายเลข: {data.Address}{Environment.NewLine}" : string.Empty;
            var messageText = $"โปรดยืนยันการดำเนินการนี้{Environment.NewLine}ธุรกรรม: {data.Operation}{Environment.NewLine}ช่องทาง: {data.Method}{Environment.NewLine}{qrCodeText}จำนวน: {data.Amount} บาท{Environment.NewLine}ถูกต้องหรือไม่?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var data = (PaymentInfo)stepContext.Options;
                return await stepContext.EndDialogAsync(data, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

    public class OperationInfo
    {
        public string MainOperation { get; set; }
        public string SubOperation { get; set; }
        public string MethodOperation { get; set; }
        public string Amount { get; set; }
    }
}
