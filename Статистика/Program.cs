using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Stats.Sql;
using Stats.Enums;

namespace Stats
{
    class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;
        private static State _state = State.Start;

        [Obsolete]
        static async Task Main()
        {
            _botClient = new TelegramBotClient("7602969726:AAHvO3euZ71LJJOHY0zgHLp-2YT29fMLpOo");
            _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message
                }
            };

            using (var cts = new CancellationTokenSource())
            {
                _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

                var me = await _botClient.GetMeAsync();
                Console.WriteLine($"{me.FirstName} запущен!");

                await Task.Delay(-1);
            };
        }

        [Obsolete]
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var message = update.Message;
                            var user = message.From;
                            var chat = message.Chat;

                            var userInDb = SqlHelper.GetUser(user.Id);

                            if (userInDb == null)
                            {
                                var newUser = new Models.User
                                {
                                    Id = (int)user.Id,
                                    UserName = user.Username,
                                    FirstName = user.FirstName
                                };
                                SqlHelper.AddUser(newUser);

                                userInDb = newUser;
                                
                            }
                            History.SetUserId(userInDb.Id);
                            History.SetDay();
                            
                            switch (message.Type)
                            {
                                case MessageType.Text:
                                    _state = await Handlers.TextHandler(message.Text, _state, botClient, chat, userInDb);
                                    break;

                                default:
                                    {
                                        await botClient.SendTextMessageAsync(chat.Id, "Используй только текст!");
                                        return;
                                    }
                            }
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                _state = State.Start;
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            Console.WriteLine(error);
            _state = State.Start;
            return Task.CompletedTask;
        }

    }


}
