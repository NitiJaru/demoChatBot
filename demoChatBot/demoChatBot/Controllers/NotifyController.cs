// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using demoChatBot;
using demoChatBot.Models;
using demoChatBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotStateService _botStateService;
        private readonly IRestClientService _restClientService;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConnectionSettings _connectionSetting;

        private readonly IConversationReferenceRepository _referenceRepository;

        //private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public NotifyController(IBotStateService botStateService, IBotFrameworkHttpAdapter adapter, IConfiguration configuration, IRestClientService restClientService, IConversationReferenceRepository referenceRepository, ConnectionSettings connectionSetting)
        {
            _botStateService = botStateService;
            _adapter = adapter;
            _restClientService = restClientService;
            _referenceRepository = referenceRepository;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _connectionSetting = connectionSetting;
        }

        [HttpPost]
        public async Task<IActionResult> Ordering(OrderingRequest request)
        {
            var invalid = request is null || request.ChatBotIds is null || request.ChatBotIds.Count is 0;
            if (invalid) return Ok();

            var conversationReferences = await _referenceRepository.ListConversationReferenceAsync(request.ChatBotIds);
            foreach (var conversationReference in conversationReferences)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                IMessageActivity messageActivity;
                //   IMessageActivity messageActivity2;
                //   messageActivity2 = getHeroCard($"botUserId:{botUserId}{Environment.NewLine}userDetails.RestaurantId: {userDetails.RestaurantId}",
                //"", "", null, null);
                //await turnContext.SendActivityAsync(messageActivity2, cancellationToken);
                var resOrderApi = $"{_connectionSetting.LineRestaurantOrderMain}";
                var button = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิด", value: resOrderApi) };
                messageActivity = getHeroCard("ดูข้อมูลออเดอร์หรืออัพเดทสถานะออเดอร์", "", "", null, button);
                await turnContext.SendActivityAsync(messageActivity, cancellationToken);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMenu(OrderingRequest request)
        {

            var invalid = request is null || request.ChatBotIds is null || request.ChatBotIds.Count is 0;
            if (invalid) return Ok();

            var conversationReferences = await _referenceRepository.ListConversationReferenceAsync(request.ChatBotIds);
            foreach (var conversationReference in conversationReferences)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                IMessageActivity messageActivity;
                //   IMessageActivity messageActivity2;
                //   messageActivity2 = getHeroCard($"botUserId:{botUserId}{Environment.NewLine}userDetails.RestaurantId: {userDetails.RestaurantId}",
                //"", "", null, null);
                //await turnContext.SendActivityAsync(messageActivity2, cancellationToken);
                var resOrderApi = $"{_connectionSetting.LineRestaurantMenuUpdate}";
                var image = new CardImage("");
                var button = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "เปิด", value: resOrderApi) };
                messageActivity = getHeroCard("ดูเมนูอัพเดท", "", "", null, button);
                await turnContext.SendActivityAsync(messageActivity, cancellationToken);
            }

        }

        [HttpPost]
        public async Task<IActionResult> CancleOrderByAdmin(OrderingRequest request)
        {
            var invalid = request is null || request.ChatBotIds is null || request.ChatBotIds.Count is 0;
            if (invalid) return Ok();

            var conversationReferences = await _referenceRepository.ListConversationReferenceAsync(request.ChatBotIds);
            foreach (var conversationReference in conversationReferences)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
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


        [HttpPost]
        public async Task<IActionResult> OpenRestaurant(OrderingRequest request)
        {
            var invalid = request is null || request.ChatBotIds is null || request.ChatBotIds.Count is 0;
            if (invalid) return Ok();

            var conversationReferences = await _referenceRepository.ListConversationReferenceAsync(request.ChatBotIds);
            foreach (var conversationReference in conversationReferences)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                userDetails.StatusRestaurant = true;
                await _botStateService.SaveChangesAsync(turnContext);
            }
        }
        [HttpPost]
        public async Task<IActionResult> CloseRestaurant(OrderingRequest request)
        {
            var invalid = request is null || request.ChatBotIds is null || request.ChatBotIds.Count is 0;
            if (invalid) return Ok();

            var conversationReferences = await _referenceRepository.ListConversationReferenceAsync(request.ChatBotIds);
            foreach (var conversationReference in conversationReferences)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                userDetails.StatusRestaurant = false;
                await _botStateService.SaveChangesAsync(turnContext);
            }
        }


        [HttpPost]
        public async Task<IActionResult> OpenRestaurantByAdmin(OrderingRequest request)
        {
            var invalid = request is null || request.ChatBotIds is null || request.ChatBotIds.Count is 0;
            if (invalid) return Ok();

            var conversationReferences = await _referenceRepository.ListConversationReferenceAsync(request.ChatBotIds);
            foreach (var conversationReference in conversationReferences)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                userDetails.StatusRestaurant = true;
                await turnContext.SendActivityAsync("สถานะร้านถูกเปิดจากแอดมิน");
                await _botStateService.SaveChangesAsync(turnContext);

            }
        }

        [HttpPost]
        public async Task<IActionResult> CloseRestaurantByAdmin(OrderingRequest request)
        {
            var invalid = request is null || request.ChatBotIds is null || request.ChatBotIds.Count is 0;
            if (invalid) return Ok();

            var conversationReferences = await _referenceRepository.ListConversationReferenceAsync(request.ChatBotIds);
            foreach (var conversationReference in conversationReferences)
            {
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }
            return Ok();

            async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                var userDetails = await _botStateService.UserDetailsAccessor.GetAsync(turnContext, () => new RestaurantDetails(), cancellationToken);
                userDetails.StatusRestaurant = false;
                await turnContext.SendActivityAsync("สถานะร้านถูกปิดจากแอดมิน");
                await _botStateService.SaveChangesAsync(turnContext);
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
                if (turnContext.Activity.From.Id != botUserId) return;
                if (isApprove)
                {
                    var resturnonAPI = $"{_connectionSetting.DeliveryAPIBaseUrl}/api/Restaurant/GetRestaurantInfo/{resId}";
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
