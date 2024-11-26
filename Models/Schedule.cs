using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStats.Models
{
    public class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ScheduleId { get; set; }
        public DateTime CreatedOn { get; set; }
        public long UserId { get; set; }
        public string ScheduleName { get; set; }
        public List<TrainingDay> TrainingDays { get; set; }
    }
}
