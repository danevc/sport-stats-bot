using System;

namespace Stats.Models
{
    public class MuscleGroup
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string MuscleGroupName { get; set; }
    }
}
