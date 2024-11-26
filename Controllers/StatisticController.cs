using Microsoft.Extensions.Caching.Memory;
using SportStats.Enums;
using SportStats.Interfaces;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SportStats.Controllers
{
    public class StatisticController : BaseController, IStatistic
    {
        public StatisticController(Models.User user, ITelegramBotClient bot, Chat chat, IMemoryCache cache) : base(user, bot, chat, cache)
        {

        }

        public void Stats(string text)
        {
            try
            {

            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }

        public void StatsByMuscleGroup(string text)
        {
            try
            {

            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
