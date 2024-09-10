// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using demoChatBot.Models;
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
        private readonly IBotStateService _botStateService;
        private readonly IRestClientService _restClientService;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public NotifyController(IBotStateService botStateService, IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, IRestClientService restClientService)
        {
            _botStateService = botStateService;
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
            IMessageActivity messageActivity;
            var image = new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/openResturent_Rich_Message.png");
            var buttondetail = new List<CardAction> { new CardAction(ActionTypes.ImBack, "ยืนยัน", value: "ยืนยันการเปิดร้าน"), new CardAction(ActionTypes.ImBack, "ยกเลิก", value: "ยกเลิกการเปิดร้าน") };
            messageActivity = getHeroCard("พิเศษ", "คุณต้องการยกเลิกใช่หรือไม่", "", image, buttondetail);
            //await turnContext.SendActivityAsync("proactive hello");
            //var heroCard = new HeroCard
            //{
            //    Title = "พิเศษ",
            //    Text = "คุณต้องการยกเลิกใช่หรือไม่",
            //    //Images = new List<CardImage> { new CardImage("https://failfast.blob.core.windows.net/upload/Delivery/openResturent_Rich_Message.png") },
            //    Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "ยืนยัน", value: "ยืนยันการเปิดร้าน"), new CardAction(ActionTypes.ImBack, "ยกเลิก", value: "ยกเลิกการเปิดร้าน") }
            //};

            ////Create a new activity with the Hero Card attachment
            //var activity = turnContext.Activity.CreateReply();
            //activity.Attachments = new[] { heroCard.ToAttachment() };
            //activity.Text = "ยืนยันการเปิดร้าน";
            // Send the activity
            await turnContext.SendActivityAsync(messageActivity, cancellationToken);
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
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                if (userDetails.RestaurantId != resId) return;
                IMessageActivity messageActivity;
                var resOrderApi = $"https://liff.line.me/2006157455-3dNXrwAO#order-main";
                var button = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิด", value: resOrderApi) };
                messageActivity = getHeroCard("ดูข้อมูลออเดอร์หรืออัพเดทสถานะออเดอร์", "", "", null, button);
                await turnContext.SendActivityAsync(messageActivity, cancellationToken);
            }
        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> UpdateMenu(string resId)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                if (userDetails.RestaurantId != resId) return;
                IMessageActivity messageActivity;
                var resOrderApi = $"https://liff.line.me/2006157455-3dNXrwAO#menuupdate-main";
                var image = new CardImage("");
                var button = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิด", value: resOrderApi) };
                messageActivity = getHeroCard("ดูเมนูอัพเดท", "", "", null, button);
                await turnContext.SendActivityAsync(messageActivity, cancellationToken);
            }

        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> CancleOrderByAdmin(string resId)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                if (userDetails.RestaurantId != resId) return;
                await turnContext.SendActivityAsync("ออเดอร์ถูกยกเลิก");
            }

        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> DenyOrderByAdmin(string resId)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                if (userDetails.RestaurantId != resId) return;
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
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                if (userDetails.RestaurantId != resId) return;
                var activity = Activity.CreateMessageActivity();
                activity.Text = "สถานะร้าน ปิด";
                var choices = new List<string> { "เปิดร้าน" };
                var reply = MessageFactory.SuggestedActions(choices, activity.Text, null, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(reply);
            }
        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> OpenRestaurantByAdmin(string resId)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                if (userDetails.RestaurantId != resId) return;
                userDetails.StatusRestaurant = true;
                await _botStateService.SaveChangesAsync(turnContext);
                await turnContext.SendActivityAsync("สถานะร้านถูกเปิดจากแอดมิน");
            }
        }

        [HttpGet("{resId}")]
        public async Task<IActionResult> CloseRestaurantByAdmin(string resId)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                if (userDetails.RestaurantId != resId) return;
                var activity = Activity.CreateMessageActivity();
                activity.Text = "สถานะร้านถูกปิดจากแอดมิน";
                var choices = new List<string> { "เปิดร้าน" };
                var reply = MessageFactory.SuggestedActions(choices, activity.Text, null, InputHints.ExpectingInput);
                userDetails.StatusRestaurant = false;
                await _botStateService.SaveChangesAsync(turnContext);
                await turnContext.SendActivityAsync(reply);
            }
        }

        [HttpGet("{resId}/{botUserId}/{isApprove}")]
        public async Task<IActionResult> LinkRestaurantAccount(string resId, string botUserId, bool isApprove)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var activity = Activity.CreateMessageActivity();
                if (turnContext.Activity.From.Id != botUserId) return;
                if (isApprove)
                {
                    var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                    var resturnonAPI = $"{APIBaseUrl}/api/Restaurant/GetRestaurantInfo/{resId}";
                    var respon = await _restClientService.Get<RestaurantShortResponse>(resturnonAPI, botUserId);
                    restaurantDetails.RestaurantName = respon.Name;
                    restaurantDetails.IsLinkedAccount = true;
                    restaurantDetails.BaId = resId;
                    restaurantDetails.RestaurantId = respon._id;
                    restaurantDetails.StatusRestaurant = respon.IsStandby;
                    var textstatus = restaurantDetails.StatusRestaurant ? "เปิดอยู่" : "ปิดอยู่";
                    activity.Text = $"คุณ {respon.Name} ได้ทำการผูก line account กับ mana เรียบร้อยแล้ว สถานะร้านค้า {textstatus}";
                    await _botStateService.SaveChangesAsync(turnContext);
                    await turnContext.SendActivityAsync(activity);
                }
                else
                {
                    IMessageActivity messageActivity;
                    var button = new List<CardAction> { new(ActionTypes.ImBack, title: "เริ่มผูกบัญชีใหม่", value: "เริ่มผูกบัญชีใหม่") };
                    messageActivity = getHeroCard("คุณถูกปฎิเสธการผูก line account กับ mana", "", "", null, button);
                    await turnContext.SendActivityAsync(messageActivity, cancellationToken);
                }
            }
        }
        private IMessageActivity getHeroCard(string title, string subtitle, string url, CardImage imagecard, List<CardAction> buttondetail)
        {
            if (imagecard is not null)
            {
                var card = new HeroCard
                {
                    Title = title,
                    Subtitle = $"{url}",
                    Images = new List<CardImage> { imagecard },
                    Buttons = buttondetail
                };
                var attachment = card.ToAttachment();
                return MessageFactory.Attachment(attachment);
            }
            else
            {
                var card = new HeroCard
                {
                    Title = title,
                    Subtitle = $"{url}",
                    Buttons = buttondetail
                };
                var attachment = card.ToAttachment();
                return MessageFactory.Attachment(attachment);
            }
        }
    }
}
