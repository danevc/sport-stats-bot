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
        /// <summary>
        /// Результат выполнения упражнения
        /// </summary>
        /// <param name="text">Информация о выполнении упражнения</param>
        void DoExercise(string text);

        /// <summary>
        /// Выбрать упражнение для выполнения
        /// </summary>
        /// <param name="text">Порядковый номер упражнения</param>
        void ChooseExercise(string text);

        /// <summary>
        /// Выбрать тренировочный день для выполнения
        /// </summary>
        /// <param name="text">Порядковый номер тренировочного дня</param>
        void ChooseTrainingDay(string text);

        /// <summary>
        /// Добавить средний пульс
        /// </summary>
        /// <param name="text">Порядковый номер тренировочного дня</param>
        void AddAverageHeartRate(string text);

        /// <summary>
        /// Добавить сожжённые калории
        /// </summary>
        /// <param name="text">Порядковый номер тренировочного дня</param>
        void AddCalories(string text);

        /// <summary>
        /// Добавить длительность тренировки
        /// </summary>
        /// <param name="text">Минуты и часы [1ч20мин]</param>
        void AddDurationWorkout(string text);
    }
}
