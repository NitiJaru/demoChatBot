// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DemoEchoBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.BotBuilderSamples.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly string APIBaseUrl = "https://delivery-3rd-test-api.azurewebsites.net";
        private readonly IRestClientService _restClientService;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, IRestClientService restClientService)
        {

            _adapter = adapter;
            _restClientService = restClientService;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }

        public async Task<IActionResult> Get()
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }

            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = "<html><body><h1>Proactive messages have been sent.</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            //await turnContext.SendActivityAsync("proactive hello");
            var heroCard = new HeroCard
            {
                Title = "พิเศษ",
                Text = "คุณต้องการยกเลิกใช่หรือไม่",
                //Images = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/openResturent_Rich_Message.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "ยืนยัน", value: "ยืนยันการเปิดร้าน"), new CardAction(ActionTypes.ImBack, "ยกเลิก", value: "ยกเลิกการเปิดร้าน") }
            };

            // Create a new activity with the Hero Card attachment
            var activity = turnContext.Activity.CreateReply();
            activity.Attachments = new[] { heroCard.ToAttachment() };
            activity.Text = "ยืนยันการเปิดร้าน";
            // Send the activity
            await turnContext.SendActivityAsync(activity, cancellationToken);
        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> Ordering(string resId)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var resOrderApi = $"https://devster-delivery-test.onmana.space/apprestaurant/index.html#/order-main";
                var heroCard = new HeroCard
                {
                    Title = "ดูข้อมูลออเดอร์หรืออัพเดทสถานะออเดอร์",
                    Text = $"ผ่านจากลิงค์นี้ {resOrderApi}",
                    //Images = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/openResturent_Rich_Message.png") },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิดลิงค์", value: resOrderApi) }
                };

                var activity = turnContext.Activity.CreateReply();
                activity.Attachments = new[] { heroCard.ToAttachment() };
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }
        }


        [HttpGet("{resId}")]
        public async Task<IActionResult> UpdateMenu(string resId)
        {
            var resDetailsApi = $"https://jsonplaceholder.typicode.com/todos/1";
            var request = await _restClientService.Get<object>(resDetailsApi);
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var heroCard = new HeroCard
                {
                    Title = "มีเมนูอัพเดท",
                    Images = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/newupdate.png") },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "ดูรายละเอียด", value: "https://devster-delivery-test.onmana.space/apprestaurant/index.html#/menuupdate-main") }
                };

                var activity = turnContext.Activity.CreateReply();
                activity.Attachments = new[] { heroCard.ToAttachment() };
                await turnContext.SendActivityAsync(activity, cancellationToken);
            }

        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> CancleOrderByAdmin(string resId)
        {
            var resDetailsApi = $"https://jsonplaceholder.typicode.com/todos/1";
            var request = await _restClientService.Get<object>(resDetailsApi);
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                await turnContext.SendActivityAsync("คำขอยกเลิกออเดอร์ได้รับการอนุมัติแล้ว");

            }

        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> DenyOrderByAdmin(string resId)
        {
            var resDetailsApi = $"https://jsonplaceholder.typicode.com/todos/1";
            var request = await _restClientService.Get<object>(resDetailsApi);
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                await turnContext.SendActivityAsync("คำขอยกเลิกออเดอร์ไม่ได้รับการอนุมัติ");
            }

        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> CloseRestaurant(string resId)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var activity = Activity.CreateMessageActivity();
                activity.Text = "สถานะร้าน ปิด";
                //await turnContext.SendActivityAsync(activity);
                var choices = new List<string> { "เปิดร้าน" };
                var reply = MessageFactory.SuggestedActions(choices, activity.Text, null, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(reply);
            }
        }

    }
}
