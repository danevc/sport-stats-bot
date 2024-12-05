using Microsoft.Extensions.Caching.Memory;
using SportStats.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SportStats
{
    public static class ButtonsKit
    {
        public static InlineKeyboardMarkup GetBtnsInline(ButtonsInline group, long userId = 0, IMemoryCache? cache = null)
        {
            var inlineKeyboard = new InlineKeyboardMarkup();
            var userHaveExercises = false;
            
            switch (group)
            {
                case ButtonsInline.Start:
                    return inlineKeyboard
                        .AddButton("Тренировка", "Workout")
                        .AddButton("Статистика", "Stats")
                        .AddNewRow()
                        .AddButton("Упражнения", "Exercises")
                        .AddButton("Расписание", "Schedule");

                case ButtonsInline.Workout:
                    return inlineKeyboard
                        .AddButton("По упражнению", "StartWorkoutExercise")
                        .AddButton("По расписанию", "StartWorkoutSchedule")
                        .AddNewRow()
                        .AddButton("Выбрать тренировочный день", "ChooseTrainingDay")
                        .AddNewRow()
                        .AddButton("« Назад", "Back");

                case ButtonsInline.Settings:
                    return inlineKeyboard
                        .AddButton("« Назад", "Back");

                case ButtonsInline.Stats:
                    return inlineKeyboard
                        .AddButton("По упражнениям", "ExerciseStats")
                        .AddButton("По тренировкам", "WorkoutStats")
                        .AddNewRow()
                        .AddButton("По тренировочным дням", "TrainingDayStats")
                        .AddNewRow()
                        .AddButton("« Назад", "Back");

                case ButtonsInline.Schedule:
                    return inlineKeyboard
                        .AddButton("Добавить", "AddSchedule")
                        .AddButton("Изменить", "EditSchedule")
                        .AddButton("Удалить", "RemoveSchedule")
                        .AddNewRow()
                        .AddButton("Назначить основное", "AssignMainSchedule")
                        .AddNewRow()
                        .AddButton("« Назад", "Back");

                case ButtonsInline.Exercises:
                    return inlineKeyboard
                        .AddButton("Добавить", "AddExercises")
                        .AddButton("Изменить", "EditExercise")
                        .AddButton("Удалить", "RemoveExercise")
                        .AddNewRow()
                        .AddButton("« Назад", "Back");

                case ButtonsInline.AddInfoWorkout:
                    if(cache != null)
                    {
                        var workout = CacheHelper.GetCreateWorkout(cache, userId);
                        if (workout != null)
                        {
                            if (workout.AverageHeartRate == 0)
                            {
                                inlineKeyboard
                                .AddButton("Средний пульс", "AddAverageHeartRate");
                            }
                            if (workout.Calories == 0)
                            {
                                inlineKeyboard
                                .AddButton("Калории", "AddCalories");
                            }
                            if (workout.Duration == 0)
                            {
                                inlineKeyboard
                                    .AddNewRow()
                                    .AddButton("Продолжительность тренировки", "AddDurationWorkout");
                            }
                            inlineKeyboard
                                .AddNewRow()
                                .AddButton("Закончить тренировку", "EndWorkout")
                                .AddNewRow()
                                .AddButton("Отменить тренировку", "CancelWorkout");
                        }
                        return inlineKeyboard;
                    }
                    else
                    {
                        throw new Exception("Не передан параметр cache");
                    }

                default:
                    return inlineKeyboard;
            }
        }
    }
}
