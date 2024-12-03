using SportStats.Controllers;
using SportStats.Enums;

namespace SportStats
{
    public class StateRouter
    {
        private readonly Dictionary<State, Action<string>> _stateHandlers;

        public StateRouter(MainController mainController, WorkoutController workoutController, StatisticController statisticController)
        {
            _stateHandlers = new Dictionary<State, Action<string>>
        {
            { State.None, mainController.Start },
            { State.AddExercise, mainController.AddExercise },
            { State.RemoveExercise, mainController.RemoveExercise },
            { State.AddSchedule, mainController.AddSchedule },
            { State.AddTrainDay, mainController.AddTrainDay },
            { State.AddExerciseToTrainDay, mainController.AddExercisesToTrainDay },
            { State.AddDayRest, mainController.AddDayRest },
            { State.WriteDateSequenceTrainingDay, mainController.WriteDateSequenceTrainingDay },
            { State.AddAverageHeartRate, workoutController.AddAverageHeartRate },
            { State.AddCalories, workoutController.AddCalories },
            { State.AddDurationWorkout, workoutController.AddDurationWorkout },
            { State.WorkoutChooseExercise, workoutController.ChooseExercise },
            { State.ChooseTrainingDay, workoutController.ChooseTrainingDay },
            { State.DoExercise, workoutController.DoExercise },
            { State.StatsChooseExercise, statisticController.StatsByExercise },
            { State.TrainingDayStats, statisticController.TrainingDayStats },
        };
        }

        public void Route(State state, string text)
        {
            if (_stateHandlers.TryGetValue(state, out var handler))
            {
                handler(text);
            }
            else
            {
                throw new Exception($"Неизвестное состояние: {state}");
            }
        }
    }
}
