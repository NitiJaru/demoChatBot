// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using demoChatBot.Models;
using demoChatBot.Services;
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
        private readonly string APIBaseUrl = "https://delivery-3rd-th-api.azurewebsites.net";
        private readonly IBotStateService _botStateService;
        private readonly IRestClientService _restClientService;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;

        private readonly IConversationReferenceRepository _referenceRepository;

        //private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public NotifyController(IBotStateService botStateService, IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IRestClientService restClientService, IConversationReferenceRepository referenceRepository)
        {
            _botStateService = botStateService;
            _adapter = adapter;
            _restClientService = restClientService;
            _referenceRepository = referenceRepository;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }


        [HttpGet("{botUserId}")]
        public async Task<IActionResult> Ordering(string botUserId)
        {
            var conversationReference = await _referenceRepository.GetConversationReferenceAsync(botUserId);
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                IMessageActivity messageActivity;
             //   IMessageActivity messageActivity2;
             //   messageActivity2 = getHeroCard($"botUserId:{botUserId}{Environment.NewLine}userDetails.RestaurantId: {userDetails.RestaurantId}",
             //"", "", null, null);
                //await turnContext.SendActivityAsync(messageActivity2, cancellationToken);
                var resOrderApi = $"https://liff.line.me/2006157455-3dNXrwAO#order-main";
                var button = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิด", value: resOrderApi) };
                messageActivity = getHeroCard("ดูข้อมูลออเดอร์หรืออัพเดทสถานะออเดอร์", "", "", null, button);
                await turnContext.SendActivityAsync(messageActivity, cancellationToken);
            }
        }

        [HttpGet("{botUserId}")]
        public async Task<IActionResult> UpdateMenu(string botUserId)
        {
            var conversationReference = await _referenceRepository.GetConversationReferenceAsync(botUserId);
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                IMessageActivity messageActivity;
             //   IMessageActivity messageActivity2;
             //   messageActivity2 = getHeroCard($"botUserId:{botUserId}{Environment.NewLine}userDetails.RestaurantId: {userDetails.RestaurantId}",
             //"", "", null, null);
                //await turnContext.SendActivityAsync(messageActivity2, cancellationToken);
                var resOrderApi = $"https://liff.line.me/2006157455-3dNXrwAO#menuupdate-main";
                var image = new CardImage("");
                var button = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิด", value: resOrderApi) };
                messageActivity = getHeroCard("ดูเมนูอัพเดท", "", "", null, button);
                await turnContext.SendActivityAsync(messageActivity, cancellationToken);
            }

        }

        [HttpGet("{botUserId}")]
        public async Task<IActionResult> CancleOrderByAdmin(string botUserId)
        {
            var conversationReference = await _referenceRepository.GetConversationReferenceAsync(botUserId);
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                //IMessageActivity messageActivity2;
                //messageActivity2 = getHeroCard($"botUserId:{botUserId}", "", $"userDetails.RestaurantId: {userDetails.RestaurantId}", null, null);
                //await turnContext.SendActivityAsync(messageActivity2, cancellationToken);
                await turnContext.SendActivityAsync("ออเดอร์ถูกยกเลิก");
            }

        }

        [HttpGet("{botUserId}")]
        public async Task<IActionResult> CloseRestaurant(string botUserId)
        {
            var conversationReference = await _referenceRepository.GetConversationReferenceAsync(botUserId);
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                var activity = Activity.CreateMessageActivity();
                activity.Text = "สถานะร้าน ปิด";
                var choices = new List<string> { "เปิดร้าน" };
                var reply = MessageFactory.SuggestedActions(choices, activity.Text, null, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(reply);
            }
        }

        [HttpGet("{botUserId}")]
        public async Task<IActionResult> OpenRestaurantByAdmin(string botUserId)
        {
            var conversationReference = await _referenceRepository.GetConversationReferenceAsync(botUserId);
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
             //   IMessageActivity messageActivity2;
             //   messageActivity2 = getHeroCard($"botUserId:{botUserId}{Environment.NewLine}userDetails.RestaurantId: {userDetails.RestaurantId}",
             //"", "", null, null);
                //await turnContext.SendActivityAsync(messageActivity2, cancellationToken);
                userDetails.StatusRestaurant = true;
                await _botStateService.SaveChangesAsync(turnContext);
                await turnContext.SendActivityAsync("สถานะร้านถูกเปิดจากแอดมิน");
            }
        }

        [HttpGet("{botUserId}")]
        public async Task<IActionResult> CloseRestaurantByAdmin(string botUserId)
        {
            var conversationReference = await _referenceRepository.GetConversationReferenceAsync(botUserId);
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            return Ok();


            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
             //   IMessageActivity messageActivity2;
             //   messageActivity2 = getHeroCard($"botUserId:{botUserId}{Environment.NewLine}userDetails.RestaurantId: {userDetails.RestaurantId}",
             //"", "", null, null);
                //await turnContext.SendActivityAsync(messageActivity2, cancellationToken);
                var activity = Activity.CreateMessageActivity();
                activity.Text = "สถานะร้านถูกปิดจากแอดมิน";
                var choices = new List<string> { "เปิดร้าน" };
                var reply = MessageFactory.SuggestedActions(choices, activity.Text, null, InputHints.ExpectingInput);
                userDetails.StatusRestaurant = false;
                await _botStateService.SaveChangesAsync(turnContext);
                await turnContext.SendActivityAsync(reply);
            }
        }

        [HttpGet("{botUserId}/{resId}/{isApprove}")]
        public async Task<IActionResult> LinkRestaurantAccount(string botUserId, string resId, bool isApprove)
        {
            var conversationReference = await _referenceRepository.GetConversationReferenceAsync(botUserId);
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var activity = Activity.CreateMessageActivity();
                var restaurantDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                IMessageActivity messageActivity;
                //messageActivity = getHeroCard($"ResId:{resId}{Environment.NewLine}botUserId: {botUserId}{Environment.NewLine}isApprove: {isApprove}", "", "", null, null);
                //await turnContext.SendActivityAsync(messageActivity, cancellationToken);
                if (turnContext.Activity.From.Id != botUserId) return;
                if (isApprove)
                {
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
