using Microsoft.Extensions.Caching.Memory;
using SportStats.Enums;
using SportStats.Interfaces;
using SportStats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SportStats.Controllers
{
    public class MainController : BaseController, IMain 
    {
        public MainController(Models.User user, ITelegramBotClient bot, Chat chat, IMemoryCache cache) : base(user, bot, chat, cache)
        {

        }

        public async void Start(string text)
        {
            try
            {
                using (var db = new SportContext())
                {
                    if (db.Exercises.Any(e => e.UserId == _user.UserId))
                    {
                        var replyKeyboard = ButtonsKit.GetBtnsReply(ButtonsReply.None);
                        await _bot.SendMessage(_chat.Id, "<b>Привет!</b>✌",
                            replyMarkup: replyKeyboard,
                            parseMode: ParseMode.Html);
                        UserStateManager.SetState(_user.UserId, State.None);
                    }
                    else
                    {
                        var replyKeyboard = ButtonsKit.GetBtnsReply(ButtonsReply.None);
                        await _bot.SendMessage(_chat.Id, "<b>Привет!</b>✌\nДля работы с ботом необходимо <b>добавить упражнения</b>",
                            replyMarkup: new InlineKeyboardMarkup().AddButton("Добавить", "AddExercises"),
                            parseMode: ParseMode.Html);
                        UserStateManager.SetState(_user.UserId, State.Start);
                    }
                }

                if (text == Utils._btn_Workout)
                {
                    using (var db = new SportContext())
                    {
                        await _bot.SendMessage(_chat.Id, "Группа мышц",
                            replyMarkup: ButtonsKit.GetBtnsInline(ButtonsInline.Workout, _user.UserId));
                        UserStateManager.SetState(_user.UserId, State.Workout);
                    }
                }
                else if (text == Utils._btn_Stats)
                {
                    await _bot.SendMessage(_chat.Id, "<b>Выбери ...</b>",
                        replyMarkup: new InlineKeyboardMarkup().AddButton("Закончить", "StopAddExercises"),
                        parseMode: ParseMode.Html);
                    UserStateManager.SetState(_user.UserId, State.Stats);
                }
                else
                {
                    
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }


        public async void AddExercise(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    using (var db = new SportContext())
                    {
                        if (db.Exercises.Any(e => e.UserId == _user.UserId && e.ExerciseName == text))
                        {
                            var replyKeyboard = ButtonsKit.GetBtnsReply(ButtonsReply.None);
                            await _bot.SendMessage(_chat.Id, "Такое упражнение уже есть, напиши другое",
                                replyMarkup: replyKeyboard);
                        }
                        else
                        {
                            db.Exercises.Add(new Exercise
                            {
                                ExerciseId = Guid.NewGuid(),
                                CreatedOn = DateTime.Now,
                                UserId = _user.UserId,
                                ExerciseName = text.Trim()
                            });
                            db.SaveChanges();
                            var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).OrderBy(e => e.CreatedOn).Select(e => e.ExerciseName).ToList();
                            var message = "Твои упражнения:\n\n";

                            for (var i = 0; i < exercises.Count(); i++)
                            {
                                message += $"<b>{i + 1}.</b> {exercises[i]}\n";
                            }

                            message += "\n<b>Напиши следующее</b>";

                            await _bot.SendMessage(_chat.Id, message,
                                replyMarkup: new InlineKeyboardMarkup()
                                .AddButton("Удалить", "DeleteExercise")
                                .AddButton("Закончить", "StopAddExercises"),
                                parseMode: ParseMode.Html);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }

        public async void AddSchedule(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    using (var db = new SportContext())
                    {
                        if (db.Schedules.Any(e => e.UserId == _user.UserId && e.ScheduleName == text))
                        {
                            await _bot.SendMessage(_chat.Id, "Такое расписание уже есть, напиши другое название",
                                replyMarkup: new InlineKeyboardMarkup()
                                        .AddButton("Отменить создание расписания", "StopCreateSchedule"));
                        }
                        else
                        {
                            var schedule = new Schedule
                            {
                                ScheduleId = Guid.NewGuid(),
                                CreatedOn = DateTime.Now,
                                ScheduleName = text,
                                UserId = _user.UserId,
                                TrainingDays = new List<TrainingDay>()
                            };

                            db.Schedules.Add(schedule);
                            db.SaveChanges();

                            _cache.Set($"{_user.UserId}CreateSchedule", schedule);

                            await _bot.SendMessage(_chat.Id, "Отлично, теперь напиши название тренировочного дня.\nНапример, «День 1», «День 2»... или по группе мышц «Спина», «Грудь»...\nДалее в эти группы будут добавлены упражнения",
                                replyMarkup: new InlineKeyboardMarkup()
                                        .AddButton("Отменить создание расписания", "StopCreateSchedule"));
                            UserStateManager.SetState(_user.UserId, State.AddTrainDay);
                        }
                    }
                }
                else
                {
                    await _bot.SendMessage(_chat.Id, "В названии допускаются только символы алфавита");
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }

        public async void AddTrainDay(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (_cache.TryGetValue($"{_user.UserId}CreateSchedule", out Schedule schedule))
                    {
                        if (schedule == null)
                            throw new Exception("schedule == null");

                        var trainingDay = new TrainingDay
                        {
                            TrainingDayId = Guid.NewGuid(),
                            CreatedOn = DateTime.Now,
                            ScheduleId = schedule.ScheduleId,
                            TrainingDayName = text.Trim()
                        };

                        using(var db = new SportContext())
                        {
                            var scheduleInDb = db.Schedules.FirstOrDefault(e => e.ScheduleId == schedule.ScheduleId);
                            scheduleInDb.TrainingDays.Add(trainingDay);
                            db.SaveChanges();
                        }

                        _cache.Set($"{_user.UserId}CreateTrainingDay", trainingDay);
                        
                        using (var db = new SportContext())
                        {
                            var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).OrderBy(e => e.CreatedOn).Select(e => e.ExerciseName).ToList();
                            var message = $"Теперь добавь упражнения для тренировочного дня <b>«{text}»</b>\n\n";

                            for (var i = 0; i < exercises.Count(); i++)
                            {
                                message += $"<b>{i + 1}.</b> {exercises[i]}\n";
                            }
                            message += $"\n<b>Напиши номера через запятую</b>";
                            await _bot.SendMessage(_chat.Id, message,
                                replyMarkup: new InlineKeyboardMarkup()
                                        .AddButton("Отменить создание расписания", "StopCreateSchedule"),
                                parseMode: ParseMode.Html);
                            UserStateManager.SetState(_user.UserId, State.AddExerciseToTrainDay);
                        }
                    }
                    else
                    {
                        throw new Exception("Расписание не найдено в кэше");
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }

        public async void AddExercisesToTrainDay(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (_cache.TryGetValue($"{_user.UserId}CreateSchedule", out Schedule schedule))
                    {
                        if (schedule == null)
                            throw new Exception("schedule == null");

                        if (_cache.TryGetValue($"{_user.UserId}CreateTrainingDay", out TrainingDay trainingDay))
                        {
                            if(trainingDay == null)
                                throw new Exception("trainingDay == null");

                            using (var db = new SportContext())
                            {
                                var splitText = text.Replace(" ", "").Split(',');
                                var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).OrderBy(e => e.CreatedOn).ToList();

                                if (exercises == null)
                                    throw new Exception("exercises == null");

                                for (int i = 0; i < splitText.Length; i++)
                                {
                                    if(int.TryParse(splitText[i], out var num))
                                    {
                                        if(num - 1 <= exercises.Count())
                                        {
                                            var trainingDayInDb = db.TrainingDays.FirstOrDefault(e => e.TrainingDayId == trainingDay.TrainingDayId);
                                            trainingDay.Exercises.Add(new ExerciseTrainingDay
                                            {
                                                TrainingDay = trainingDay,
                                                ExerciseId = exercises[num - 1].ExerciseId,
                                                TrainingDayId = trainingDay.TrainingDayId,
                                                Exercise = exercises[num - 1]
                                            });
                                            db.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                                        return;
                                    }
                                }

                                if(!trainingDay.Exercises.Any())
                                {
                                    await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                                    return;
                                }

                                _cache.Set($"{_user.UserId}CreateTrainingDay", trainingDay);
                                await _bot.SendMessage(_chat.Id,
                                   "Отлично! Теперь напиши количество полных дней отдыха после этого дня");
                                UserStateManager.SetState(_user.UserId, State.AddDayRest);
                            }
                        }
                        else
                        {
                            throw new Exception("Тренировочный день не найдено в кэше");
                        }
                    }
                    else
                    {
                        throw new Exception("Расписание не найдено в кэше");
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }

        public async void AddDayRest(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text) && int.TryParse(text, out int result))
                {
                    if(result >= 0)
                    {
                        if (_cache.TryGetValue($"{_user.UserId}CreateSchedule", out Schedule schedule))
                        {
                            if (schedule == null)
                                throw new Exception("schedule == null");

                            if (_cache.TryGetValue($"{_user.UserId}CreateTrainingDay", out TrainingDay trainingDay))
                            {
                                if (trainingDay == null)
                                    throw new Exception("trainingDay == null");

                                trainingDay.RestDaysAfter = result;
                                schedule.TrainingDays.Add(trainingDay);
                                _cache.Set($"{_user.UserId}CreateSchedule", schedule);
                                _cache.Remove($"{_user.UserId}CreateTrainingDay");

                                var message = $"Отлично, на данном этапе так выглядит расписание {schedule.ScheduleName}:\n\n";

                                foreach (var day in schedule.TrainingDays)
                                {
                                    message += $"<u><b>Тренировочный день: «{day.TrainingDayName}»</b></u>\n\n";
                                    foreach (var exercise in day.Exercises)
                                    {
                                        message += $"{exercise.Exercise.ExerciseName}\n";
                                    }
                                    message += "\n";
                                }
                                message += "<b>Напиши название следующего тренировочного дня</b>";

                                await _bot.SendMessage(_chat.Id, message,
                                replyMarkup: new InlineKeyboardMarkup()
                                        .AddButton("Сохранить", "EndCreateSchedule")
                                        .AddButton("Отменить создание расписания", "StopCreateSchedule"),
                                parseMode: ParseMode.Html);
                                UserStateManager.SetState(_user.UserId, State.AddTrainDay);
                            }
                        }
                        else
                        {
                            throw new Exception("Расписание не найдено в кэше");
                        }
                    }
                    else
                    {
                        await _bot.SendMessage(_chat.Id, $"Число должно быть больше нуля, либо равно нулю");
                    }
                }
                else
                {
                    await _bot.SendMessage(_chat.Id, $"Напиши только число");
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }

        }

        public async void RemoveExercise(string text)
        {
            try
            {
                try
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        using (var db = new SportContext())
                        {
                            var splitText = text.Replace(" ", "").Split(',');
                            var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).OrderBy(e => e.CreatedOn).ToList();
                            var removedExercises = new List<string>();


                            if (exercises == null)
                                throw new Exception("exercises == null");

                            for (int i = 0; i < splitText.Length; i++)
                            {
                                if (int.TryParse(splitText[i], out var num))
                                {
                                    if (num - 1 <= exercises.Count())
                                    {
                                        db.Exercises.Remove(exercises[num - 1]);
                                        removedExercises.Add(exercises[num - 1].ExerciseName);
                                    }
                                }
                                else
                                {
                                    await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                                    return;
                                }
                            }

                            db.SaveChanges();

                            if (!removedExercises.Any())
                            {
                                await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                                return;
                            }
                            else
                            {
                                var message = "Удаленные упражнения:\n\n";
                                foreach (var exercise in removedExercises)
                                {
                                    message += $"{exercise}\n";
                                }
                                await _bot.SendMessage(_chat.Id, message,
                                    replyMarkup: new InlineKeyboardMarkup()
                                    .AddButton("Добавить упражнения", "AddExercises")
                                    .AddButton("На главную", "ToHome"));
                                UserStateManager.SetState(_user.UserId, State.None);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    UserStateManager.SetState(_user.UserId, State.None);
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
