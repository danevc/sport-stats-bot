using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stats.Models
{
    public class SportAudit
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public Guid ExerciseId { get; set; }
        public Guid MuscleGroupId { get; set; }
        public DateTime WorkoutDate { get; set; }
        public int Weight { get; set; }
        public int NumOfRepetitions { get; set; }
        public int Approach { get; set; }
    }
}
