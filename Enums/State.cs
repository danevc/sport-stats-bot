using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace SportStats.Enums
{
    public enum State
    {
        None = 0,
        CreateSchedule,
        ChangeSchedule,
        AddDayOfWeek,
        OnSaveScheduler,
        Workout,
        AddExercise,
        ChooseDayToWork,
        DoExercise,
        Stats,
        StatsByMuscleGroup,
        AddMuscleGroup,
        AddStandardMuscleGroup,
        AddSchedule,
        AddTrainDay,
        AddExerciseToTrainDay,
        AddDayRest,
        RemoveExercise,
        WorkoutChooseExercise,
        WorkoutSchedule,
        WriteDateSequenceTrainingDay,
        ChooseTrainingDay,
        StatsChooseExercise,
        AddAverageHeartRate,
        AddCalories,
        AddDurationWorkout,
        TrainingDayStats
    }

    public static class UserStateManager
    {
        private static readonly ConcurrentDictionary<long, State> _userStates = new ConcurrentDictionary<long, State>();

        public static void SetState(long userId, State state, IMemoryCache cache)
        {
            CacheHelper.SetUserState(cache, userId, state);
            _userStates[userId] = state;
        }

        public static State GetState(long userId)
        {
            return _userStates.TryGetValue(userId, out var state) ? state : State.None;
        }

        public static void ClearState(long userId)
        {
            _userStates.TryRemove(userId, out _);
        }
    }
}
