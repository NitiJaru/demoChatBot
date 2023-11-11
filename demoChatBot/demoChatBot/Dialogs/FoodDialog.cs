using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using demoChatBot.Models;

namespace DemoEchoBot.Dialogs
{
    public class FoodDialog : ComponentDialog
    {
        public FoodDialog() : base(nameof(FoodDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            var waterfallSteps = new WaterfallStep[]
            {
                AskFoodType,
                //AskMethod,
                //AskAmount,
                //ConfirmOperationAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskFoodType(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            {
                var messageText = "ทานอาหารประเภทไหนดี?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = promptMessage,
                    Choices = new[]
                    {
                        new Choice { Value = "ก๋วยเตี๋ยว"},
                        new Choice { Value = "ข้าวแกง"},
                        new Choice { Value = "ของหวาน"},
                        new Choice { Value = "น้ำดื่ม"},
                        new Choice { Value = "ชาบู"},
                    }
                }, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (FoundChoice)stepContext.Result;
            return await stepContext.EndDialogAsync(new FoodInfo { FoodType = result.Value }, cancellationToken);
        }
    }
}
