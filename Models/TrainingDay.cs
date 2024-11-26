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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TrainingDayId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string TrainingDayName { get; set; }
        public Guid ScheduleId { get; set; }
        public int RestDaysAfter { get; set; }
        public virtual ICollection<ExerciseTrainingDay> Exercises { get; set; } = new List<ExerciseTrainingDay>();
    }
}
