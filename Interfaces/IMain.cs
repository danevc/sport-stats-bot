using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace SportStats.Interfaces
{
    public interface IMain
    {
        void Start(string text);

        void AddExercise(string text);

        void AddSchedule(string text);

        void AddTrainDay(string text);

        void AddExercisesToTrainDay(string text);

        void AddDayRest(string text);

        void RemoveExercise(string text);
    }
}
