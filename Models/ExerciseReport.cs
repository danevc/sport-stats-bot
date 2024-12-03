using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStats.Models
{
    public class ExerciseReport
    {
        /// <summary>
        /// ID отчета по упражнению
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ExerciseReportId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// ID упражнения
        /// </summary>
        public Guid ExerciseId { get; set; }

        /// <summary>
        /// Вес
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Количество повторений
        /// </summary>
        public int NumOfRepetitions { get; set; }

        /// <summary>
        /// Повторение
        /// </summary>
        public int Approach { get; set; }

        /// <summary>
        /// Тренировка
        /// </summary>
        public Guid? WorkoutId { get; set; }
    }
}
