using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStats.Models
{
    public class Workout
    {
        /// <summary>
        /// ID тренировки
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid WorkoutId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        [Required]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// ID тренировочного дня
        /// </summary>
        [Required]
        public Guid TrainingDayId { get; set; }

        /// <summary>
        /// Продолжительность тренировки
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Средний пульс
        /// </summary>
        public int AverageHeartRate { get; set; }

        /// <summary>
        /// Сожжено калорий
        /// </summary>
        public int Calories { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Отчены по упражнениям
        /// </summary>
        public virtual List<ExerciseReport> ExerciseReports { get; set; } = new();

        /// <summary>
        /// Тренировочный день
        /// </summary>
        public virtual TrainingDay TrainingDay { get; set; } = null!;
    }
}
