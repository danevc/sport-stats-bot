using Stats.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stats.Models
{
    public class SportSchedule
    {
        public Guid Id { get; set; }
        public MyDayOfWeek DayOfWeek { get; set; }
        public Guid MuscleGroupId { get; set; }
        public string MuscleGroupName { get; set; }
        public List<Exercise> Exercises { get; set; }
        public int UserId { get; set; }
    }
}
