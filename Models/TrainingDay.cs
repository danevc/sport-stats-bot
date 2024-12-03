using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStats.Models
{
    public class TrainingDay
    {
        /// <summary>
        /// ID тренировочного дня
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TrainingDayId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string? TrainingDayName { get; set; }

        /// <summary>
        /// ID расписания
        /// </summary>
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Количество дней отдыха после тренировки
        /// </summary>
        public int RestDaysAfter { get; set; }

        /// <summary>
        /// Порядковый номер
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Тренировки
        /// </summary>
        public virtual Workout Workout { get; set; } = new();

        /// <summary>
        /// Расписание
        /// </summary>
        public virtual Schedule Schedule { get; set; } = null!;

        /// <summary>
        /// Упражнения
        /// </summary>
        public virtual List<Exercise> Exercises { get; set; } = new();
    }
}
