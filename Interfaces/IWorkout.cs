using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace SportStats.Interfaces
{
    public interface IWorkout
    {
        void Workout(string text);

        void DoExercise(string text);
    }
}
