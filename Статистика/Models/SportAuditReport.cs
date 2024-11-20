using System;
using System.Collections.Generic;

namespace Stats.Models
{
    public class SportAuditReport
    {
        public int UserId { get; set; }
        public List<ExercisesReport> Exercises { get; set; }
        public Guid MuscleGroupId { get; set; }
        public string MuscleGroupName { get; set; }
        public DateTime WorkoutDate { get; set; }
    }
}
