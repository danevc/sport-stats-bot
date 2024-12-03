using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStats.Models
{
    public class Schedule
    {
        /// <summary>
        /// ID расписания
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string? ScheduleName { get; set; }

        /// <summary>
        /// День первой тренировки
        /// </summary>
        public DateTime? DateFirstTrainingDay { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Тренировочные дни
        /// </summary>
        public virtual List<TrainingDay> TrainingDays { get; set; } = new();
    }
}
