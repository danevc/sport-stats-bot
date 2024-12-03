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
        /// <summary>
        /// Отдает начальное сообщение для пользователя
        /// </summary>
        /// <param name="text">Любой текст</param>
        void Start(string text);

        /// <summary>
        /// Добавить упражнение
        /// </summary>
        /// <param name="text">Название упражнения</param>
        void AddExercise(string text);

        /// <summary>
        /// Добавить расписание
        /// </summary>
        /// <param name="text">Название расписания</param>
        void AddSchedule(string text);

        /// <summary>
        /// Добавить тренировочный день
        /// </summary>
        /// <param name="text">Название тренировочного дня</param>
        void AddTrainDay(string text);

        /// <summary>
        /// Добавить упражнения в тренировочный день
        /// </summary>
        /// <param name="text">Название упражнения со * или порядковые номера упражнений</param>
        void AddExercisesToTrainDay(string text);

        /// <summary>
        /// Добавить в тренировочный день информации о днях отдыха после него
        /// </summary>
        /// <param name="text">Количество дней</param>
        void AddDayRest(string text);

        /// <summary>
        /// Удалить упражнение
        /// </summary>
        /// <param name="text">Порядковый номер упражнения</param>
        void RemoveExercise(string text);
    }
}
