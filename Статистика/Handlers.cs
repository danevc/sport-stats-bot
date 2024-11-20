using Stats.Api;
using Stats.Enums;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Stats
{
    public static class Handlers
    {
        [Obsolete]
        public static async Task<State> TextHandler(string text, State state, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            var _state = state;

            if (text == Utils._btn_ToHome)
            {
                History.Clear();
                return await StartApi.Start(text, botClient, chat, user);
            }

            switch (state)
            {
                case State.Start:
                    _state = await StartApi.Start(text, botClient, chat, user);
                    break;
                case State.CreateSchedule:
                    _state = await SportApi.CreateSchedule(text, botClient, chat, user);
                    break;
                case State.AddDayOfWeek:
                    _state = await SportApi.AddDayOfWeek(text, botClient, chat, user);
                    break;
                case State.OnSaveScheduler:
                    _state = await SportApi.OnSaveScheduler(text, botClient, chat, user);
                    break;
                case State.Workout:
                    _state = await SportApi.Workout(text, botClient, chat, user);
                    break;
                case State.AddExercise:
                    _state = await SportApi.AddExercise(text, botClient, chat, user);
                    break;
                case State.ChooseDayToWork:
                    _state = await StartApi.ChooseDayToWork(text, botClient, chat, user);
                    break;
                case State.RealizeExercise:
                    _state = await SportApi.RealizeExercise(text, botClient, chat, user);
                    break;
                case State.Stats:
                    _state = await SportApi.Stats(text, botClient, chat, user);
                    break;
                case State.StatsByExercise:
                    _state = await SportApi.StatsByExercise(text, botClient, chat, user);
                    break;
                default:
                    break;
            }


            return _state;
        }
    }
}
