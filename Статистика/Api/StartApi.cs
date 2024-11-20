using Stats.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Stats.Api
{
    public static class StartApi
    {
        [Obsolete]
        public static async Task<State> Start(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            var sportSchedule = History.GetSchedule();
            
            if (sportSchedule.Any())
            {
                if (text == Utils._btn_Workout)
                {
                    var dayOfWeek = History.GetDay();
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.StartWithSchedule);

                    if (!sportSchedule.Any(e => (int)e.DayOfWeek == dayOfWeek))
                    {
                        await botClient.SendTextMessageAsync(chat.Id, "По расписанию сегодня тренировок нет", replyMarkup: replyKeyboard);
                        return State.Start;
                    }
                    else
                    {
                        replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.Workout);
                        await botClient.SendTextMessageAsync(chat.Id, $"Тренировка <b>«{sportSchedule?.FirstOrDefault(e => (int)e.DayOfWeek == dayOfWeek)?.MuscleGroupName}»</b>💪\n\nℹ Выбери упражнение\nПиши результат выполнения в формате кг-повторений.\nЧто бы закончить напиши -", replyMarkup: replyKeyboard, parseMode: ParseMode.Html);
                        return State.Workout;
                    }
                }
                else if (text == Utils._btn_Schedule)
                {
                    var schedule = SportApi.ViewSchedule();
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.StartWithSchedule);
                    await botClient.SendTextMessageAsync(chat.Id, schedule, replyMarkup: replyKeyboard);
                    return State.Start;
                }
                else if (text == Utils._btn_Stats)
                {
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.Stats);
                    await botClient.SendTextMessageAsync(chat.Id, "Раздел статистика", replyMarkup: replyKeyboard);
                    return State.Stats;
                }
                else if (text == Utils._btn_ChooseDay)
                {
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.ChooseDayOfWeek);
                    await botClient.SendTextMessageAsync(chat.Id, "Выбери день с которым хочешь работать", replyMarkup: replyKeyboard);
                    return State.ChooseDayToWork;
                }
                else
                {
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.StartWithSchedule);
                    await botClient.SendTextMessageAsync(chat.Id, "<b>Привет!</b>✌", replyMarkup: replyKeyboard, parseMode: ParseMode.Html);
                    return State.Start;
                }
            }
            else
            {
                if (text == "Добавить расписание")
                {
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.ChooseDayOfWeek);
                    await botClient.SendTextMessageAsync(chat.Id, "Выбери день", replyMarkup: replyKeyboard);
                    return State.AddDayOfWeek;
                }
                else
                {
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.AddSchedule);
                    await botClient.SendTextMessageAsync(chat.Id, "Добавь расписание", replyMarkup: replyKeyboard);
                    return State.Start;

                }
            }
        }

        [Obsolete]
        public static async Task<State> ChooseDayToWork(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.StartWithSchedule);
            switch (text)
            {
                case "Понедельник":
                    History.SetDay((int)MyDayOfWeek.Monday);
                    break;
                case "Вторник":
                    History.SetDay((int)MyDayOfWeek.Tuesday);
                    break;
                case "Среда":
                    History.SetDay((int)MyDayOfWeek.Wednesday);
                    break;
                case "Четверг":
                    History.SetDay((int)MyDayOfWeek.Thursday);
                    break;
                case "Пятница":
                    History.SetDay((int)MyDayOfWeek.Friday);
                    break;
                case "Суббота":
                    History.SetDay((int)MyDayOfWeek.Saturday);
                    break;
                case "Воскресенье":
                    History.SetDay((int)MyDayOfWeek.Sunday);
                    break;
                default:
                    await botClient.SendTextMessageAsync(chat.Id, "Такого дня нет в моем списке", replyMarkup: replyKeyboard);
                    return State.Start;
            }

            await botClient.SendTextMessageAsync(chat.Id, $"Выбран день {text}", replyMarkup: replyKeyboard);
            return State.Start;
        }
    }
}
