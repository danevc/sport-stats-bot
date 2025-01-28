using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
    public class WorkoutController : BaseController, IWorkout
    {
        public WorkoutController(Models.User user, ITelegramBotClient bot, Chat chat, IMemoryCache cache, Service service, IConfigurationRoot config) : base(user, bot, chat, cache, service, config) { }

        public async void DoExercise(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    try
                    {
                        (int weight, int numOfRepetitions) = Utils.ParseResApproach(text);

                        if (weight < 0 || numOfRepetitions < 0)
                        {
                            await _bot.SendMessage(_chat.Id, $"Данные введены не верно");
                        }
                        else
                        {
                            var curExercise = CacheHelper.GetCurrentExercise(_cache, _user.UserId);

                            if (curExercise == null)
                                throw new Exception("curExercise == null");

                            var approach = CacheHelper.GetCurrentApproach(_cache, _user.UserId);

                            var exerciseReport = new ExerciseReport
                            {
                                ExerciseReportId = Guid.NewGuid(),
                                CreatedOn = DateTime.Now,
                                ExerciseId = curExercise.ExerciseId,
                                Weight = weight,
                                NumOfRepetitions = numOfRepetitions,
                                Approach = approach
                            };

                            var workout = CacheHelper.GetCreateWorkout(_cache, _user.UserId);

                            if(workout is not null)
                            {
                                workout.ExerciseReports.Add(exerciseReport);
                                CacheHelper.SetCreateWorkout(_cache, _user.UserId, workout);
                            }

                            approach++;

                            using (var db = new SportContext(_config))
                            {
                                var maxCreatedOn = db.ExerciseReports
                                    .Where(e => e.ExerciseId == curExercise.ExerciseId && e.Approach == 1 && e.CreatedOn < DateTime.Today)
                                    .Max(e => e.CreatedOn);

                                var lastEx = db.ExerciseReports
                                    .FirstOrDefault(e => e.ExerciseId == curExercise.ExerciseId && e.Approach == approach && e.CreatedOn.Value.Date == maxCreatedOn.Value.Date);

                                CacheHelper.SetCurrentApproach(_cache, _user.UserId, approach);
                                var message = Utils.GetWorkoutStringMessage(approach, curExercise, lastEx);
                                await _bot.SendMessage(_chat.Id, message,
                                    replyMarkup: new InlineKeyboardMarkup()
                                    .AddButton("Закончить", "EndWriteExerciseReport"),
                                    parseMode: ParseMode.Html);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
                Console.WriteLine(ex.Message);
            }
        }

        public async void ChooseExercise(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if(int.TryParse(text, out int num))
                    {
                        using(var db = new SportContext(_config))
                        {
                            var exercises = CacheHelper.GetTodayExercises(_cache, _user.UserId);

                            if (exercises is null)
                                throw new Exception("ChooseExercise || exercises is null");

                            exercises = exercises.OrderBy(e => e.CreatedOn).ToList();

                            if (num - 1 <= exercises.Count)
                            {
                                var exercise = exercises[num - 1];
                                var workout = CacheHelper.GetCreateWorkout(_cache, _user.UserId);
                                int approach = 1;

                                if (workout is not null)
                                {
                                    var exRep = workout.ExerciseReports.Where(e => e.ExerciseId == exercise.ExerciseId);
                                    var exerciseReportsInDb = db.ExerciseReports
                                        .Where(e => e.ExerciseId == exercise.ExerciseId && e.CreatedOn.Value.Date == DateTime.Now.Date); 

                                    if (exRep.Any())
                                    {
                                        approach = exRep.Max(e => e.Approach) + 1;
                                    }

                                    if (exerciseReportsInDb.Any())
                                    {
                                        approach = exerciseReportsInDb.Max(e => e.Approach) + 1;
                                    }
                                }

                                var maxCreatedOn = db.ExerciseReports
                                    .Where(e => e.ExerciseId == exercise.ExerciseId && e.Approach == 1 && e.CreatedOn < DateTime.Today)
                                    .Max(e => e.CreatedOn);

                                var lastEx = db.ExerciseReports
                                    .FirstOrDefault(e => e.ExerciseId == exercise.ExerciseId && e.Approach == approach && e.CreatedOn.Value.Date == maxCreatedOn.Value.Date);

                                var bestEx = db.ExerciseReports
                                    .Where(e => e.ExerciseId == exercise.ExerciseId)
                                    .OrderByDescending(e => e.Weight)
                                    .ThenByDescending(e => e.NumOfRepetitions).FirstOrDefault();

                                CacheHelper.SetCurrentExercise(_cache, _user.UserId, exercise);
                                CacheHelper.SetCurrentApproach(_cache, _user.UserId, approach);
                                var message = Utils.GetWorkoutStringMessage(approach, exercise, lastEx, bestEx);
                                await _bot.SendMessage(_chat.Id, 
                                    message,
                                    replyMarkup: new InlineKeyboardMarkup()
                                    .AddButton("Закончить", "EndWriteExerciseReport"),
                                    parseMode: ParseMode.Html);
                                UserStateManager.SetState(_user.UserId, State.DoExercise, _cache);
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

        public async void ChooseTrainingDay(string text)
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

                            var workout = new Workout
                            {
                                WorkoutId = Guid.NewGuid(),
                                TrainingDayId = trainingDay.TrainingDayId,
                                UserId = _user.UserId
                            };

                            CacheHelper.SetCreateWorkout(_cache, _user.UserId, workout);

                            var message = $"Тренировка: <b>«{trainingDay.TrainingDayName}»</b>\nУпражнения:\n\n";
                            message += Utils.GetStringExercises(trainingDay.Exercises, CacheHelper.GetDoneExercises(_cache, _user.UserId));
                            message += "\n<b>Напиши номер упражнения</b>";

                            CacheHelper.SetTodayExercises(_cache, _user.UserId, trainingDay.Exercises);
                            await _bot.SendMessage(_chat.Id,
                                message,
                                replyMarkup: new InlineKeyboardMarkup()
                                    .AddButton("Закончить тренировку", "EndWorkout"),
                                parseMode: ParseMode.Html);
                            UserStateManager.SetState(_user.UserId, State.WorkoutChooseExercise, _cache);
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

        public async void AddAverageHeartRate(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (int.TryParse(text, out int num))
                    {
                        if (num > 30 && num < 250)
                        {
                            var workout = CacheHelper.GetCreateWorkout(_cache, _user.UserId);

                            if (workout is not null)
                            {
                                workout.AverageHeartRate = num;
                                CacheHelper.SetCreateWorkout(_cache, _user.UserId, workout);

                                var exercises = CacheHelper.GetTodayExercises(_cache, _user.UserId);
                                var replyMarkup = ButtonsKit.GetBtnsInline(ButtonsInline.AddInfoWorkout, _user.UserId, _cache);

                                if (exercises == null)
                                    throw new Exception("exercises == null");

                                var doneExercises = CacheHelper.GetDoneExercises(_cache, _user.UserId);

                                var message = "Выбери следующее упражнение\n\n";
                                message += Utils.GetStringExercises(exercises, doneExercises);
                                message += "\n<b>Напиши номер упражнения</b>";

                                await _bot.SendMessage(_chat.Id,
                                    message,
                                    replyMarkup: replyMarkup,
                                    parseMode: ParseMode.Html);
                                UserStateManager.SetState(_user.UserId, State.WorkoutChooseExercise, _cache);
                            }
                        }
                        else
                        {
                            await _bot.SendMessage(_chat.Id, $"Значение не валидно");
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

        public async void AddCalories(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (int.TryParse(text, out int num))
                    {
                        if(num > 0 && num < 5000)
                        {
                            var workout = CacheHelper.GetCreateWorkout(_cache, _user.UserId);

                            if (workout is not null)
                            {
                                workout.Calories = num;
                                CacheHelper.SetCreateWorkout(_cache, _user.UserId, workout);

                                var exercises = CacheHelper.GetTodayExercises(_cache, _user.UserId);
                                var replyMarkup = ButtonsKit.GetBtnsInline(ButtonsInline.AddInfoWorkout, _user.UserId, _cache);

                                if (exercises == null)
                                    throw new Exception("exercises == null");

                                var doneExercises = CacheHelper.GetDoneExercises(_cache, _user.UserId);

                                var message = "Выбери следующее упражнение\n\n";
                                message += Utils.GetStringExercises(exercises, doneExercises);
                                message += "\n<b>Напиши номер упражнения</b>";

                                await _bot.SendMessage(_chat.Id,
                                    message,
                                    replyMarkup: replyMarkup,
                                    parseMode: ParseMode.Html);
                                UserStateManager.SetState(_user.UserId, State.WorkoutChooseExercise, _cache);
                            }
                        }
                        else
                        {
                            await _bot.SendMessage(_chat.Id, $"Значение не валидно");
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

        public async void AddDurationWorkout(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (Utils.TryParseTime(text, out int num))
                    {
                        if(num > 0 && num < 600)
                        {
                            var workout = CacheHelper.GetCreateWorkout(_cache, _user.UserId);

                            if (workout is not null)
                            {
                                workout.Duration = num;
                                CacheHelper.SetCreateWorkout(_cache, _user.UserId, workout);

                                var exercises = CacheHelper.GetTodayExercises(_cache, _user.UserId);
                                var replyMarkup = ButtonsKit.GetBtnsInline(ButtonsInline.AddInfoWorkout, _user.UserId, _cache);

                                if (exercises == null)
                                    throw new Exception("exercises == null");

                                var doneExercises = CacheHelper.GetDoneExercises(_cache, _user.UserId);

                                var message = "Выбери следующее упражнение\n\n";
                                message += Utils.GetStringExercises(exercises, doneExercises);
                                message += "\n<b>Напиши номер упражнения</b>";

                                await _bot.SendMessage(_chat.Id,
                                    message,
                                    replyMarkup: replyMarkup,
                                    parseMode: ParseMode.Html);
                                UserStateManager.SetState(_user.UserId, State.WorkoutChooseExercise, _cache);
                            }
                        }
                        else
                        {
                            await _bot.SendMessage(_chat.Id, $"Значение не валидно");
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
