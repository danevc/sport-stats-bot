using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ScottPlot;
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
        public StatisticController(Models.User user, ITelegramBotClient bot, Chat chat, IMemoryCache cache, Service service, IConfigurationRoot config) : base(user, bot, chat, cache, service, config) { }

        public async void StatsByExercise(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (int.TryParse(text, out int num))
                    {
                        using (var db = new SportContext(_config))
                        {
                            var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).OrderBy(e => e.CreatedOn).ToList();

                            if (num - 1 <= exercises.Count)
                            {
                                var exercise = exercises[num - 1];

                                if (exercise is null)
                                    throw new Exception("exercise is null");

                                var reports = db.ExerciseReports
                                    .Where(e => e.ExerciseId == exercise.ExerciseId && e.CreatedOn >= DateTime.Now.AddMonths(-2)).ToList();

                                if (reports != null)
                                {
                                    var myStat_date = new List<DateTime>();
                                    var myStat_weight = new List<double>();
                                    var myStat_numOfRepetitions = new List<double>();
                                    var bars = new List<PlotBarElem>();
                                    var uniqueReports = reports.GroupBy(e => e.CreatedOn.Value.Date)
                                        .Select(group => group.OrderByDescending(report => report.Weight)
                                            .ThenByDescending(report => report.NumOfRepetitions)
                                            .First()
                                    ).ToList();

                                    foreach (var rep in uniqueReports.OrderBy(e => e.CreatedOn))
                                    {
                                        if (rep.CreatedOn is null)
                                            break;
                                        myStat_date.Add(rep.CreatedOn.Value);
                                        myStat_weight.Add(rep.Weight);
                                        myStat_numOfRepetitions.Add(rep.NumOfRepetitions);
                                    }
                                    bars.Add(new PlotBarElem
                                    {
                                        Values = myStat_weight,
                                        color = Colors.Tomato,
                                        Title = "weight",
                                        Type = BarPlotTypes.Number
                                    });
                                    bars.Add(new PlotBarElem
                                    {
                                        Values = myStat_numOfRepetitions,
                                        color = Colors.LightBlue,
                                        Title = "numOfRepetitions",
                                        Type = BarPlotTypes.Number
                                    });

                                    var plot = Utils.CreateBarPlot(myStat_date, bars, exercise.ExerciseName);

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
                        using (var db = new SportContext(_config))
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

                            var dates = new List<DateTime>();
                            var bars1 = new List<PlotBarElem>();
                            var bars2 = new List<PlotBarElem>();
                            var coefBar = new List<double>();
                            var durationBar = new List<double>();
                            var heartRateBar = new List<double>();
                            var caloriesBar = new List<double>();

                            var averageDuration = 0;
                            var averageCalories = 0.0;
                            var averageHeartRate = 0.0;
                            var averageCoef = 0.0;

                            foreach (var w in workouts)
                            {
                                dates.Add(w.CreatedOn);
                                var coefficient = 0.0;
                                foreach (var exReport in w.ExerciseReports)
                                {
                                    coefficient += exReport.Weight != 0 ? exReport.Weight * exReport.NumOfRepetitions : exReport.NumOfRepetitions;
                                }
                                coefficient = Math.Round(coefficient / 250, 2);
                                coefBar.Add(coefficient);

                                var workoutDuration
                                    = w.Duration == 0
                                    ? Convert.ToInt32((w.ExerciseReports.Max(e => e.CreatedOn) - w.ExerciseReports.Min(e => e.CreatedOn)).Value.TotalMinutes)
                                    : w.Duration;

                                averageDuration += workoutDuration;
                                averageCalories += w.Calories;
                                averageHeartRate += w.AverageHeartRate;
                                averageCoef += coefficient;

                                durationBar.Add(workoutDuration);
                                heartRateBar.Add(w.AverageHeartRate);
                                caloriesBar.Add(w.Calories);
                            }

                            averageDuration /= workouts.Count;
                            averageCalories /= workouts.Where(e => e.Calories != 0).Count();
                            averageHeartRate /= workouts.Where(e => e.AverageHeartRate != 0).Count();
                            averageCoef /= workouts.Count;

                            bars1.Add(new PlotBarElem
                            {
                                Values = coefBar,
                                color = Colors.Tomato,
                                Title = "Коэффициент",
                                Type = BarPlotTypes.Number
                            });
                            bars1.Add(new PlotBarElem
                            {
                                Values = durationBar,
                                color = Colors.LightBlue,
                                Title = "Продолжительность тренировки",
                                Type = BarPlotTypes.Time
                            });
                            bars2.Add(new PlotBarElem
                            {
                                Values = heartRateBar,
                                color = Colors.Tomato,
                                Title = "Средний пульс",
                                Type = BarPlotTypes.Number
                            });
                            bars2.Add(new PlotBarElem
                            {
                                Values = caloriesBar,
                                color = Colors.LightBlue,
                                Title = "Калории",
                                Type = BarPlotTypes.Number
                            });

                            var plot1 = Utils.CreateBarPlot(dates, bars1, trainingDay.TrainingDayName ?? "");
                            var plot2 = Utils.CreateBarPlot(dates, bars2, $"Средние показатели:\nкалории: {Math.Round(averageCalories, 2)}\nсердцебиение: {Math.Round(averageHeartRate, 2)}\nкоэфицеиент: {Math.Round(averageCoef, 2)}\nдлительность: {Utils.GetHoursByMin(averageDuration, false)}");
                            plot2.Axes.Title.Label.ForeColor = Colors.Black;
                            plot2.Axes.Title.Label.FontSize = 12;
                            plot2.Axes.Title.Label.Alignment = Alignment.LowerLeft;
                            plot2.Axes.Title.Label.OffsetX = -300;

                            var multiplot = new Multiplot();
                            multiplot.AddPlot(plot1);
                            multiplot.AddPlot(plot2);

                            multiplot.SavePng($"{trainingDay.TrainingDayId}.png", 650, 1100);

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
