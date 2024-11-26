using Microsoft.EntityFrameworkCore;
using SportStats.Models;

namespace SportStats
{
    public class SportContext : DbContext
    {
        public SportContext() : base()
        {
            //Database.SetInitializer(new DropCreateDatabaseAlways<SportContext>());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseReport> ExerciseReports { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<TrainingDay> TrainingDays { get; set; }
    }
}
