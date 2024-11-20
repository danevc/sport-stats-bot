using Stats.Enums;
using Stats.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Stats
{
    public static class SqlSport
    {
        private static string conString = ConfigurationManager.AppSettings["ConnectionString"];

        private static readonly string getSportSchedule_sqlquery = @"SELECT ss.Id as SportScheduleId, ss.DayOfWeek, mg.MuscleGroupName, mg.Id as MuscleGroupId, e.Id as ExerciseId, e.ExerciseName
                                                                        FROM SportSchedule as ss
                                                                        INNER JOIN MuscleGroup as mg ON ss.MuscleGroupId = mg.Id
                                                                        INNER JOIN UserTelegram as u ON u.Id = ss.UserId
                                                                        LEFT JOIN Exercise as e ON e.MuscleGroupId = mg.Id 
                                                                        WHERE u.Id = @Id";

        private static readonly string addSportSchedule_sqlquery = @"INSERT INTO SportSchedule (Id, DayOfWeek, MuscleGroupId, UserId)
                                                                        VALUES (@Id, @DayOfWeek, @MuscleGroupId, @UserId);";

        private static readonly string addMuscleGroup_sqlquery = @"INSERT INTO MuscleGroup (Id, MuscleGroupName, UserId)
                                                                    VALUES (@Id, @MuscleGroupName, @UserId);";

        private static readonly string addExercise_sqlquery = @"INSERT INTO Exercise (Id, ExerciseName, MuscleGroupId)
                                                                    VALUES (@Id, @ExerciseName, @MuscleGroupId);";

        private static readonly string addSportAudit_sqlquery = @"INSERT INTO SportAudit (Id, UserId, ExerciseId, MuscleGroupId, WorkoutDate, Weight, NumOfRepetitions, Approach)
                                                                    VALUES (@Id, @UserId, @ExerciseId, @MuscleGroupId, @WorkoutDate, @Weight, @NumOfRepetitions, @Approach);";

        public static readonly string getSportAuditLastTwoMonth_sqlquery = @"SELECT sa.WorkoutDate, mg.MuscleGroupName, mg.Id as MuscleGroupId, e.ExerciseName, e.Id as ExerciseId, sa.Approach, sa.Weight, sa.NumOfRepetitions
                                                                    FROM SportAudit as sa
                                                                    INNER JOIN Exercise as e ON e.Id = sa.ExerciseId
                                                                    INNER JOIN MuscleGroup as mg ON mg.Id = sa.MuscleGroupId
                                                                    INNER JOIN UserTelegram as u ON u.Id = sa.UserId
                                                                    WHERE u.Id = @Id AND mg.Id = @MuscleGroupId AND WorkoutDate >= DATEADD(month, -2, GETDATE())
                                                                    ORDER BY sa.WorkoutDate, mg.MuscleGroupName, e.ExerciseName, sa.Approach, sa.Weight, sa.NumOfRepetitions;";

        public static void AddExercise(Exercise exercise)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    var command = new SqlCommand(addExercise_sqlquery, connection);

                    command.Parameters.AddWithValue("@Id", exercise.Id);
                    command.Parameters.AddWithValue("@ExerciseName", exercise.ExerciseName);
                    command.Parameters.AddWithValue("@MuscleGroupId", exercise.MuscleGroupId);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        public static List<SportSchedule> GetSportSchedule(int UserId)
        {
            var sportSchedules = new List<SportSchedule>();

            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    var command = new SqlCommand(getSportSchedule_sqlquery, connection);

                    command.Parameters.AddWithValue("@Id", UserId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var scheduleId = (Guid)reader[reader.GetOrdinal("SportScheduleId")];

                            var ordinalExId = reader.GetOrdinal("ExerciseId");
                            Guid exerciseId = !reader.IsDBNull(ordinalExId) ? (Guid)reader[ordinalExId] : Guid.Empty;

                            var muscleGroupId = !reader.IsDBNull(reader.GetOrdinal("MuscleGroupId")) ? (Guid)reader[reader.GetOrdinal("MuscleGroupId")] : Guid.Empty;

                            if (!sportSchedules.Any(e => e.Id == scheduleId))
                            {
                                var schedule = new SportSchedule
                                {
                                    Id = scheduleId,
                                    DayOfWeek = (MyDayOfWeek)reader[reader.GetOrdinal("DayOfWeek")],
                                    MuscleGroupName = (string)reader[reader.GetOrdinal("MuscleGroupName")],
                                    MuscleGroupId = muscleGroupId
                                };
                                if(exerciseId != null && exerciseId != Guid.Empty)
                                {
                                    schedule.Exercises = new List<Exercise>();
                                    schedule.Exercises.Add(new Exercise
                                    {
                                        Id = exerciseId,
                                        ExerciseName = (string)reader[reader.GetOrdinal("ExerciseName")],
                                        MuscleGroupId = muscleGroupId
                                    });
                                }
                                sportSchedules.Add(schedule);

                            }
                            else
                            {
                                if (exerciseId != Guid.Empty)
                                {
                                    sportSchedules.FirstOrDefault(e => e.Id == scheduleId).Exercises.Add(new Exercise
                                    {
                                        Id = exerciseId,
                                        ExerciseName = (string)reader[reader.GetOrdinal("ExerciseName")],
                                        MuscleGroupId = muscleGroupId
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            return sportSchedules;
        }

        public static List<SportAuditReport> GetSportAudit(int UserId, Guid MuscleGroupId)
        {
            var sportAudits = new List<SportAuditReport>();

            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    var command = new SqlCommand(getSportAuditLastTwoMonth_sqlquery, connection);

                    command.Parameters.AddWithValue("@Id", UserId);
                    command.Parameters.AddWithValue("@MuscleGroupId", MuscleGroupId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var muscleGroupId = (Guid)reader[reader.GetOrdinal("MuscleGroupId")];
                            var workoutDate = (DateTime)reader[reader.GetOrdinal("WorkoutDate")];

                            var ordinalExId = reader.GetOrdinal("ExerciseId");
                            Guid exerciseId = !reader.IsDBNull(ordinalExId) ? (Guid)reader[ordinalExId] : Guid.Empty;

                            if (!sportAudits.Any(e => e.MuscleGroupId == muscleGroupId && e.WorkoutDate.Date == workoutDate.Date))
                            {
                                var sportAudit = new SportAuditReport
                                {
                                    MuscleGroupId = muscleGroupId,
                                    MuscleGroupName = (string)reader[reader.GetOrdinal("MuscleGroupName")],
                                    Exercises = new List<ExercisesReport>(),
                                    WorkoutDate = workoutDate.Date
                                };
                                if (exerciseId != null && exerciseId != Guid.Empty)
                                {
                                    sportAudit.Exercises = new List<ExercisesReport>();
                                    sportAudit.Exercises.Add(new ExercisesReport
                                    {
                                        ExerciseId = (Guid)reader[reader.GetOrdinal("ExerciseId")],
                                        ExerciseName = (string)reader[reader.GetOrdinal("ExerciseName")],
                                        Approach = (int)reader[reader.GetOrdinal("Approach")],
                                        Weight = (int)reader[reader.GetOrdinal("Weight")],
                                        NumOfRepetitions = (int)reader[reader.GetOrdinal("NumOfRepetitions")]
                                    });
                                }
                                sportAudits.Add(sportAudit);
                            }
                            else
                            {
                                if (exerciseId != Guid.Empty)
                                {
                                    sportAudits.FirstOrDefault(e => e.MuscleGroupId == muscleGroupId && e.WorkoutDate.Date == workoutDate.Date).Exercises.Add(new ExercisesReport
                                    {
                                        ExerciseId = (Guid)reader[reader.GetOrdinal("ExerciseId")],
                                        ExerciseName = (string)reader[reader.GetOrdinal("ExerciseName")],
                                        Approach = (int)reader[reader.GetOrdinal("Approach")],
                                        Weight = (int)reader[reader.GetOrdinal("Weight")],
                                        NumOfRepetitions = (int)reader[reader.GetOrdinal("NumOfRepetitions")]
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            return sportAudits.Any() ? sportAudits : new List<SportAuditReport>();
        }

        public static void AddSportAudit(SportAudit sportAudit)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    var command = new SqlCommand(addSportAudit_sqlquery, connection);

                    command.Parameters.AddWithValue("@Id", sportAudit.Id);
                    command.Parameters.AddWithValue("@MuscleGroupId", sportAudit.MuscleGroupId);
                    command.Parameters.AddWithValue("@ExerciseId", sportAudit.ExerciseId);
                    command.Parameters.AddWithValue("@UserId", sportAudit.UserId);
                    command.Parameters.AddWithValue("@WorkoutDate", sportAudit.WorkoutDate);
                    command.Parameters.AddWithValue("@Weight", (int)sportAudit.Weight);
                    command.Parameters.AddWithValue("@NumOfRepetitions", sportAudit.NumOfRepetitions);
                    command.Parameters.AddWithValue("@Approach", sportAudit.Approach);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        public static void AddSportSchedule(SportSchedule sportSchedule)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    var command = new SqlCommand(addSportSchedule_sqlquery, connection);

                    command.Parameters.AddWithValue("@Id", sportSchedule.Id);
                    command.Parameters.AddWithValue("@DayOfWeek", (int)sportSchedule.DayOfWeek);
                    command.Parameters.AddWithValue("@MuscleGroupId", sportSchedule.MuscleGroupId);
                    command.Parameters.AddWithValue("@UserId", sportSchedule.UserId);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        public static void AddMuscleGroup(MuscleGroup muscleGroup)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    var command = new SqlCommand(addMuscleGroup_sqlquery, connection);

                    command.Parameters.AddWithValue("@Id", muscleGroup.Id);
                    command.Parameters.AddWithValue("@MuscleGroupName", muscleGroup.MuscleGroupName);
                    command.Parameters.AddWithValue("@UserId", muscleGroup.UserId);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}

