using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportStats.Models;
using static System.Net.Mime.MediaTypeNames;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;

namespace SportStats
{
    public class Service
    {
        /// <summary>
        /// Создать/изменить расписание
        /// </summary>
        /// <param name="schedule">Расписание</param>
        public Models.ActionResult EditOrCreateSchedule(Schedule schedule, IConfigurationRoot config)
        {
            try
            {
                using (var db = new SportContext(config))
                {
                    var isCreate = true;
                    var existingSchedule = db.Schedules.Find(schedule.ScheduleId);

                    if (existingSchedule != null)
                    {
                        existingSchedule.ScheduleName = schedule.ScheduleName;
                        existingSchedule.TrainingDays = schedule.TrainingDays;
                        schedule = existingSchedule;
                        isCreate = false;
                    }
                    else
                    {
                        db.Entry(schedule).State = EntityState.Added;
                    }

                    for (int i = 0; i < schedule.TrainingDays.Count; i++)
                    {
                        var trainingDay = schedule.TrainingDays[i];
                        var existingTrainingDay = db.TrainingDays.Find(trainingDay.TrainingDayId);

                        if (existingTrainingDay != null)
                        {
                            schedule.TrainingDays[i].TrainingDayName = trainingDay.TrainingDayName;
                            schedule.TrainingDays[i].RestDaysAfter = trainingDay.RestDaysAfter;
                            schedule.TrainingDays[i].Exercises = trainingDay.Exercises;
                            schedule.TrainingDays[i] = existingTrainingDay;
                        }
                        else
                        {
                            db.Entry(trainingDay).State = EntityState.Added;
                        }

                        for (int j = 0; j < trainingDay.Exercises.Count; j++)
                        {
                            var exercise = trainingDay.Exercises[j];
                            var existingExercise = db.Exercises.Find(exercise.ExerciseId);

                            if (existingExercise != null)
                            {
                                trainingDay.Exercises[j].ExerciseName = exercise.ExerciseName;
                                trainingDay.Exercises[j].TrainingDays = exercise.TrainingDays;
                                trainingDay.Exercises[j] = existingExercise;
                            }
                            else
                            {
                                db.Entry(exercise).State = EntityState.Added;
                            }
                        }
                    }

                    db.Schedules.Add(schedule);
                    db.SaveChanges();

                    if (isCreate)
                    {
                        return new Models.ActionResult
                        {
                            Message = "Расписание успешно создано",
                            StatusCode = 200
                        };
                    }
                    else
                    {
                        return new Models.ActionResult
                        {
                            Message = "Расписание успешно изменено",
                            StatusCode = 200
                        };
                    }

                }
            }
            catch (Exception ex)
            {
                return new Models.ActionResult
                {
                    Message = $"Произошла ошибка {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Создать/изменить упражнение
        /// </summary>
        /// <param name="schedule">Упражнение</param>
        public Models.ActionResult EditOrCreateExercise(Exercise exercise, IConfigurationRoot config)
        {
            try
            {
                using (var db = new SportContext(config))
                {
                    var existingExercise = db.Exercises.Find(exercise.ExerciseId);

                    if (existingExercise != null)
                    {
                        db.Entry(exercise).State = EntityState.Modified;
                        db.SaveChanges();
                        return new Models.ActionResult
                        {
                            Message = "Упражнение успешно создано",
                            StatusCode = 200
                        };
                    }
                    else
                    {
                        db.Exercises.Add(exercise);
                        db.SaveChanges();
                        return new Models.ActionResult
                        {
                            Message = "Упражнение успешно изменено",
                            StatusCode = 200
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new Models.ActionResult
                {
                    Message = $"Произошла ошибка {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
