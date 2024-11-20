using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stats.Models
{
    public class Exercise
    {
        public Guid Id { get; set; }
        public Guid MuscleGroupId { get; set; }
        public string ExerciseName { get; set; }
    }
}
