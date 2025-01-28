using Microsoft.IdentityModel.Tokens;
using ScottPlot;
using SportStats.Enums;
using SportStats.Models;
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

            foreach (var day in schedule.TrainingDays.OrderBy(e => e.SequenceNumber))
            {
                if (date == DateTime.Today)
                {
                    return day;
                }
                date = date.Value.Date.AddDays(day.RestDaysAfter + 1);
            }

            return null;
        }

        public static string GetWorkoutStringMessage(int approach, Exercise curEx, ExerciseReport? lastEx, ExerciseReport? bestEx = null)
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

            if(bestEx is not null)
            {
                _response += "\nЛучший результат: ";

                if (bestEx?.Weight != 0 && bestEx?.NumOfRepetitions != null)
                {
                    _response += $"<b>{bestEx.Weight}кг</b> на <b>{bestEx.NumOfRepetitions}</b> повторен{EndingDependsOfNum(bestEx.NumOfRepetitions)}.";
                }
                else if (bestEx?.Weight == 0 && bestEx?.NumOfRepetitions != null)
                {
                    _response += $"<b>{bestEx.NumOfRepetitions}</b> повторен{EndingDependsOfNum(bestEx.NumOfRepetitions)}.";
                }
                else if (bestEx?.Weight == null && bestEx?.NumOfRepetitions == null)
                {
                    _response += $"-";
                }
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
        
        public static DateTime? ParseDate(string text)
        {
            var month = 0;
            var year = 0;

            var splitText = text.Split('.');
            if (splitText.Length == 3)
            {
                if (int.TryParse(splitText[1], out int num2))
                {
                    if (num2 > 0 && num2 < 13)
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
                    if (month != 0 && year != 0)
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

        public static string GetHoursByMin(int mins, bool lineBreak = true)
        {
            string hoursString;
            int hours = mins / 60;
            if (hours > 0)
            {
                if (lineBreak)
                {
                    hoursString = $"{hours}ч\n{mins - hours * 60}мин";
                }
                else
                {
                    hoursString = $"{hours}ч {mins - hours * 60}мин";
                }
            }
            else
            {
                hoursString = $"{mins}мин";
            }

            return hoursString;
        }

        public static Plot CreateBarPlot(List<DateTime> dates, List<PlotBarElem> barsInfo, string Title = "")
        {
            var myPlot = new Plot();

            var startIndex = dates.Count - 11 >= 0 ? dates.Count - 11 : 0;

            for (int i = startIndex; i < dates.Count; i++)
            {
                var bars = new List<Bar>();

                var valueBase = 0.0;
                foreach (var barInfo in barsInfo.OrderBy(e => e.Values[i]))
                {
                    var label = $"{barInfo.Values[i]}";

                    if (barInfo.Type == BarPlotTypes.Time)
                        label = GetHoursByMin(Convert.ToInt32(barInfo.Values[i]));
                    
                    bars.Add(new Bar
                    {
                        FillColor = barInfo.color,
                        Position = i,
                        ValueBase = valueBase,
                        Value = barInfo.Values[i],
                        Label = label,
                        CenterLabel = true
                    });
                    valueBase = barInfo.Values[i];
                }

                var barPlot = myPlot.Add.Bars(bars);
                barPlot.Horizontal = false;
            }

            foreach(var barInfo in barsInfo)
            {
                myPlot.Legend.ManualItems.Add(new LegendItem
                {
                    LabelText = barInfo.Title ?? "",
                    FillColor = barInfo.color
                });
            }

            myPlot.Axes.Margins(bottom: 0);

            var tickPositions = Generate.Consecutive(dates.Count);
            var tickLabels = dates.Select(x => DateTimeToString(x)).ToArray();

            if (dates.Count > 10)
            {
                myPlot.Axes.Bottom.TickLabelStyle.Rotation = 30;
                myPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            }

            if (!Title.IsNullOrEmpty())
            {
                myPlot.Axes.Title.Label.Text = Title;
            }

            var padding = new PixelPadding(30, 30, 100, 100);
            myPlot.Axes.Bottom.SetTicks(tickPositions, tickLabels);
            myPlot.HideGrid();
            myPlot.Legend.Orientation = Orientation.Horizontal;
            myPlot.ShowLegend(Edge.Bottom);
            myPlot.Layout.Fixed(padding);
            myPlot.Axes.Title.Label.Text = Title;
            myPlot.Axes.Title.Label.ForeColor = Colors.DarkRed;
            myPlot.Axes.Title.Label.FontSize = 32;
            myPlot.Axes.Title.Label.Bold = true;
            return myPlot;
        }

        public static Multiplot CreateWorkoutPlot(List<Workout> workouts, string name)
        {
            var dates = new List<DateTime>();
            var bars1 = new List<PlotBarElem>();
            var bars2 = new List<PlotBarElem>();

            var coefBar = new List<double>();
            var durationBar = new List<double>();
            var heartRateBar = new List<double>();
            var caloriesBar = new List<double>();

            var averageDuration = 0;
            var averageCalories = 0.0;
            var averageHeartRate = 0.0;
            var averageCoef = 0.0;

            foreach (var w in workouts)
            {
                dates.Add(w.CreatedOn);
                var coefficient = 0.0;
                foreach (var exReport in w.ExerciseReports)
                {
                    coefficient += exReport.Weight != 0 ? exReport.Weight * exReport.NumOfRepetitions : exReport.NumOfRepetitions * 80;
                }
                coefficient = Math.Round(coefficient / 350, 2);
                coefBar.Add(coefficient);

                var workoutDuration
                    = w.Duration == 0
                    ? Convert.ToInt32((w.ExerciseReports.Max(e => e.CreatedOn) - w.ExerciseReports.Min(e => e.CreatedOn)).Value.TotalMinutes)
                    : w.Duration;

                averageDuration += workoutDuration;
                averageCalories += w.Calories;
                averageHeartRate += w.AverageHeartRate;
                averageCoef += coefficient;

                durationBar.Add(workoutDuration);
                heartRateBar.Add(w.AverageHeartRate);
                caloriesBar.Add(w.Calories);
            }

            averageDuration /= workouts.Count;
            averageCalories /= workouts.Where(e => e.Calories != 0).Count();
            averageHeartRate /= workouts.Where(e => e.AverageHeartRate != 0).Count();
            averageCoef /= workouts.Count;

            bars1.Add(new PlotBarElem
            {
                Values = coefBar,
                color = Colors.Tomato,
                Title = "Коэффициент",
                Type = BarPlotTypes.Number
            });
            bars1.Add(new PlotBarElem
            {
                Values = durationBar,
                color = Colors.LightBlue,
                Title = "Продолжительность тренировки",
                Type = BarPlotTypes.Time
            });
            bars2.Add(new PlotBarElem
            {
                Values = heartRateBar,
                color = Colors.Tomato,
                Title = "Средний пульс",
                Type = BarPlotTypes.Number
            });
            bars2.Add(new PlotBarElem
            {
                Values = caloriesBar,
                color = Colors.LightBlue,
                Title = "Калории",
                Type = BarPlotTypes.Number
            });


            var plot1 = CreateBarPlot(dates, bars1, name);
            var plot2 = CreateBarPlot(dates, bars2, $"Средние показатели:\nкалории: {Math.Round(averageCalories, 2)}\nсердцебиение: {Math.Round(averageHeartRate, 2)}\nкоэфицеиент: {Math.Round(averageCoef, 2)}\nдлительность: {Utils.GetHoursByMin(averageDuration, false)}");
            plot2.Axes.Title.Label.ForeColor = Colors.Black;
            plot2.Axes.Title.Label.FontSize = 12;
            plot2.Axes.Title.Label.Alignment = Alignment.LowerLeft;
            plot2.Axes.Title.Label.OffsetX = -300;

            var multiplot = new Multiplot();
            multiplot.AddPlot(plot1);
            multiplot.AddPlot(plot2);

            return multiplot;
        }
    }
}
