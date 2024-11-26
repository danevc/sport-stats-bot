using System.Collections.Concurrent;

namespace SportStats.Enums
{
    public enum State
    {
        None,
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
        Start,
        AddSchedule,
        AddTrainDay,
        AddExerciseToTrainDay,
        AddDayRest,
        RemoveExercise
    }

    public static class UserStateManager
    {
        private static readonly ConcurrentDictionary<long, State> _userStates = new ConcurrentDictionary<long, State>();

        public static void SetState(long userId, State state)
        {
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
