using Microsoft.Extensions.Caching.Memory;
using SportStats.Enums;
using SportStats.Interfaces;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SportStats.Controllers
{
    public class WorkoutController : BaseController, IWorkout
    {
        public WorkoutController(Models.User user, ITelegramBotClient bot, Chat chat, IMemoryCache cache) : base(user, bot, chat, cache)
        {

        }

        public void DoExercise(string text)
        {
            try
            {
                /**if (text == "-")
                {
                    var response = "";
                    if (History.GetNumApproach() <= 1)
                    {
                        response += "Выбери упражнение";
                    }
                    else
                    {
                        response += "Результаты сохранены.\nВыбери следующее упражнение";
                        if (History.GetCurrentExercise() != new Exercise())
                        {
                            History.AddDoneExercises(History.GetCurrentExercise().Id);
                        }
                    }
                    History.SetCurrentExercise(new Exercise());
                    History.SetNumApproach(1);
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.Workout);
                    await botClient.SendTextMessageAsync(chat.Id, response, replyMarkup: replyKeyboard);
                    return State.Workout;
                }
                else if (!string.IsNullOrEmpty(text))
                {
                    try
                    {
                        (int weight, int numOfRepetitions) = Utils.ParseResApproach(text);

                        if (weight < 0 || numOfRepetitions < 0)
                        {
                            await botClient.SendTextMessageAsync(chat.Id, $"Данные введены не верно");
                        }
                        else
                        {
                            var numApproach = History.GetNumApproach();

                            var sportAudit = new SportAudit
                            {
                                Id = Guid.NewGuid(),
                                ExerciseId = History.GetCurrentExercise().Id,
                                MuscleGroupId = History.GetSchedule().FirstOrDefault(e => (int)e.DayOfWeek == History.GetDay()).MuscleGroupId,
                                UserId = user.Id,
                                WorkoutDate = DateTime.Now,
                                Weight = weight,
                                NumOfRepetitions = numOfRepetitions,
                                Approach = numApproach
                            };

                            SqlSport.AddSportAudit(sportAudit);
                            numApproach++;
                            var lastEx = History.GetAuditLastWorkout()?.Exercises?.FirstOrDefault(e => e.ExerciseId == History.GetCurrentExercise()?.Id && e.Approach == numApproach);
                            History.SetNumApproach(numApproach);
                            await botClient.SendTextMessageAsync(chat.Id, Utils.GetWorkoutStringResponse(numApproach, lastEx), parseMode: ParseMode.Html);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                */
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }

        public async void Workout(string text)
        {
            try
            {
                /**if (text == Utils._btn_AddExercise)
                {
                    var removeKeyboard = new ReplyKeyboardRemove();
                    await botClient.SendTextMessageAsync(chat.Id, $"Напиши название упражнения для группы мышц <b>«{History.GetSchedule().FirstOrDefault(e => (int)e.DayOfWeek == History.GetDay()).MuscleGroupName}»</b>", replyMarkup: removeKeyboard, parseMode: ParseMode.Html);
                    return State.AddExercise;
                }

                if (text[0] == '▶')
                {
                    text = text.Substring(2);
                    var exercise = History.GetSchedule()?.FirstOrDefault(e => (int)e.DayOfWeek == History.GetDay())?.Exercises?.FirstOrDefault(e => e.ExerciseName == text);

                    if (exercise != null)
                    {
                        var sportAudits = SqlSport.GetSportAudit(user.Id, exercise.MuscleGroupId);

                        if (sportAudits.Any())
                        {
                            var maxDate = sportAudits.Max(x => x.WorkoutDate).Date;
                            var latestSportAudits = sportAudits.Where(x => x.WorkoutDate.Date == maxDate).ToList().First();
                            History.SetAuditLastWorkout(latestSportAudits);
                        }

                        History.SetCurrentExercise(exercise);

                        var removeKeyboard = new ReplyKeyboardRemove();
                        var numApproach = History.GetNumApproach();
                        var lastEx = History.GetAuditLastWorkout()?.Exercises?.FirstOrDefault(e => e.ExerciseId == History.GetCurrentExercise()?.Id && e.Approach == numApproach);
                        await botClient.SendTextMessageAsync(chat.Id, Utils.GetWorkoutStringResponse(numApproach, lastEx), parseMode: ParseMode.Html, replyMarkup: removeKeyboard);
                        return State.RealizeExercise;
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Ты уже выполнил это упражнение");
                    return State.Workout;
                }*/
            }
            catch (Exception ex)
            {
                UserStateManager.SetState(_user.UserId, State.None);
                Console.WriteLine(ex.Message);
            }
        }
    }
}
