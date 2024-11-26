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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ExerciseId { get; set; }
        public DateTime CreatedOn { get; set; }
        public long UserId {  get; set; }
        public string Group { get; set; }
        public string ExerciseName { get; set; }
        public virtual ICollection<ExerciseTrainingDay> TrainingDays { get; set; } = new List<ExerciseTrainingDay>();
    }
}
