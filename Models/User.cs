using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportStats.Enums;

namespace SportStats.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DayOfBirth { get; set; }
        public int Height { get; set; }
        public List<Schedule> Schedules { get; set; }
        public List<Exercise> Exercises { get; set; }
    }
}
