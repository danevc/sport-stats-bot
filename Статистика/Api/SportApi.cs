using Stats.Enums;
using Stats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;

namespace Stats.Api
{
    public static class SportApi
    {
        private static bool addSportSchedulesHistory(MyDayOfWeek dayOfWeek, Models.User user)
        {
            var sportHis = History.GetSportSchedulesHistory();
            if (sportHis == null)
            {
                History.AddSportSchedulesHistory(new SportSchedule
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = dayOfWeek,
                    UserId = user.Id
                });
                return true;
            }
            else if (sportHis.FirstOrDefault(e => e.DayOfWeek == dayOfWeek) == null)
            {
                History.AddSportSchedulesHistory(new SportSchedule
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = dayOfWeek,
                    UserId = user.Id
                });
                return true;
            }
            return false;
        }

        [Obsolete]
        public static async Task<State> Workout(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            
            if (text == Utils._btn_AddExercise)
            {
                var removeKeyboard = new ReplyKeyboardRemove();
                await botClient.SendTextMessageAsync(chat.Id, $"Напиши название упражнения для группы мышц <b>«{History.GetSchedule().FirstOrDefault(e => (int)e.DayOfWeek == History.GetDay()).MuscleGroupName}»</b>", replyMarkup: removeKeyboard, parseMode: ParseMode.Html);
                return State.AddExercise;
            }

            if(text[0] == '▶')
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
            }
            
            return State.Workout;
        }

        [Obsolete]
        public static async Task<State> Stats(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            var muscleGroup = History.GetSchedule()?.FirstOrDefault(e => e.MuscleGroupName == text);

            if (muscleGroup != null)
            {
                History.SetStatMuscleGroupId(muscleGroup.MuscleGroupId);
                var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.StatsByMuscleGroup);
                await botClient.SendTextMessageAsync(chat.Id, "Выбери упражнение.", replyMarkup: replyKeyboard);
                return State.StatsByMuscleGroup;
            }
            return State.Stats;
        }

        [Obsolete]
        public static async Task<State> StatsByMuscleGroup(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            if (text == "По тренировкам")
            {
                var workouts = SqlSport.GetWorkoutInfos(user.Id, History.GetStatMuscleGroupId());
                var plot = Utils.CreateBarWorkoutPlot(workouts, workouts.Select(x => x.MuscleGroupName).First());

                plot.SavePng("По тренировкам.png", 650, 500);

                using (var fileStream = new FileStream("По тренировкам.png", FileMode.Open, FileAccess.Read))
                {
                    var message = await botClient.SendPhotoAsync(chat.Id, photo: InputFile.FromStream(fileStream, "По тренировкам.png"));
                }
                return State.StatsByMuscleGroup;
            }
            else if (text == "По всем упражнениям")
            {
                var exercises = History.GetSchedule()?.FirstOrDefault(e => e.MuscleGroupId == History.GetStatMuscleGroupId())?.Exercises;

                if (exercises != null && exercises.Any())
                {
                    var audit = SqlSport.GetSportAudit(History.GetUserId(), History.GetStatMuscleGroupId());
                    var photoPaths = new List<string>();
                    foreach (var exercise in exercises)
                    {
                        if (audit != null)
                        {
                            var myStat_date = new List<DateTime>();
                            var myStat_weight = new List<int>();
                            var myStat_numOfRepetitions = new List<int>();

                            foreach (var a in audit)
                            {
                                var myExs = a.Exercises.Where(e => e.ExerciseId == exercise.Id);

                                if (myExs != null && myExs.Any())
                                {
                                    var myTopExc = myExs.OrderByDescending(e => e.Weight).ThenByDescending(e => e.NumOfRepetitions).First();
                                    myStat_date.Add(a.WorkoutDate);
                                    myStat_weight.Add(myTopExc.Weight);
                                    myStat_numOfRepetitions.Add(myTopExc.NumOfRepetitions);
                                    photoPaths.Add($"{exercise.Id}.png");
                                }
                            }
                            if (myStat_date.Any())
                            {
                                var plot = Utils.CreateBarPlot(myStat_date, myStat_weight, myStat_numOfRepetitions, exercise.ExerciseName);
                                plot.SavePng($"{exercise.Id}.png", 650, 600);
                            }
                        }
                    }

                    List<IAlbumInputMedia> inputMedia = new List<IAlbumInputMedia>();

                    foreach (string path in photoPaths)
                    {
                        var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                        var memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        inputMedia.Add(new InputMediaPhoto
                        {
                            Media = new InputFileStream(memoryStream, Path.GetFileName(path))
                        });

                        fileStream.Close();
                    }

                    var message = await botClient.SendMediaGroup(chat.Id, inputMedia.ToArray());
                }
                return State.StatsByMuscleGroup;
            }
            else
            {
                var exercise = History.GetSchedule()?.FirstOrDefault(e => e.MuscleGroupId == History.GetStatMuscleGroupId())?.Exercises?.FirstOrDefault(e => e.ExerciseName == text);

                if (exercise != null)
                {
                    var audit = SqlSport.GetSportAudit(History.GetUserId(), History.GetStatMuscleGroupId());

                    if (audit != null)
                    {
                        var myStat_date = new List<DateTime>();
                        var myStat_weight = new List<int>();
                        var myStat_numOfRepetitions = new List<int>();

                        foreach (var a in audit)
                        {
                            var myExs = a.Exercises.Where(e => e.ExerciseId == exercise.Id);

                            if (myExs != null)
                            {
                                var myTopExc = myExs.OrderByDescending(e => e.Weight).ThenByDescending(e => e.NumOfRepetitions).First();
                                myStat_date.Add(a.WorkoutDate);
                                myStat_weight.Add(myTopExc.Weight);
                                myStat_numOfRepetitions.Add(myTopExc.NumOfRepetitions);
                            }
                        }

                        var plot = Utils.CreateBarPlot(myStat_date, myStat_weight, myStat_numOfRepetitions, exercise.ExerciseName);

                        plot.SavePng($"{exercise.Id}.png", 650, 600);

                        using (var fileStream = new FileStream($"{exercise.Id}.png", FileMode.Open, FileAccess.Read))
                        {
                            var message = await botClient.SendPhotoAsync(chat.Id, photo: InputFile.FromStream(fileStream, $"{exercise.Id}.png"));
                        }
                    }
                }
                return State.StatsByMuscleGroup;
            }
        }

        [Obsolete]
        public static async Task<State> RealizeExercise(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {

            if (text == "-")
            {
                var response = "";
                if(History.GetNumApproach() <= 1)
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
            return State.RealizeExercise;
        }

        [Obsolete]
        public static async Task<State> AddExercise(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.AddingExercises);
            if (text == "Добавить ещё")
            {
                var removeKeyboard = new ReplyKeyboardRemove();
                await botClient.SendTextMessageAsync(chat.Id, "Напиши название упражнения", replyMarkup: removeKeyboard);
                return State.AddExercise;
            }
            else if (text == "Закончить")
            {
                var sportSchedule = SqlSport.GetSportSchedule(History.GetUserId());
                History.SetSchedule(sportSchedule);
                replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.StartWithSchedule);
                await botClient.SendTextMessageAsync(chat.Id, "Упражнения сохранены", replyMarkup: replyKeyboard);
                return State.Start;
            }
            else
            {
                var exercise = new Exercise
                {
                    Id = Guid.NewGuid(),
                    ExerciseName = text,
                    MuscleGroupId = History.GetSchedule().FirstOrDefault(e => (int)e.DayOfWeek == History.GetDay()).MuscleGroupId
                };
                SqlSport.AddExercise(exercise);
                var answer = $"Упражнение <b>«{text}»</b> добавлено в тренировку <b>«{History.GetSchedule().FirstOrDefault(e => (int)e.DayOfWeek == History.GetDay()).MuscleGroupName}»</b>";
                await botClient.SendTextMessageAsync(chat.Id, answer, replyMarkup: replyKeyboard, parseMode: ParseMode.Html);
            }
            
            return State.AddExercise;

        }

        [Obsolete]
        public static async Task<State> AddDayOfWeek(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            var isAddWorkout = true;
            switch (text)
            {
                case "Понедельник":
                    isAddWorkout = addSportSchedulesHistory(MyDayOfWeek.Monday, user);
                    break;
                case "Вторник":
                    isAddWorkout = addSportSchedulesHistory(MyDayOfWeek.Tuesday, user);
                    break;
                case "Среда":
                    isAddWorkout = addSportSchedulesHistory(MyDayOfWeek.Wednesday, user);
                    break;
                case "Четверг":
                    isAddWorkout = addSportSchedulesHistory(MyDayOfWeek.Thursday, user);
                    break;
                case "Пятница":
                    isAddWorkout = addSportSchedulesHistory(MyDayOfWeek.Friday, user);
                    break;
                case "Суббота":
                    isAddWorkout = addSportSchedulesHistory(MyDayOfWeek.Saturday, user);
                    break;
                case "Воскресенье":
                    isAddWorkout = addSportSchedulesHistory(MyDayOfWeek.Sunday, user);
                    break;
                case "Закончить":
                    var schedule = PreviewSchedule();
                    var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.OnSaveScheduler);
                    await botClient.SendTextMessageAsync(chat.Id, schedule, replyMarkup: replyKeyboard);
                    return State.OnSaveScheduler;
            }
            if (isAddWorkout)
            {
                var removeKeyboard = new ReplyKeyboardRemove();
                await botClient.SendTextMessageAsync(chat.Id, "Напиши название тренировки", replyMarkup: removeKeyboard);
                return State.CreateSchedule;
            }
            else
            {
                var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.ChooseDayOfWeek);
                await botClient.SendTextMessageAsync(chat.Id, "На этот день уже назначена тренировка", replyMarkup: replyKeyboard);
                return State.AddDayOfWeek;
            }
        }

        [Obsolete]
        public static async Task<State> CreateSchedule(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            var sportSchedule = History.GetSportSchedulesHistory().Last();

            if (sportSchedule != null)
            {
                var muscleGroup = new MuscleGroup
                {
                    Id = Guid.NewGuid(),
                    MuscleGroupName = text,
                    UserId = user.Id
                };
                History.AddMuscleGroupHistory(muscleGroup);
                sportSchedule.MuscleGroupId = muscleGroup.Id;
                History.ChangeSportSchedulesHistory(sportSchedule);
            }

            var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.ChooseDayOfWeek);
            await botClient.SendTextMessageAsync(chat.Id, "Выбери день", replyMarkup: replyKeyboard);
            return State.AddDayOfWeek;
        }

        [Obsolete]
        public static string PreviewSchedule()
        {
            var sportSchedule = History.GetSportSchedulesHistory();
            var muscleGroups = History.GetMuscleGroupHistory();
            string schedule = "";
            if (sportSchedule.Any() && muscleGroups.Any() && sportSchedule.Count() == muscleGroups.Count())
            {
                schedule += $"Ваше расписание:\n\n";
                foreach (var s in sportSchedule)
                {
                    var group = muscleGroups.FirstOrDefault(e => e.Id == s.MuscleGroupId).MuscleGroupName;
                    if (!string.IsNullOrEmpty(group))
                    {
                        schedule += $"{s.DayOfWeek} - {group}\n";
                    }
                }
            }
            else
            {
                schedule = "Возникли ошибки!";
            }
            return schedule;
        }

        [Obsolete]
        public static string ViewSchedule()
        {
            var sportSchedule = SqlSport.GetSportSchedule(History.GetUserId());
            string schedule = "";
            if (sportSchedule.Any())
            {
                schedule += $"Ваше расписание:\n\n";
                foreach (var s in sportSchedule)
                {
                    schedule += $"{Utils.GetDayOfWeekDescription((int)s.DayOfWeek)} - {s.MuscleGroupName}\n";
                }
            }
            else
            {
                schedule = "Возникли ошибки!";
            }
            return schedule;
        }

        [Obsolete]
        public static async Task<State> OnSaveScheduler(string text, ITelegramBotClient botClient, Chat chat, Models.User user)
        {
            if (text == "Сохранить")
            {
                var sportSchedule = History.GetSportSchedulesHistory();
                var muscleGroups = History.GetMuscleGroupHistory();
                foreach (var schedule in sportSchedule)
                {
                    if (schedule != null)
                    {
                        var group = muscleGroups.FirstOrDefault(e => e.Id == schedule.MuscleGroupId);
                        if (group != null)
                        {
                            SqlSport.AddMuscleGroup(group);
                            SqlSport.AddSportSchedule(schedule);
                        }
                    }
                }
            }
            var replyKeyboard = ButtonsKit.ChangeButtons(ButtonGroups.StartWithSchedule);
            await botClient.SendTextMessageAsync(chat.Id, "Расписание сохранено", replyMarkup: replyKeyboard);
            return State.Start;
        }

    }
}
