using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportStats.Enums;

namespace SportStats.Models
{
    public class User
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// Username в Telegram
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Id выбранного расписания
        /// </summary>
        public Guid? CurrentScheduleId { get; set; }

        /// <summary>
        /// Расписание
        /// </summary>
        public virtual List<Schedule> Schedules { get; set; } = new();

        /// <summary>
        /// Упражнения
        /// </summary>
        public virtual List<Exercise> Exercises { get; set; } = new();
    }
}
