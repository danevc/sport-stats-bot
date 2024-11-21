using Stats.Enums;
using Stats.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ScottPlot;
using System.Globalization;

namespace Stats
{
    public static class Utils
    {
        public static string _btn_ToHome = "↩ В начало";
        public static string _btn_Workout = "💪 Тренировка";
        public static string _btn_Stats = "📊 Статистика";
        public static string _btn_Schedule = "🗓 Расписание";
        public static string _btn_ChooseDay = "📆 Выбрать день";
        public static string _btn_AddExercise = "+ Упражнение";

        public static Plot CreateBarPlot(List<DateTime> dates, List<int> FirstPlot, List<int> SecondPlot, string Title = "Без названия")
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
                var colorFirst = Colors.Lime;
                var colorSecond = Colors.OrangeRed;
                var firstBarLabel = $"{firstBarNum}";
                var secondBarLabel = $"{secondBarNum}";

                if (hWeight < hNum)
                {
                    firstBarNum = hWeight;
                    secondBarNum = hNum;
                    colorFirst = Colors.OrangeRed;
                    colorSecond = Colors.Lime;
                    firstBarLabel = $"{firstBarNum}";
                    secondBarLabel = $"{secondBarNum}";
                }
                
                if(hWeight == hNum)
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
                FillColor = Colors.OrangeRed
            };
            var item2 = new LegendItem()
            {
                LabelText = "Повторений",
                FillColor = Colors.Lime
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

        public static Plot CreateBarWorkoutPlot(List<WorkoutInfo> workouts, string Title)
        {
            var myPlot = new Plot();

            int size = workouts.Count();

            for (int i = 0; i < size; i++)
            {
                var bars = new List<Bar>();

                bars.Add(new Bar()
                {
                    FillColor = Colors.OrangeRed,
                    Position = i,
                    ValueBase = 0,
                    Value = workouts[i].Koef / 50,
                    Label = $"{workouts[i].Koef}",
                    CenterLabel = true,
                });
                bars.Add(new Bar()
                {
                    FillColor = Colors.AliceBlue,
                    Position = i,
                    ValueBase = workouts[i].Koef / 50,
                    Value = workouts[i].MinuteDifference,
                    Label = $"{GetHoursByMin(workouts[i].MinuteDifference)}",
                    CenterLabel = true,
                });
                var barPlot = myPlot.Add.Bars(bars);
                barPlot.Horizontal = false;
            }

            myPlot.Axes.Margins(bottom: 0);

            var tickPositions = Generate.Consecutive(size);
            var tickLabels = workouts.Select(x => $"{x.WorkoutDate.ToString("dd MMMM", CultureInfo.CreateSpecificCulture("ru-RU"))}").ToArray();
            if (size > 8)
            {
                myPlot.Axes.Bottom.TickLabelStyle.Rotation = 30;
                myPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            }

            var padding = new PixelPadding(30, 30, 70, 100);

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

        public static string GetHoursByMin(int mins)
        {
            string hoursString;
            int hours = mins / 60;
            if(hours > 0)
            {
                hoursString = $"{hours}ч {mins - hours * 60}мин";
            }
            else
            {
                hoursString = $"{mins}мин";
            }

            return hoursString;
        }

        public static string GetDescription(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static string GetDayOfWeekDescription(int dayOfWeekValue)
        {
            try
            {
                var dayOfWeek = (MyDayOfWeek)Enum.Parse(typeof(MyDayOfWeek), dayOfWeekValue.ToString());
                return GetDescription(dayOfWeek);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        public static List<Exercise> GetExercisesToday() 
        {
            return History.GetSchedule().FirstOrDefault(e => (int)e.DayOfWeek == History.GetDay())?.Exercises;
        }

        public static List<Exercise> GetExercisesByMuscleGroup(Guid id)
        {
            return History.GetSchedule().FirstOrDefault(e => e.MuscleGroupId == id)?.Exercises;
        }

        public static string GetWorkoutStringResponse(int approach, ExercisesReport lastExercise)
        {
            var _response = $"Упражнение: <b>{History.GetCurrentExercise()?.ExerciseName}</b>\nПодход: <b>#{approach}</b>\nПрошлое занятие: ";
            
            if(lastExercise?.Weight != 0 && lastExercise?.NumOfRepetitions != null)
            {
                _response += $"<b>{lastExercise.Weight}кг</b> на <b>{lastExercise.NumOfRepetitions}</b> повторен{EndingDependsOfNum(lastExercise.NumOfRepetitions)}.";
            }
            else if (lastExercise?.Weight == 0 && lastExercise?.NumOfRepetitions != null)
            {
                _response += $"<b>{lastExercise.NumOfRepetitions}</b> повторен{EndingDependsOfNum(lastExercise.NumOfRepetitions)}.";
            }
            else if (lastExercise?.Weight == null && lastExercise?.NumOfRepetitions == null)
            {
                _response += $"-";
            }
            
            return _response;
        }

        public static string EndingDependsOfNum(int num)
        {
            string end = "ий";

            int numEnd = num % 10;

            if(numEnd == 0 || numEnd >= 5 || num % 100 >= 10 && num % 100 <= 20)
            {
                end = "ий";
            }
            else if (numEnd == 1)
            {
                end = "иe";
            }
            else if(numEnd > 1 && numEnd < 5)
            {
                end = "ия";
            }

            return end;
        }

        public static (int, int) ParseResApproach(string text)
        {
            var isCorrect = false;
            int weight = 0;
            int numOfRepetitions = 0;
            var splitText = text.Split('-');
            if (splitText.Length == 1)
            {
                if (int.TryParse(splitText[0], out var num))
                {
                    numOfRepetitions = num;
                    isCorrect = true;
                }
            }
            else if (splitText.Length == 2)
            {
                if (int.TryParse(splitText[0], out var num1))
                {
                    weight = num1;
                }

                if (int.TryParse(splitText[1], out var num2))
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
    }
}
