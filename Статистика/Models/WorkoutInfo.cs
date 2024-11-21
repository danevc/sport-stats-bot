using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stats.Models
{
    public class WorkoutInfo
    {
        public int Koef { get; set; }
        public int MinuteDifference { get; set; }
        public DateTime WorkoutDate { get; set; }
        public string MuscleGroupName { get; set; }
    }
}
