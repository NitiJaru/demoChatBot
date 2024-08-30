using demoChatBot.Models;
using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace DemoEchoBot.Services
{
    public interface IBotStateService
    {
        IStatePropertyAccessor<RestaurantDetails> UserDetailsAccessor { get; }
        Task SaveChangesAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default);
    }
    public class BotStateService : IBotStateService
    {
        private readonly UserState _userState;
        public IStatePropertyAccessor<RestaurantDetails> UserDetailsAccessor { get; }
        public BotStateService(UserState userState)
        {
            _userState = userState;
            UserDetailsAccessor = userState.CreateProperty<RestaurantDetails>("RestaurantDetails");
        }
        public async Task SaveChangesAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default)
        {
            await _userState.SaveChangesAsync(turnContext, force, cancellationToken);
        }
    }
}
