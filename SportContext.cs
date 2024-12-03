using Microsoft.EntityFrameworkCore;
using SportStats.Interfaces;
using SportStats.Models;

namespace SportStats
{
    public class SportContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<ExerciseReport> ExerciseReports { get; set; } = null!;
        public DbSet<Schedule> Schedules { get; set; } = null!;
        public DbSet<TrainingDay> TrainingDays { get; set; } = null!;
        public DbSet<Workout> Workouts { get; set; } = null!;

        public SportContext() {
            //this.Database.EnsureDeleted();
            //this.Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=LAPTOP-LMHEG0J6\SQLEXPRESS;Database=SportStats;Trusted_Connection=True;TrustServerCertificate=true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Exercise>()
                .Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ExerciseReport>()
                .Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Schedule>()
                .Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<TrainingDay>()
                .Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Workout>()
                .Property(u => u.CreatedOn)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Exercise>()
               .HasMany(e => e.TrainingDays)
               .WithMany(td => td.Exercises)
               .UsingEntity<Dictionary<string, object>>(
                   "ExerciseTrainingDays",
                   j => j
                       .HasOne<TrainingDay>()
                       .WithMany()
                       .HasForeignKey("TrainingDayId")
                       .OnDelete(DeleteBehavior.Restrict),
                   j => j
                       .HasOne<Exercise>()
                       .WithMany()
                       .HasForeignKey("ExerciseId")
                       .OnDelete(DeleteBehavior.Cascade));
        }   
    }
}
