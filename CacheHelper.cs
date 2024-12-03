using Microsoft.Extensions.Caching.Memory;
using SportStats.Enums;
using SportStats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace SportStats
{
    public static class CacheHelper
    {
        #region KeyboardState
        public static void SetKeyboardState(IMemoryCache cache, long userId, long messageId, InlineKeyboardMarkup? keyboard, string message)
        {
            var cacheKey = $"{userId}_{messageId}";
            var keyboardHistory = cache.Get<Stack<KeyboardState>>(cacheKey) ?? new Stack<KeyboardState>();
            keyboardHistory.Push(new KeyboardState
            {
                MessageText = message,
                KeyboardMarkup = keyboard
            });
            cache.Set(cacheKey, keyboardHistory);
        }

        public static KeyboardState? GetKeyboardState(IMemoryCache cache, long userId, long messageId)
        {
            string cacheKey = $"{userId}_{messageId}";
            var keyboardHistory = cache.Get<Stack<KeyboardState>>(cacheKey);
            if (keyboardHistory == null || keyboardHistory.Count == 0)
            {
                return null;
            }
            var keyboardState = keyboardHistory.Pop();
            cache.Set(cacheKey, keyboardHistory);
            return keyboardState;
        }
        #endregion

        #region UserState
        public static void SetUserState(IMemoryCache cache, long userId, State state)
        {
            var cacheKey = $"{userId}UserState";
            var states = cache.Get<Stack<State>>(cacheKey) ?? new Stack<State>();
            states.Push(state);
            cache.Set(cacheKey, states);
        }

        public static State? GetUserState(IMemoryCache cache, long userId)
        {
            string cacheKey = $"{userId}UserState";
            var states = cache.Get<Stack<State>>(cacheKey);
            if (states == null || states.Count == 0)
            {
                return null;
            }
            var state = states.Pop();
            cache.Set(cacheKey, states);
            return state;
        }
        #endregion

        #region CreateSchedule
        public static void SetCreateSchedule(IMemoryCache cache, long userId, Schedule schedule)
        {
            var cacheKey = $"{userId}CreateSchedule";
            cache.Set(cacheKey, schedule);
        }

        public static Schedule? GetCreateSchedule(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CreateSchedule";
            return cache.Get(cacheKey) as Schedule;
        }

        public static void RemoveCreateSchedule(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CreateSchedule";
            cache.Remove(cacheKey);
        }
        #endregion

        #region CreateTrainingDay
        public static void SetCreateTrainingDay(IMemoryCache cache, long userId, TrainingDay trainingDay)
        {
            var cacheKey = $"{userId}CreateTrainingDay";
            cache.Set(cacheKey, trainingDay);
        }

        public static TrainingDay? GetCreateTrainingDay(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CreateTrainingDay";
            return cache.Get(cacheKey) as TrainingDay;
        }

        public static void RemoveCreateTrainingDay(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CreateTrainingDay";
            cache.Remove(cacheKey);
        }
        #endregion

        #region CurrentExercise
        public static void SetCurrentExercise(IMemoryCache cache, long userId, Exercise exercise)
        {
            var cacheKey = $"{userId}CurrentExercise";
            cache.Set(cacheKey, exercise);
        }

        public static Exercise? GetCurrentExercise(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CurrentExercise";
            return cache.Get(cacheKey) as Exercise;
        }

        public static void RemoveCurrentExercise(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CurrentExercise";
            cache.Remove(cacheKey);
        }
        #endregion

        #region CurrentApproach
        public static void SetCurrentApproach(IMemoryCache cache, long userId, int approach)
        {
            var cacheKey = $"{userId}CurrentApproach";
            cache.Set(cacheKey, approach);
        }

        public static int GetCurrentApproach(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CurrentApproach";
            return (int)cache.Get(cacheKey);
        }

        public static void RemoveCurrentApproach(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CurrentApproach";
            cache.Remove(cacheKey);
        }
        #endregion

        #region ExerciseReport
        public static void AddExerciseReport(IMemoryCache cache, long userId, ExerciseReport exReport)
        {
            var cacheKey = $"{userId}ExerciseReport";
            var exerciseReportList = cache.Get<Stack<ExerciseReport>>(cacheKey) ?? new Stack<ExerciseReport>();
            exerciseReportList.Push(exReport);
            cache.Set(cacheKey, exerciseReportList);
        }

        public static Stack<ExerciseReport>? GetExerciseReport(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}ExerciseReport";
            return cache.Get(cacheKey) as Stack<ExerciseReport>;
        }

        public static void RemoveExerciseReport(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}ExerciseReport";
            cache.Remove(cacheKey);
        }
        #endregion

        #region CreateWorkout
        public static void SetCreateWorkout(IMemoryCache cache, long userId, Workout workout)
        {
            var cacheKey = $"{userId}CreateWorkout";
            cache.Set(cacheKey, workout);
        }

        public static Workout? GetCreateWorkout(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CreateWorkout";
            return cache.Get(cacheKey) as Workout;
        }

        public static void RemoveCreateWorkout(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CreateWorkout";
            cache.Remove(cacheKey);
        }
        #endregion

        #region DoneExercises
        public static void AddDoneExercises(IMemoryCache cache, long userId, Exercise exercise)
        {
            var cacheKey = $"{userId}DoneExercises";
            var doneExercises = cache.Get<List<Exercise>>(cacheKey) ?? new List<Exercise>();
            doneExercises.Add(exercise);
            cache.Set(cacheKey, doneExercises);
        }

        public static List<Exercise>? GetDoneExercises(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}DoneExercises";
            return cache.Get(cacheKey) as List<Exercise>;
        }

        public static void RemoveDoneExercises(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}DoneExercises";
            cache.Remove(cacheKey);
        }
        #endregion

        #region TodayExercises
        public static void SetTodayExercises(IMemoryCache cache, long userId, List<Exercise> exercises)
        {
            var cacheKey = $"{userId}TodayExercises";
            cache.Set(cacheKey, exercises);
        }

        public static List<Exercise>? GetTodayExercises(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}TodayExercises";
            return cache.Get(cacheKey) as List<Exercise>;
        }

        public static void RemoveTodayExercises(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}TodayExercises";
            cache.Remove(cacheKey);
        }
        #endregion

        #region CurrentTrainingDay
        public static void SetCurrentTrainingDay(IMemoryCache cache, long userId, TrainingDay trainingDay)
        {
            var cacheKey = $"{userId}CurrentTrainingDay";
            cache.Set(cacheKey, trainingDay);
        }

        public static TrainingDay? GetCurrentTrainingDay(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CurrentTrainingDay";
            return cache.Get(cacheKey) as TrainingDay;
        }

        public static void RemoveCurrentTrainingDay(IMemoryCache cache, long userId)
        {
            var cacheKey = $"{userId}CurrentTrainingDay";
            cache.Remove(cacheKey);
        }
        #endregion
    }
}
