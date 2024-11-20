using System;

namespace Stats.Models
{
    public class ExercisesReport
    {
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public int Weight { get; set; }
        public int NumOfRepetitions { get; set; }
        public int Approach { get; set; }
    }
}
