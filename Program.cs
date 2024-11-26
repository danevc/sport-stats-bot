using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SportStats.Enums;
using System.Linq;
using SportStats.Controllers;
using SportStats;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types.ReplyMarkups;
using SportStats.Models;

IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("7602969726:AAHvO3euZ71LJJOHY0zgHLp-2YT29fMLpOo", cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;
Console.WriteLine($"@{me.FirstName} started\nPress Enter to terminate");
Console.ReadLine();
cts.Cancel();

async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query })
    {
        switch (query.Data)
        {
            case "AddExercises":
                await bot.EditMessageText(query.Message!.Chat,
                   query.Message.Id,
                   "Напиши название упражнения\n",
                   replyMarkup: null);
                UserStateManager.SetState(query.From.Id, State.AddExercise);
                break;
            case "StopAddExercises":
                await bot.AnswerCallbackQuery(query.Id);
                await bot.SendMessage(query.Message!.Chat.Id,
                   "Отлично! Ты можешь создать расписание.\nЕсли хочешь пропустить нажми /start",
                   replyMarkup: new InlineKeyboardMarkup()
                   .AddButton("Создать расписание", "AddSchedule"));
                UserStateManager.SetState(query.From.Id, State.None);
                break;
            case "AddSchedule":
                await bot.EditMessageText(query.Message!.Chat,
                   query.Message.Id,
                   "Напиши название расписания",
                   replyMarkup: null);
                UserStateManager.SetState(query.From.Id, State.AddSchedule);
                break;
            case "StopAddExercisesToTrainDay":
                await bot.EditMessageText(query.Message!.Chat,
                   query.Message.Id,
                   "Напиши количество полных дней отдыха после этого дня",
                   replyMarkup: null);
                UserStateManager.SetState(query.From.Id, State.AddDayRest);
                break;
            case "StopCreateSchedule":
                await bot.EditMessageText(query.Message!.Chat,
                   query.Message.Id,
                   "/start",
                   replyMarkup: null);
                _cache.Remove($"{query.From.Id}CreateSchedule");
                _cache.Remove($"{query.From.Id}CreateTrainingDay");
                UserStateManager.SetState(query.From.Id, State.None);
                break;
            case "EndCreateSchedule":
                if (_cache.TryGetValue($"{query.From.Id}CreateSchedule", out Schedule schedule))
                {
                    if (schedule == null)
                        throw new Exception("schedule == null");

                    using (var db = new SportContext())
                    {
                        db.Schedules.Add(schedule);
                        db.SaveChanges();
                    }
                    await bot.EditMessageText(query.Message!.Chat,
                       query.Message.Id,
                       "Расписание добавлено. Нажми /start",
                       replyMarkup: null);
                    _cache.Remove($"{query.From.Id}CreateSchedule");
                    _cache.Remove($"{query.From.Id}CreateTrainingDay");
                    UserStateManager.SetState(query.From.Id, State.None);
                }
                break;
            case "DeleteExercise": 
                await bot.AnswerCallbackQuery(query.Id);
                await bot.SendMessage(query.Message!.Chat.Id,
                   "Напиши номера упражнений которые хочешь удалить через запятую",
                   replyMarkup: new InlineKeyboardMarkup()
                   .AddButton("Отменить", "AddExercises"));
                UserStateManager.SetState(query.From.Id, State.RemoveExercise);
                break;
            default:
                break;
        }
    }
}

async Task OnMessage(Message msg, UpdateType type)
{
    using (var db = new SportContext())
    {
        var text = msg.Text;

        var user = db.Users.FirstOrDefault(e => e.UserId == msg.From.Id);
        if (user == null)
        {
            var newUser = new SportStats.Models.User
            {
                UserId = msg.From.Id,
                CreatedOn = DateTime.Now,
                UserName = msg.From.Username,
                FirstName = msg.From.FirstName
            };
            db.Users.Add(newUser);
            db.SaveChanges();

            user = newUser;
        }

        var state = UserStateManager.GetState(user.UserId);

        var stateRouter = new StateRouter(
                new MainController(user, bot, msg.Chat, _cache),
                new WorkoutController(user, bot, msg.Chat, _cache),
                new StatisticController(user, bot, msg.Chat, _cache));

        if (text == Utils._btn_ToHome || text == "/start" || text == "Home")
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