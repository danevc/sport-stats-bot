using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Caching.Memory;

namespace SportStats.Controllers
{
    public class BaseController
    {
        protected ITelegramBotClient _bot;
        protected Chat _chat;
        protected Models.User _user;
        protected readonly IMemoryCache _cache;
        protected Service _service;

        public BaseController(Models.User user, ITelegramBotClient botClient, Chat chat, IMemoryCache cache, Service service)
        {
            _cache = cache;
            _chat = chat;
            _bot = botClient;
            _user = user;
            _service = service;
        }
    }
}
