using Microsoft.EntityFrameworkCore;
using SportStats.Models;
using Microsoft.Extensions.Configuration;

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

        private readonly IConfiguration _configuration;
        private readonly bool _isRelease;
        private readonly string _connectionString;

        public SportContext(IConfiguration configuration) {

            _configuration = configuration;
            _isRelease = _configuration.GetValue<bool>("isRelease");

            if (_isRelease)
                _connectionString = _configuration.GetConnectionString("SportStatsDB");
            else
                _connectionString = _configuration.GetConnectionString("SportStatsDB_Dev");

            //this.Database.EnsureDeleted();
            //this.Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
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
