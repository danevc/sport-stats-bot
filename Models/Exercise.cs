using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStats.Models
{
    public class Exercise
    {
        /// <summary>
        /// ID упражнения
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ExerciseId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        [Required]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        [Required]
        public long UserId {  get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string? ExerciseName { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Тренировочные дни
        /// </summary>
        public virtual List<TrainingDay> TrainingDays { get; set; } = new ();
    }
}
