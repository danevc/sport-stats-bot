using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStats.Models
{
    public class ExerciseTrainingDay
    {
        public Guid ExerciseId { get; set; }
        public virtual Exercise Exercise { get; set; }
        public Guid TrainingDayId { get; set; }
        public virtual TrainingDay TrainingDay { get; set; }
    }
}
