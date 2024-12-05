using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using SportStats.Enums;
using SportStats.Interfaces;
using SportStats.Models;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SportStats.Controllers
{
    public class StatisticController : BaseController, IStatistic
    {
        public StatisticController(Models.User user, ITelegramBotClient bot, Chat chat, IMemoryCache cache, Service service) : base(user, bot, chat, cache, service) { }

        public async void StatsByExercise(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (int.TryParse(text, out int num))
                    {
                        using (var db = new SportContext())
                        {
                            var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).OrderBy(e => e.CreatedOn).ToList();

                            if (num - 1 <= exercises.Count)
                            {
                                var exercise = exercises[num - 1];

                                if (exercise is null)
                                    throw new Exception("exercise is null");

                                var reports = db.ExerciseReports
                                    .Where(e => e.ExerciseId == exercise.ExerciseId && e.CreatedOn >= DateTime.Now.AddMonths(-2));

                                if (reports != null)
                                {
                                    var myStat_date = new List<DateTime>();
                                    var myStat_weight = new List<int>();
                                    var myStat_numOfRepetitions = new List<int>();

                                    var uniqueReports = reports.GroupBy(e => e.CreatedOn).Select(group => group.OrderByDescending(report => report.Weight)
                                           .ThenByDescending(report => report.NumOfRepetitions)
                                           .First()
                                    );

                                    foreach (var rep in uniqueReports)
                                    {
                                        if (rep.CreatedOn is null)
                                            break;
                                        myStat_date.Add(rep.CreatedOn.Value);
                                        myStat_weight.Add(rep.Weight);
                                        myStat_numOfRepetitions.Add(rep.NumOfRepetitions);
                                    }

                                    var plot = Utils.CreateExercisesPlot(myStat_date, myStat_weight, myStat_numOfRepetitions, exercise.ExerciseName);

                                    plot.SavePng($"{exercise.ExerciseId}.png", 650, 600);

                                    using (var fileStream = new FileStream($"{exercise.ExerciseId}.png", FileMode.Open, FileAccess.Read))
                                    {
                                        await _bot.SendPhoto(_chat.Id, photo: InputFile.FromStream(fileStream, $"{exercise.ExerciseId}.png"));
                                        var message = $"Результат по упражнению <b>«{exercise.ExerciseName}»</b> ⬆⬆⬆\n<b>Напиши следующий номер</b>";
                                        await _bot.SendMessage(_chat.Id, message, replyMarkup: new InlineKeyboardMarkup().AddButton("На главную", "Main"), parseMode: ParseMode.Html);
                                    }
                                }

                            }
                            else
                            {
                                await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                            }
                        }
                    }
                    else
                    {
                        await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
                Console.WriteLine(ex.Message);
            }
        }

        public async void TrainingDayStats(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (int.TryParse(text, out int num))
                    {
                        using (var db = new SportContext())
                        {
                            var trainingDay = db.TrainingDays
                                .Include(e => e.Exercises)
                                .FirstOrDefault(e => e.ScheduleId == _user.CurrentScheduleId && e.SequenceNumber == num);

                            if (trainingDay is null)
                            {
                                await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                            }

                            var workouts = db.Workouts
                                .Include(e => e.ExerciseReports)
                                .Where(e => e.CreatedOn >= DateTime.Now.AddMonths(-2) && e.UserId == _user.UserId && e.TrainingDayId == trainingDay.TrainingDayId)
                                .OrderBy(e => e.CreatedOn).ToList();

                            if (workouts is null)
                            {
                                await _bot.SendMessage(_chat.Id, $"Тренировки не найдены");
                                return;
                            }

                            var plot = Utils.CreateWorkoutPlot(workouts.ToList(), trainingDay.TrainingDayName);
                            plot.SavePng($"{trainingDay.TrainingDayId}.png", 650, 500);

                            using (var fileStream = new FileStream($"{trainingDay.TrainingDayId}.png", FileMode.Open, FileAccess.Read))
                            {
                                await _bot.SendPhoto(_chat.Id, photo: InputFile.FromStream(fileStream, $"{trainingDay.TrainingDayId}.png"));
                                var message = $"Результат по тренировочному дню <b>«{trainingDay.TrainingDayName}»</b> ⬆⬆⬆\n<b>Напиши следующий номер</b>";
                                await _bot.SendMessage(_chat.Id, message, replyMarkup: new InlineKeyboardMarkup().AddButton("На главную", "Main"), parseMode: ParseMode.Html);
                            }
                        }
                    }
                    else
                    {
                        await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
