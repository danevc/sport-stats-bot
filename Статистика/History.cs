using Stats.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stats
{
    public static class History
    {
        private static int _today = 0;
        private static bool _isUserDefinedDay = false;
        private static List<SportSchedule> _schedule = new List<SportSchedule>();
        private static SportAuditReport _auditLastWorkout = new SportAuditReport();
        private static List<SportSchedule> _sportSchedulesHistory = new List<SportSchedule>();
        private static List<MuscleGroup> _muscleGroupHistory = new List<MuscleGroup>();
        private static int _userId = 0;
        private static Exercise _currentExerciseId = new Exercise();
        private static int _numApproach = 1;
        private static Guid _statByMuscleGroup = Guid.Empty;
        private static List<Guid> _doneExercises = new List<Guid>();

        public static void Clear()
        {
            _sportSchedulesHistory = new List<SportSchedule>();
            _muscleGroupHistory = new List<MuscleGroup>();
            _currentExerciseId = new Exercise();
            _auditLastWorkout = new SportAuditReport();
            _numApproach = 1;   
            _statByMuscleGroup = Guid.Empty;
            _doneExercises = new List<Guid>();
        }

        public static List<Guid> GetDoneExercises()
        {
            return _doneExercises;
        }

        public static void AddDoneExercises(Guid doneExercises)
        {
            _doneExercises.Add(doneExercises);
        }

        public static Guid GetStatMuscleGroupId()
        {
            return _statByMuscleGroup;
        }

        public static void SetStatMuscleGroupId(Guid statByMuscleGroup)
        {
            _statByMuscleGroup = statByMuscleGroup;
        }

        public static SportAuditReport GetAuditLastWorkout()
        {
            return _auditLastWorkout;
        }

        public static void SetAuditLastWorkout(SportAuditReport auditLastWorkout)
        {
            _auditLastWorkout = auditLastWorkout;
        }

        public static void SetNumApproach(int numApproach)
        {
            _numApproach = numApproach;
        }

        public static int GetNumApproach()
        {
            return _numApproach;
        }

        public static void SetCurrentExercise(Exercise exercise)
        {
            _currentExerciseId = exercise;
        }

        public static Exercise GetCurrentExercise()
        {
            return _currentExerciseId;
        }

        public static void SetDay(int day = 0)
        {
            var today = _today;
            if (day == 0 && !_isUserDefinedDay)
            {
                _today = DateTime.Now.DayOfWeek != 0 ? (int)DateTime.Now.DayOfWeek : 7;
            }
            else if (day != 0)
            {
                _isUserDefinedDay = true;
                _today = day;
            }

            if (today != _today)
            {
                var sportSchedule = SqlSport.GetSportSchedule(_userId);
                SetSchedule(sportSchedule);
            }
        }

        public static int GetDay()
        {
            return _today;
        }

        public static List<SportSchedule> GetSchedule()
        {
            return _schedule;
        }

        public static void SetSchedule(List<SportSchedule> schedule)
        {
            _schedule = schedule;
        }

        public static int GetUserId()
        {
            return _userId;
        }

        public static void SetUserId(int id)
        {
            _userId = id;
        }

        public static List<SportSchedule> GetSportSchedulesHistory()
        {
            return _sportSchedulesHistory;
        }

        public static void AddSportSchedulesHistory(SportSchedule sportSchedule)
        {
            _sportSchedulesHistory.Add(sportSchedule);
        }

        public static void ChangeSportSchedulesHistory(SportSchedule sportSchedule)
        {
            var s = _sportSchedulesHistory.FirstOrDefault(e => e.Id == sportSchedule.Id);

            s.MuscleGroupId = sportSchedule.MuscleGroupId;
            s.DayOfWeek = sportSchedule.DayOfWeek;
            s.UserId = sportSchedule.UserId;
        }

        public static List<MuscleGroup> GetMuscleGroupHistory()
        {
            return _muscleGroupHistory;
        }

        public static void AddMuscleGroupHistory(MuscleGroup muscleGroup)
        {
            _muscleGroupHistory.Add(muscleGroup);
        }
    }
}
