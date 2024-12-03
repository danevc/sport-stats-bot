using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace SportStats.Interfaces
{
    public interface IStatistic
    {
        /// <summary>
        /// Статистика по упражнению
        /// </summary>
        /// <param name="text">Порядковый номер упражнения</param>
        void StatsByExercise(string text);

        /// <summary>
        /// Статистика по тренировочному дню
        /// </summary>
        /// <param name="text">Порядковый номер тренировочного дня</param>
        void TrainingDayStats(string text);
    }
}
