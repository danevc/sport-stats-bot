using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SportStats.Enums;
using SportStats.Controllers;
using SportStats;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types.ReplyMarkups;
using SportStats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;

var _config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var isRelease = _config.GetValue<bool>("isRelease");
var _token = "";

if (isRelease)
    _token = _config["Tokens:TG_TOKEN"];
else
    _token = _config["Tokens:TG_TOKEN_Dev"];

IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
Service _service = new Service();
using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(_token, cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;
Console.WriteLine($"@{me.FirstName} started\nPress Enter to terminate");
Console.ReadLine();
cts.Cancel();

async Task OnUpdate(Telegram.Bot.Types.Update update)
{
    if (update is { CallbackQuery: { } query })
    {
        if (query.Message == null)
            throw new Exception("query.Message == null");

        using (var db = new SportContext(_config))
        {
            var user = db.Users.FirstOrDefault(e => e.UserId == query.From.Id);

            if (user == null)
                return;

            var chatId = query.Message!.Chat.Id;
            var msgId = query.Message.Id;

            InlineKeyboardMarkup? keyboard;
            string message;

            switch (query.Data)
            {
                #region Основные
                case "Main":
                    _cache = new MemoryCache(new MemoryCacheOptions());
                    message = "<b>Привет!</b>✌";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Start);

                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId, message, replyMarkup: keyboard, parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.None, _cache);
                    break;

                case "Back":
                    var keyboardState = CacheHelper.GetKeyboardState(_cache, user.UserId, msgId);
                    message = keyboardState?.MessageText ?? "";
                    keyboard = keyboardState?.KeyboardMarkup;

                    if (string.IsNullOrEmpty(message))
                        return;

                    var state = CacheHelper.GetUserState(_cache, user.UserId) ?? State.None;
                    UserStateManager.SetState(user.UserId, state, _cache);
                    await bot.EditMessageText(chatId, msgId, message, replyMarkup: keyboard);
                    return;

                case "Exercises":
                    message = "Упражнения";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Exercises);
                    await bot.EditMessageText(chatId, msgId, message, replyMarkup: keyboard);
                    break;

                case "Schedule":
                    message = "Расписание";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Schedule);
                    await bot.EditMessageText(chatId, msgId, message, replyMarkup: keyboard);
                    break;

                case "Stats":
                    message = "Статистика";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Stats);
                    await bot.EditMessageText(chatId, msgId, message, replyMarkup: keyboard);
                    break;

                case "Settings":
                    message = "Настройки";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Settings);
                    await bot.EditMessageText(chatId, msgId, message, replyMarkup: keyboard);
                    break;

                case "Workout":
                    message = "Тренировка";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Workout);
                    await bot.EditMessageText(chatId, msgId, message, replyMarkup: keyboard);
                    break;
                #endregion

                #region Тренировки
                case "StartWorkoutExercise":
                    var workout1 = new Workout
                    {
                        WorkoutId = Guid.NewGuid(),
                        UserId = user.UserId
                    };

                    CacheHelper.SetCreateWorkout(_cache, user.UserId, workout1);

                    var exercises = db.Exercises.Where(e => e.UserId == user.UserId).ToList();

                    message = "Твои упражнения:\n\n";
                    message += Utils.GetStringExercises(exercises, CacheHelper.GetDoneExercises(_cache, user.UserId));
                    message += "\n<b>Напиши номер упражнения</b>";

                    CacheHelper.SetTodayExercises(_cache, user.UserId, exercises);
                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                        message,
                        replyMarkup: new InlineKeyboardMarkup()
                                .AddButton("Закончить тренировку", "EndWorkout")
                                .AddButton("Отменить тренировку", "CancelWorkout"),
                        parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.WorkoutChooseExercise, _cache);
                    break;

                case "StartWorkoutSchedule":
                    var userSchedule = db.Schedules
                        .Include(e => e.TrainingDays)
                        .ThenInclude(e => e.Exercises)
                        .FirstOrDefault(e => e.ScheduleId == user.CurrentScheduleId);

                    if (!user.Schedules.Any() || user.CurrentScheduleId is null || userSchedule is null || userSchedule.DateFirstTrainingDay is null)
                    {
                        await bot.EditMessageText(chatId, msgId, "Настрой расписание в разделе <b>«Расписание»</b>",
                            parseMode: ParseMode.Html,
                            replyMarkup: new InlineKeyboardMarkup()
                            .AddButton("« Назад", "Back"));
                        break;
                    }

                    var trainingDay = Utils.GetCurrentTrainDay(userSchedule);

                    if (trainingDay is null)
                    {
                        await bot.AnswerCallbackQuery(query.Id);
                        await bot.EditMessageText(chatId, msgId, $"По расписанию <b>«{userSchedule.ScheduleName}»</b> сегодня нет тренировок",
                        replyMarkup: new InlineKeyboardMarkup()
                        .AddButton("Выбрать тренировочный день", "ChooseTrainingDay")
                        .AddNewRow()
                        .AddButton("« Назад", "Back"),
                        parseMode: ParseMode.Html);
                        break;
                    }
                    else
                    {
                        workout1 = new Workout
                        {
                            WorkoutId = Guid.NewGuid(),
                            TrainingDayId = trainingDay.TrainingDayId,
                            UserId = user.UserId
                        };

                        CacheHelper.SetCreateWorkout(_cache, user.UserId, workout1);

                        message = $"Тренировка: <b>«{trainingDay.TrainingDayName}»</b>\nУпражнения:\n\n";
                        message += Utils.GetStringExercises(trainingDay.Exercises, CacheHelper.GetDoneExercises(_cache, user.UserId));
                        message += "\n<b>Напиши номер упражнения</b>";

                        CacheHelper.SetTodayExercises(_cache, user.UserId, trainingDay.Exercises);
                        await bot.AnswerCallbackQuery(query.Id);
                        await bot.SendMessage(chatId, 
                            message,
                            replyMarkup: new InlineKeyboardMarkup()
                                .AddButton("Закончить тренировку", "EndWorkout")
                                .AddButton("Отменить тренировку", "CancelWorkout"),
                            parseMode: ParseMode.Html);
                        UserStateManager.SetState(user.UserId, State.WorkoutChooseExercise, _cache);
                    }
                    break;

                case "ChooseTrainingDay":
                    userSchedule = db.Schedules
                        .Include(e => e.TrainingDays)
                        .FirstOrDefault(e => e.ScheduleId == user.CurrentScheduleId);

                    if (userSchedule == null)
                        throw new Exception("userSchedule == null");

                    message = $"Тренировочные дни в расписании <b>«{userSchedule.ScheduleName}»</b>\n\n";
                    foreach (var day in userSchedule.TrainingDays.OrderBy(e => e.SequenceNumber))
                    {
                        message += $"<b>{day.SequenceNumber}.</b> {day.TrainingDayName}\n";
                    }
                    message += "\n<b>Напиши номер тренировочного дня</b>";
                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId, message, parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.ChooseTrainingDay, _cache);
                    break;

                case "EndWriteExerciseReport":
                    exercises = CacheHelper.GetTodayExercises(_cache, user.UserId);
                    var curExercise = CacheHelper.GetCurrentExercise(_cache, user.UserId);
                    var replyMarkup = ButtonsKit.GetBtnsInline(ButtonsInline.AddInfoWorkout, user.UserId, _cache);

                    if (exercises == null)
                        throw new Exception("exercises == null");

                    if (curExercise == null)
                        throw new Exception("curExercise == null");

                    CacheHelper.AddDoneExercises(_cache, user.UserId, curExercise);
                    var doneExercises1 = CacheHelper.GetDoneExercises(_cache, user.UserId);

                    message = "Выбери следующее упражнение\n\n";
                    message += Utils.GetStringExercises(exercises, doneExercises1);
                    message += "\n<b>Напиши номер упражнения</b>";

                    CacheHelper.RemoveCurrentExercise(_cache, user.UserId);
                    CacheHelper.RemoveCurrentApproach(_cache, user.UserId);

                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.EditMessageText(chatId,
                        query.Message.Id,
                        message,
                        replyMarkup: replyMarkup,
                        parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.WorkoutChooseExercise, _cache);
                    break;

                case "CancelWorkout":
                    _cache = new MemoryCache(new MemoryCacheOptions());
                    message = "<b>Привет!</b>";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Start);

                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                        message,
                        replyMarkup: keyboard,
                        parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.None, _cache);
                    break;

                case "EndWorkout":
                    var workoutInDb = db.Workouts.Include(e => e.ExerciseReports).FirstOrDefault(e => e.CreatedOn.Date == DateTime.Now.Date);
                    var workout = CacheHelper.GetCreateWorkout(_cache, user.UserId);

                    if(workout is not null && workout.ExerciseReports.Count != 0)
                    {
                        if (workoutInDb == null)
                        {
                            workout.CreatedOn = DateTime.Now;
                            db.Workouts.Add(workout);
                            db.SaveChanges();
                        }
                        else
                        {
                            workoutInDb.AverageHeartRate = (workoutInDb.AverageHeartRate + workout.AverageHeartRate) / 2;
                            workoutInDb.Calories += workout.Calories;
                            workoutInDb.Duration += workout.Duration;
                            workoutInDb.ExerciseReports.AddRange(workout.ExerciseReports);
                            db.SaveChanges();
                        }
                    }
                    _cache = new MemoryCache(new MemoryCacheOptions());
                    message = "<b>Тренировка окончена</b>";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Start);

                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                        message,
                        replyMarkup: keyboard,
                        parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.None, _cache);
                    break;

                case "AddAverageHeartRate":
                    message = "Напиши средний пульс на этой тренировке";
                    await bot.EditMessageText(chatId,
                        msgId,
                        message,
                        replyMarkup: new InlineKeyboardMarkup()
                        .AddButton("« Назад", "Back"));
                    UserStateManager.SetState(user.UserId, State.AddAverageHeartRate, _cache);
                    break;

                case "AddCalories":
                    message = "Напиши количество сожжённых калорий за тренировку";
                    await bot.EditMessageText(chatId,
                        msgId,
                        message,
                        replyMarkup: new InlineKeyboardMarkup()
                        .AddButton("« Назад", "Back"));
                    UserStateManager.SetState(user.UserId, State.AddCalories, _cache);
                    break;

                case "AddDurationWorkout":
                    message = "Напиши продолжительность тренировки.\nПример: 1ч35мин | 35мин";
                    await bot.EditMessageText(chatId,
                        msgId,
                        message,
                        replyMarkup: new InlineKeyboardMarkup()
                        .AddButton("« Назад", "Back"));
                    UserStateManager.SetState(user.UserId, State.AddDurationWorkout, _cache);
                    break;
                #endregion

                #region Статистика
                case "ExerciseStats":
                    exercises = db.Exercises.Where(e => e.UserId == user.UserId).ToList();

                    message = "Твои упражнения:\n\n";
                    message += Utils.GetStringExercises(exercises, CacheHelper.GetDoneExercises(_cache, user.UserId));
                    message += "\n<b>Напиши номер упражнения</b>";

                    CacheHelper.SetTodayExercises(_cache, user.UserId, exercises);
                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                        message,
                        replyMarkup: new InlineKeyboardMarkup()
                        .AddButton("« Назад", "Back"), 
                        parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.StatsChooseExercise, _cache);
                    break;

                case "WorkoutStats":
                    var workouts = db.Workouts
                                .Include(e => e.ExerciseReports)
                                .Where(e => e.UserId == user.UserId)
                                .OrderBy(e => e.CreatedOn).ToList();

                    if (workouts is null)
                    {
                        await bot.AnswerCallbackQuery(query.Id, "Тренировки не найдены");
                        return;
                    }

                    var multiplot = Utils.CreateWorkoutPlot(workouts, "По тренировкам");
                    multiplot.SavePng("По тренировкам.png", 650, 1100);

                    using (var fileStream = new FileStream("По тренировкам.png", FileMode.Open, FileAccess.Read))
                    {
                        await bot.AnswerCallbackQuery(query.Id);
                        await bot.SendPhoto(chatId, photo: Telegram.Bot.Types.InputFile.FromStream(fileStream, "По тренировкам.png"));
                        message = "Результат по тренировкам⬆⬆⬆";
                        keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Start);
                        await bot.SendMessage(chatId, message, replyMarkup: keyboard, parseMode: ParseMode.Html);
                        UserStateManager.SetState(user.UserId, State.None, _cache);
                    }
                    break;

                case "TrainingDayStats":
                    userSchedule = db.Schedules
                         .Include(e => e.TrainingDays)
                         .FirstOrDefault(e => e.ScheduleId == user.CurrentScheduleId);

                    if (userSchedule == null)
                        throw new Exception("userSchedule == null");

                    message = $"Тренировочные дни в расписании <b>«{userSchedule.ScheduleName}»</b>\n\n";
                    foreach (var day in userSchedule.TrainingDays.OrderBy(e => e.SequenceNumber))
                    {
                        message += $"<b>{day.SequenceNumber}.</b> {day.TrainingDayName}\n";
                    }
                    message += "\n<b>Напиши номер тренировочного дня</b>";

                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.EditMessageText(chatId,
                        msgId,
                        message,
                        replyMarkup: new InlineKeyboardMarkup()
                        .AddButton("« Назад", "Back"),
                        parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.TrainingDayStats, _cache);
                    break;
                case "BestScores":
                    message = "";
                    var exercises1 = db.Exercises.ToList();
                    foreach (var ex in exercises1)
                    {
                        var bestEx = db.ExerciseReports
                            .Where(e => e.ExerciseId == ex.ExerciseId)
                            .OrderByDescending(e => e.Weight)
                            .ThenByDescending(e => e.NumOfRepetitions).FirstOrDefault();
                        message += ($"{ex.ExerciseName};{bestEx?.Weight};{bestEx?.NumOfRepetitions};{bestEx?.CreatedOn.Value.ToString("dd MMM yyyy", new CultureInfo("ru-RU"))}\n");
                    }

                    await bot.SendMessage(chatId, message, replyMarkup: new InlineKeyboardMarkup().AddButton("На главную", "Main"), parseMode: ParseMode.Html);
                    break;
                #endregion

                #region Настройки
                case "AssignMainSchedule":
                    await bot.AnswerCallbackQuery(query.Id);
                    message = "Выбери основное расписание:\n";
                    var schedules = db.Schedules.Where(e => e.UserId == user.UserId).OrderBy(e => e.CreatedOn).ToList();

                    for (var i = 0; i < schedules.Count; i++)
                    {
                        message += $"<b>{i + 1}.</b> {schedules[i].ScheduleName}\n";
                    }

                    await bot.SendMessage(chatId,
                      message, 
                      parseMode: ParseMode.Html);
                    UserStateManager.SetState(user.UserId, State.AssignMainSchedule, _cache);
                    break;
                case "AddExercises":
                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                       "Напиши название упражнения");
                    UserStateManager.SetState(user.UserId, State.AddExercise, _cache);
                    break;

                case "AddSchedule":
                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                       "Напиши название расписания");
                    UserStateManager.SetState(user.UserId, State.AddSchedule, _cache);
                    break;

                case "StopAddExercisesToTrainDay":
                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                       "Напиши количество полных дней отдыха после этого дня");
                    UserStateManager.SetState(user.UserId, State.AddDayRest, _cache);
                    break;

                case "NextToAddDayRest":
                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                       "Напиши количество полных дней отдыха после этого тренировочного дня");
                    UserStateManager.SetState(user.UserId, State.AddDayRest, _cache);
                    break;

                case "EndCreateSchedule":
                    await bot.AnswerCallbackQuery(query.Id);

                    var schedule = CacheHelper.GetCreateSchedule(_cache, user.UserId);
                    if (schedule is null) break;

                    var result = _service.EditOrCreateSchedule(schedule, _config);
                    user.CurrentScheduleId = schedule.ScheduleId;
                    db.SaveChanges();

                    message = result.Message ?? "";
                    keyboard = ButtonsKit.GetBtnsInline(ButtonsInline.Start);
                    await bot.SendMessage(chatId, message, replyMarkup: keyboard);

                    CacheHelper.RemoveCreateTrainingDay(_cache, user.UserId);
                    CacheHelper.RemoveCreateSchedule(_cache, user.UserId);
                    UserStateManager.SetState(user.UserId, State.None, _cache);
                    break;

                case "DeleteExercise":
                    await bot.AnswerCallbackQuery(query.Id);
                    await bot.SendMessage(chatId,
                       "Напиши номера упражнений которые хочешь удалить через запятую",
                       replyMarkup: new InlineKeyboardMarkup()
                       .AddButton("Отменить", "AddExercises"));
                    UserStateManager.SetState(user.UserId, State.RemoveExercise, _cache);
                    break;
                #endregion

                default:
                    await bot.SendMessage(chatId, "Неизвестная кнопка!");
                    break;
            }
            CacheHelper.SetKeyboardState(_cache, user.UserId, msgId, query.Message.ReplyMarkup, query.Message.Text);
        }
    }
}

async Task OnMessage(Message msg, UpdateType type)
{

    using (var db = new SportContext(_config))
    {
        var text = msg.Text ?? "";

        if (msg.From == null)
            throw new Exception("msg.from == null");

        var user = db.Users.FirstOrDefault(e => e.UserId == msg.From.Id);
        if (user == null)
        {
            var newUser = new SportStats.Models.User
            {
                UserId = msg.From.Id,
                Username = msg.From.Username,
                FirstName = msg.From.FirstName
            };
            db.Users.Add(newUser);
            db.SaveChanges();

            user = db.Users.FirstOrDefault(e => e.UserId == msg.From.Id);
        }

        var state = UserStateManager.GetState(user.UserId);

        var stateRouter = new StateRouter(
                new MainController(user, bot, msg.Chat, _cache, _service, _config),
                new WorkoutController(user, bot, msg.Chat, _cache, _service, _config),
                new StatisticController(user, bot, msg.Chat, _cache, _service, _config));

        if (text == "/start" || text == "Home")
        {
            stateRouter.Route(State.None, text);
        }
        else
        {
            stateRouter.Route(state, text);
        }
    }
}

async Task OnError(Exception ex, HandleErrorSource source)
{
    Console.WriteLine(ex.Message);
}