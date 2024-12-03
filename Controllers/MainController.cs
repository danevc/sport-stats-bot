using Microsoft.Extensions.Caching.Memory;
using SportStats.Enums;
using SportStats.Interfaces;
using SportStats.Models;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SportStats.Controllers
{
    public class MainController : BaseController, IMain 
    {
        public MainController(Models.User user, ITelegramBotClient bot, Chat chat, IMemoryCache cache, Service service) : base(user, bot, chat, cache, service) { }

        public async void Start(string text)
        {
            try
            {
                var message = "<b>Привет!</b>✌";
                var keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Start, _user.UserId);
                await _bot.SendMessage(_chat.Id, message, replyMarkup: keyboard, parseMode: ParseMode.Html);
                UserStateManager.SetState(_user.UserId, State.None, _cache);
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
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
                        var result = _service.EditOrCreateExercise(new Exercise
                        {
                            ExerciseId = Guid.NewGuid(),
                            ExerciseName = text.Trim(),
                            UserId = _user.UserId
                        });

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
                            .AddButton("Закончить", "Main"),
                            parseMode: ParseMode.Html);
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
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
                                .AddButton("Отменить создание расписания", "Main"));
                            return;
                        }

                        var schedule = new Schedule
                        {
                            ScheduleId = Guid.NewGuid(),
                            ScheduleName = text,
                            UserId = _user.UserId,
                            TrainingDays = new List<TrainingDay>()
                        };

                        CacheHelper.SetCreateSchedule(_cache, _user.UserId, schedule);

                        await _bot.SendMessage(_chat.Id,
                            "Отлично, теперь напиши название тренировочного дня.\nНапример, «День 1», «День 2»... или по группе мышц «Спина», «Грудь»...\nДалее в эти группы будут добавлены упражнения",
                            replyMarkup: new InlineKeyboardMarkup()
                            .AddButton("Отменить создание расписания", "Main"));
                        UserStateManager.SetState(_user.UserId, State.AddTrainDay, _cache);
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
                Console.WriteLine(ex.Message);
            }
        }

        public async void AddTrainDay(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (_cache.TryGetValue($"{_user.UserId}CreateSchedule", out Schedule? schedule))
                    {
                        if (schedule == null) throw new Exception("schedule == null");

                        var previousTrDay = CacheHelper.GetCreateTrainingDay(_cache, _user.UserId);

                        var sequenceNum = previousTrDay != null ? previousTrDay.SequenceNumber + 1 : 1;

                        var trainingDay = new TrainingDay
                        {
                            TrainingDayId = Guid.NewGuid(),
                            ScheduleId = schedule.ScheduleId,
                            TrainingDayName = text.Trim(),
                            SequenceNumber = sequenceNum
                        };

                        CacheHelper.SetCreateTrainingDay(_cache, _user.UserId, trainingDay);
                        if(sequenceNum == 1)
                        {
                            var message = $"Напиши дату, когда будет/был тренировочный день <b>«{trainingDay.TrainingDayName}»</b>\nПример: 24.03.2024";
                            await _bot.SendMessage(_chat.Id,
                                message,
                                parseMode: ParseMode.Html);
                            UserStateManager.SetState(_user.UserId, State.WriteDateSequenceTrainingDay, _cache);
                            return;
                        }


                        using (var db = new SportContext())
                        {
                            var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).ToList();

                            if (exercises == null) return;

                            var message = $"Теперь добавь упражнения для тренировочного дня <b>«{trainingDay.TrainingDayName}»</b>\n\n";
                            message += Utils.GetStringExercises(exercises);
                            message += $"\n<b>Напиши номера через запятую. Если в списке нет нужного, то добавь новое упражнения начиная со *</b>";

                            await _bot.SendMessage(_chat.Id, 
                                message,
                                replyMarkup: new InlineKeyboardMarkup()
                                .AddButton("Отменить создание расписания", "Main"),
                                parseMode: ParseMode.Html);
                            UserStateManager.SetState(_user.UserId, State.AddExerciseToTrainDay, _cache);
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
                UserStateManager.SetState(_user.UserId, State.None, _cache);
                Console.WriteLine(ex.Message);
            }
        }

        public async void AddExercisesToTrainDay(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var schedule = CacheHelper.GetCreateSchedule(_cache, _user.UserId);

                    if (schedule == null)
                        throw new Exception("schedule == null");

                    var trainingDay = CacheHelper.GetCreateTrainingDay(_cache, _user.UserId);

                    if (trainingDay == null)
                        throw new Exception("trainingDay == null");

                    var message = "";
                    using (var db = new SportContext())
                    {
                        if (text[0] == '*')
                        {
                            text = text.Substring(1).Trim();
                            var exercise = new Exercise
                            {
                                ExerciseId = Guid.NewGuid(),
                                ExerciseName = text,
                                UserId = _user.UserId
                            };

                            trainingDay.Exercises.Add(exercise);

                            message = $"Упражнение {exercise.ExerciseName} добавлено";
                        }
                        else
                        {
                            var splitText = text.Replace(" ", "").Split(',');
                            var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).OrderBy(e => e.CreatedOn).ToList();

                            if (exercises is null)  throw new Exception("AddExercisesToTrainDay || exercises == null");

                            for (int i = 0; i < splitText.Length; i++)
                            {
                                if (int.TryParse(splitText[i], out var num))
                                {
                                    if (num - 1 <= exercises.Count)
                                    {
                                        trainingDay.Exercises.Add(exercises[num - 1]);
                                    }
                                }
                                else
                                {
                                    await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                                    return;
                                }
                            }

                            if (!trainingDay.Exercises.Any())
                            {
                                await _bot.SendMessage(_chat.Id, $"Данные введены некорректно");
                                return;
                            }

                            message = "Упражнения добавлены";
                        }
                    }

                    message += "\nЕсли в списке нет нужного, то добавь новое упражнения начиная со *";

                    _cache.Set($"{_user.UserId}CreateTrainingDay", trainingDay);
                    await _bot.SendMessage(_chat.Id,
                        message,
                        replyMarkup: new InlineKeyboardMarkup()
                            .AddButton("Далее", "NextToAddDayRest")
                            .AddNewRow()
                            .AddButton("Отменить создание расписания", "Main"));
                    UserStateManager.SetState(_user.UserId, State.AddExerciseToTrainDay, _cache);

                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
                Console.WriteLine(ex.Message);
            }
        }

        public async void AddDayRest(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text) && int.TryParse(text, out int result))
                {
                    if (result >= 0)
                    {
                        var schedule = CacheHelper.GetCreateSchedule(_cache, _user.UserId);

                        if (schedule is null)
                            throw new Exception("schedule == null");

                        var trainingDay = CacheHelper.GetCreateTrainingDay(_cache, _user.UserId);

                        if (trainingDay is null)
                            throw new Exception("trainingDay == null");

                        trainingDay.RestDaysAfter = result;
                        schedule.TrainingDays.Add(trainingDay);

                        CacheHelper.SetCreateSchedule(_cache, _user.UserId, schedule);

                        var message = $"Отлично, на данном этапе так выглядит расписание {schedule.ScheduleName}:\n\n";

                        message += Utils.GetStringSchedule(schedule);
                        message += "<b>Напиши название следующего тренировочного дня</b>";

                        await _bot.SendMessage(_chat.Id, message,
                        replyMarkup: new InlineKeyboardMarkup()
                                .AddButton("Сохранить", "EndCreateSchedule")
                                .AddNewRow()
                                .AddButton("Отменить создание расписания", "Main"),
                        parseMode: ParseMode.Html);
                        UserStateManager.SetState(_user.UserId, State.AddTrainDay, _cache);
                    }
                }
                else
                {
                    await _bot.SendMessage(_chat.Id, $"Напиши только число");
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
                Console.WriteLine(ex.Message);
            }
        }

        public async void WriteDateSequenceTrainingDay(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var dateFirstTrainDay = Utils.ParseDate(text);

                    if(dateFirstTrainDay == null)
                    {
                        await _bot.SendMessage(_chat.Id, $"Некорректные данные");
                        return;
                    }

                    var schedule = CacheHelper.GetCreateSchedule(_cache, _user.UserId);
                    if (schedule is null) throw new Exception("schedule == null");

                    var trainingDay = CacheHelper.GetCreateTrainingDay(_cache, _user.UserId);
                    if (trainingDay is null) throw new Exception("trainingDay == null");

                    schedule.DateFirstTrainingDay = dateFirstTrainDay;
                    var message = $"Для первого тренировочного дня установлена дата: {dateFirstTrainDay.Value.Date.ToString("dd MMMM", CultureInfo.CreateSpecificCulture("ru-RU"))}\n";

                    using (var db = new SportContext())
                    {
                        var exercises = db.Exercises.Where(e => e.UserId == _user.UserId).ToList();

                        if (exercises is null) throw new Exception("exercises == null");

                        message += $"Теперь добавь упражнения для тренировочного дня <b>«{trainingDay.TrainingDayName}»</b>\n\n";
                        message += Utils.GetStringExercises(exercises);
                        message += $"\n<b>Напиши номера через запятую. Если в списке нет нужного, то добавь новое упражнения начиная со *</b>";

                        await _bot.SendMessage(_chat.Id,
                            message,
                            replyMarkup: new InlineKeyboardMarkup()
                            .AddButton("Отменить создание расписания", "Main"),
                            parseMode: ParseMode.Html);
                        UserStateManager.SetState(_user.UserId, State.AddExerciseToTrainDay, _cache);
                    }
                }
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None, _cache);
                Console.WriteLine(ex.Message);
            }
        }

        public async void RemoveExercise(string text)
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
                                .AddButton("Закончить", "Main"));
                            UserStateManager.SetState(_user.UserId, State.None, _cache);
                        }
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
