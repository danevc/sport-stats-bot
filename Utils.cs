using ScottPlot;
using ScottPlot.TickGenerators.TimeUnits;
using SportStats.Enums;
using SportStats.Models;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SportStats
{
    public static class Utils
    {
        public static TrainingDay? GetCurrentTrainDay(Schedule schedule)
        {
            var date = schedule.DateFirstTrainingDay;
            if (date is null) return null;

            if (DateTime.Today == date.Value.Date)
            {
                return schedule.TrainingDays.FirstOrDefault(e => e.SequenceNumber == 1);
            }

            var cycle = Convert.ToDouble(schedule?.TrainingDays.Count() + schedule?.TrainingDays.Sum(e => e.RestDaysAfter));

            if (cycle == 0)
            {
                return null;
            }

            if (DateTime.Today < date.Value.Date)
            {
                while (date > DateTime.Today)
                {
                    if (date.Value.Date.AddDays(-cycle) > DateTime.Today)
                    {
                        break;
                    }
                    date = date.Value.Date.AddDays(-cycle);
                }
            }
            else if (DateTime.Today > date.Value.Date)
            {
                while(date < DateTime.Today)
                {
                    if (date.Value.Date.AddDays(cycle) > DateTime.Today)
                    {
                        break;
                    }
                    date = date.Value.Date.AddDays(cycle);
                }
            }

            if (DateTime.Today == date.Value.Date)
            {
                return schedule?.TrainingDays.FirstOrDefault(e => e.SequenceNumber == 1);
            }

            foreach (var day in schedule.TrainingDays.OrderBy(e => e.SequenceNumber))
            {
                date = date.Value.Date.AddDays(day.RestDaysAfter + 1);
                if (date == DateTime.Today)
                {
                    return day;
                }
            }

            return null;
        }

        public static string GetWorkoutStringMessage(int approach, Exercise curEx, ExerciseReport? lastEx)
        {
            var _response = $"Упражнение: <b>{curEx.ExerciseName}</b>\nПодход: <b>#{approach}</b>\nПрошлое занятие: ";

            if (lastEx?.Weight != 0 && lastEx?.NumOfRepetitions != null)
            {
                _response += $"<b>{lastEx.Weight}кг</b> на <b>{lastEx.NumOfRepetitions}</b> повторен{EndingDependsOfNum(lastEx.NumOfRepetitions)}.";
            }
            else if (lastEx?.Weight == 0 && lastEx?.NumOfRepetitions != null)
            {
                _response += $"<b>{lastEx.NumOfRepetitions}</b> повторен{EndingDependsOfNum(lastEx.NumOfRepetitions)}.";
            }
            else if (lastEx?.Weight == null && lastEx?.NumOfRepetitions == null)
            {
                _response += $"-";
            }

            return _response;
        }

        public static string EndingDependsOfNum(int num)
        {
            string end = "ий";

            int numEnd = num % 10;

            if (numEnd == 0 || numEnd >= 5 || num % 100 >= 10 && num % 100 <= 20)
            {
                end = "ий";
            }
            else if (numEnd == 1)
            {
                end = "иe";
            }
            else if (numEnd > 1 && numEnd < 5)
            {
                end = "ия";
            }

            return end;
        }

        public static string GetStringExercises(List<Exercise> exercises, List<Exercise>? doneExercises = default)
        {
            var message = "";
            try
            {
                exercises = exercises.OrderBy(e => e.CreatedOn).ThenBy(e => e.ExerciseId).ToList();
                
                if (exercises == null || !exercises.Any())
                    throw new Exception("exercises == null || !exercises.Any()");

                if (doneExercises != null && doneExercises.Any())
                {
                    for (var i = 0; i < exercises.Count(); i++)
                    {
                        if (doneExercises.Any(e => e.ExerciseId == exercises[i].ExerciseId))
                        {
                            message += $"<b>{i + 1}. ✅</b> {exercises[i].ExerciseName}\n";
                        }
                        else
                        {
                            message += $"<b>{i + 1}. ▶</b> {exercises[i].ExerciseName}\n";
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < exercises.Count(); i++)
                    {
                        message += $"<b>{i + 1}. </b> {exercises[i].ExerciseName}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return message;
        }

        public static (int, int) ParseResApproach(string text)
        {
            var isCorrect = false;
            int weight = 0;
            int numOfRepetitions = 0;
            var splitText = text.Split('-');
            if (splitText.Length == 1)
            {
                if (int.TryParse(splitText[0], out int num))
                {
                    numOfRepetitions = num;
                    isCorrect = true;
                }
            }
            else if (splitText.Length == 2)
            {
                if (int.TryParse(splitText[0], out int num1))
                {
                    weight = num1;
                }

                if (int.TryParse(splitText[1], out int num2))
                {
                    numOfRepetitions = num2;
                    isCorrect = true;
                }
                else
                {
                    isCorrect = false;
                }
            }
            else
            {
                isCorrect = false;
            }

            if (isCorrect)
            {
                return (weight, numOfRepetitions);
            }
            else
            {
                return (-1, -1);
            }
        }

        public static DateTime? ParseDate(string text)
        {
            var month = 0;
            var year = 0;

            var splitText = text.Split('.');
            if (splitText.Length == 3)
            {
                if (int.TryParse(splitText[1], out int num2))
                {
                    if(num2 > 0 && num2 < 13)
                    {
                        month = num2;
                    }
                }

                if (int.TryParse(splitText[2], out int num3))
                {
                    if (num3 > 1990 && num3 < 2200)
                    {
                        year = num3;
                    }
                }

                if (int.TryParse(splitText[0], out int day))
                {
                    if(month != 0 && year != 0)
                    {
                        if (day <= DateTime.DaysInMonth(year, month))
                        {
                            return new DateTime(year, month, day);
                        }
                    }
                }
            }
            return null;
        }

        public static string GetStringSchedule(Schedule schedule)
        {
            if (schedule == null) 
                throw new Exception("GetStringSchedule || schedule == null");
            
            var message = "";
            var restDays = 0;

            foreach (var day in schedule.TrainingDays)
            {
                message += $"<u>Тренировочный день: <b>«{day.TrainingDayName}».</b></u>\n\n";
                foreach (var exercise in day.Exercises)
                {
                    message += $"-{exercise.ExerciseName}\n";
                }
                message += "\n";

                for (int i = 0; i < day.RestDaysAfter; i++)
                {
                    message += "<u>День отдыха</u>\n\n";
                }
                restDays += day.RestDaysAfter + 1;
            }
            

            return message;
        }

        public static string GetHoursByMin(int mins)
        {
            string hoursString;
            int hours = mins / 60;
            if (hours > 0)
            {
                hoursString = $"{hours}ч\n{mins - hours * 60}мин";
            }
            else
            {
                hoursString = $"{mins}мин";
            }

            return hoursString;
        }

        public static Plot CreateExercisesPlot(List<DateTime> dates, List<int> FirstPlot, List<int> SecondPlot, string Title = "Без названия")
        {
            var myPlot = new Plot();
            var size = dates.Count();

            for (int i = 0; i < size; i++)
            {
                var bars = new List<Bar>();
                var hWeight = FirstPlot[i];
                var hNum = SecondPlot[i];

                var firstBarNum = hNum;
                var secondBarNum = hWeight;
                var colorFirst = Colors.LightBlue;
                var colorSecond = Colors.Tomato;
                var firstBarLabel = $"{firstBarNum}";
                var secondBarLabel = $"{secondBarNum}";

                if (hWeight < hNum)
                {
                    firstBarNum = hWeight;
                    secondBarNum = hNum;
                    colorFirst = Colors.Tomato;
                    colorSecond = Colors.LightBlue;
                    firstBarLabel = $"{firstBarNum}";
                    secondBarLabel = $"{secondBarNum}";
                }

                if (hWeight == hNum)
                {
                    firstBarLabel = $"{secondBarNum} = {firstBarNum}";
                    secondBarLabel = "";
                }

                var barFirst = new Bar()
                {
                    FillColor = colorFirst,
                    Position = i,
                    ValueBase = 0,
                    Value = firstBarNum,
                    Label = firstBarLabel,
                    CenterLabel = true,

                };
                var barSecond = new Bar()
                {
                    FillColor = colorSecond,
                    Position = i,
                    ValueBase = firstBarNum,
                    Value = secondBarNum,
                    Label = secondBarLabel,
                    CenterLabel = true,
                };

                bars.Add(barFirst);
                bars.Add(barSecond);

                var barPlot = myPlot.Add.Bars(bars);
                barPlot.Horizontal = false;
            }

            myPlot.Axes.Margins(bottom: 0);

            var tickPositions = Generate.Consecutive(size);
            var tickLabels = dates.Select(x => $"{x.ToString("dd MMMM", CultureInfo.CreateSpecificCulture("ru-RU"))}").ToArray();
            if (size > 8)
            {
                myPlot.Axes.Bottom.TickLabelStyle.Rotation = 30;
                myPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            }

            var item1 = new LegendItem()
            {
                LabelText = "Вес",
                FillColor = Colors.Tomato
            };
            var item2 = new LegendItem()
            {
                LabelText = "Повторений",
                FillColor = Colors.LightBlue
            };

            var padding = new PixelPadding(30, 30, 100, 100);

            myPlot.Axes.Bottom.SetTicks(tickPositions, tickLabels);
            myPlot.HideGrid();
            myPlot.Legend.ManualItems.Add(item1);
            myPlot.Legend.ManualItems.Add(item2);
            myPlot.Legend.Orientation = Orientation.Horizontal;
            myPlot.ShowLegend(Edge.Bottom);
            myPlot.Layout.Fixed(padding);
            myPlot.Axes.Title.Label.Text = Title;
            myPlot.Axes.Title.Label.ForeColor = Colors.DarkRed;
            myPlot.Axes.Title.Label.FontSize = 32;
            myPlot.Axes.Title.Label.Bold = true;

            return myPlot;
        }

        public static Plot CreateWorkoutPlot(List<Workout> workouts, string name)
        {
            var myPlot = new Plot();

            int size = workouts.Count();
            for (int i = 0; i < size; i++)
            {
                var bars = new List<Bar>();

                var workoutDuration 
                    = workouts[i].Duration == 0 
                    ? Convert.ToInt32((workouts[i].ExerciseReports.Max(e => e.CreatedOn) - workouts[i].ExerciseReports.Min(e => e.CreatedOn)).Value.TotalMinutes) 
                    : workouts[i].Duration;
                
                var coefficient = 0.0;
                foreach (var exReport in workouts[i].ExerciseReports)
                {
                    coefficient += exReport.Weight != 0 ? exReport.Weight * exReport.NumOfRepetitions : exReport.NumOfRepetitions;
                }
                coefficient = Math.Round(coefficient / 400, 2);

                bars.Add(new Bar()
                {
                    FillColor = Colors.Tomato,
                    Position = i,
                    ValueBase = 0,
                    Value = coefficient,
                    Label = $"{coefficient}",
                    CenterLabel = true,
                });
                bars.Add(new Bar()
                {
                    FillColor = Colors.LightBlue,
                    Position = i,
                    ValueBase = coefficient,
                    Value = workoutDuration,
                    Label = $"{GetHoursByMin(workoutDuration)}",
                    CenterLabel = true,
                });
                bars.Add(new Bar()
                {
                    FillColor = Colors.Red,
                    Position = i,
                    ValueBase = 0,
                    Value = -workouts[i].AverageHeartRate / 10,
                    Label = $"{workouts[i].AverageHeartRate}",
                    CenterLabel = true,
                });
                bars.Add(new Bar()
                {
                    FillColor = Colors.LightGreen,
                    Position = i,
                    ValueBase = -workouts[i].AverageHeartRate / 10,
                    Value = -workouts[i].Calories / 20,
                    Label = $"{workouts[i].Calories}",
                    CenterLabel = true,
                });
                var barPlot = myPlot.Add.Bars(bars);
                barPlot.Horizontal = false;
            }

            myPlot.Axes.Margins(bottom: 0);

            var tickPositions = Generate.Consecutive(size);
            var tickLabels = workouts.Select(x => DateTimeToString(x.CreatedOn)).ToArray();

            if (size > 8)
            {
                myPlot.Axes.Bottom.TickLabelStyle.Rotation = 30;
                myPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            }

            var item1 = new LegendItem()
            {
                LabelText = "Длительность тренировки",
                FillColor = Colors.LightBlue
            };
            var item2 = new LegendItem()
            {
                LabelText = "Эффективность",
                FillColor = Colors.Tomato
            };

            var padding = new PixelPadding(30, 30, 100, 100);

            myPlot.Legend.ManualItems.Add(item1);
            myPlot.Legend.ManualItems.Add(item2);
            myPlot.Axes.Bottom.SetTicks(tickPositions, tickLabels);
            myPlot.HideGrid();
            myPlot.Legend.Orientation = Orientation.Horizontal;
            myPlot.ShowLegend(Edge.Bottom);
            myPlot.Layout.Fixed(padding);
            myPlot.Axes.Title.Label.Text = name;
            myPlot.Axes.Title.Label.ForeColor = Colors.DarkRed;
            myPlot.Axes.Title.Label.FontSize = 32;
            myPlot.Axes.Title.Label.Bold = true;

            return myPlot;
        }

        public static string DateTimeToString(DateTime date)
        {
            var res = date.ToString("dd MMMM", CultureInfo.CreateSpecificCulture("ru-RU"));
            var indexSpace = res.IndexOf(' ');
            return res.Substring(0, indexSpace + 4);
        }

        public static bool TryParseTime(string timeString, out int res)
        {
            var match = Regex.Match(timeString, @"(\d+)ч(\d+)мин|(\d+)мин(\d+)ч");

            if (match.Success)
            {
                int hours = 0;
                int minutes = 0;

                if (match.Groups[1].Success)
                {
                    hours = int.Parse(match.Groups[1].Value);
                    minutes = int.Parse(match.Groups[2].Value);
                }
                else
                {
                    hours = int.Parse(match.Groups[4].Value);
                    minutes = int.Parse(match.Groups[3].Value);

                }
                res = hours * 60 + minutes;
                return true;
            }
            else
            {
                res = 0;
                return false;
            }
        }
    }
}
