using SportStats.Controllers;
using SportStats.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

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
            { State.Workout, workoutController.Workout },
            { State.DoExercise, workoutController.DoExercise },
            { State.Stats, statisticController.Stats },
            { State.StatsByMuscleGroup, statisticController.StatsByMuscleGroup }
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
