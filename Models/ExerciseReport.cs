using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportStats.Models
{
    public class ExerciseReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ExerciseReportId { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid ExerciseId { get; set; }
        public int Weight { get; set; }
        public int NumOfRepetitions { get; set; }
        public int Approach { get; set; }
    }
}
