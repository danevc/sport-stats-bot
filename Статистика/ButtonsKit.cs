using Stats.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace Stats
{
    public class ButtonsKit
    {
        public static int buttonsPerRow = 3;

        public static ReplyKeyboardMarkup ChangeButtons(ButtonGroups group)
        {
            var buttons = new List<KeyboardButton[]>();

            switch (group)
            {
                case ButtonGroups.ChooseDayOfWeek:
                    buttons = new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Понедельник"),
                            new KeyboardButton("Вторник"),
                            new KeyboardButton("Среда"),
                            new KeyboardButton("Четверг")

                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Пятница"),
                            new KeyboardButton("Суббота"),
                            new KeyboardButton("Воскресенье"),
                            new KeyboardButton("Закончить")
                        }
                    };
                    break;
                case ButtonGroups.AddSchedule:
                    buttons = new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Добавить расписание")

                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_ToHome)
                        }
                    };
                    break;
                case ButtonGroups.StartWithSchedule:
                    buttons = new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_Workout),
                            new KeyboardButton(Utils._btn_Schedule)
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_Stats),
                            new KeyboardButton(Utils._btn_ChooseDay)
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_ToHome)
                        }
                    };
                    break;
                case ButtonGroups.OnSaveScheduler:
                    buttons = new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Сохранить")

                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_ToHome)
                        }
                    };
                    break;
                case ButtonGroups.Workout:
                    buttons.Clear();
                    var exercises = Utils.GetExercisesToday();
                    if(exercises != null)
                    {
                        for (int i = 0; i < exercises.Count() / buttonsPerRow + 1; i++)
                        {
                            var rowButtons = new KeyboardButton[Math.Min(buttonsPerRow, exercises.Count - i * buttonsPerRow)];
                            for (int j = 0; j < rowButtons.Length; j++)
                            {

                                var btnText = "";
                                if (History.GetDoneExercises().Contains(exercises[i * buttonsPerRow + j].Id))
                                {
                                    btnText += "✅ ";
                                }
                                else
                                {
                                    btnText += "▶ ";
                                }
                                btnText += exercises[i * buttonsPerRow + j]?.ExerciseName;
                                rowButtons[j] = new KeyboardButton(btnText);
                            }
                            buttons.Add(rowButtons);
                        }
                    }
                    buttons.Add(new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_AddExercise),
                            new KeyboardButton(Utils._btn_ToHome)
                        });
                    break;
                case ButtonGroups.AddingExercises:
                    buttons = new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Добавить ещё"),
                            new KeyboardButton("Закончить")
                        }
                    };
                    break;
                case ButtonGroups.Stats:
                    buttons.Clear();
                    var schedule = History.GetSchedule();
                    if (schedule != null)
                    {
                        for (int i = 0; i < schedule.Count() / buttonsPerRow + 1; i++)
                        {
                            var rowButtons = new KeyboardButton[Math.Min(buttonsPerRow, schedule.Count - i * buttonsPerRow)];
                            for (int j = 0; j < rowButtons.Length; j++)
                            {
                                rowButtons[j] = new KeyboardButton(schedule[i * buttonsPerRow + j]?.MuscleGroupName);
                            }
                            buttons.Add(rowButtons);
                        }
                    }
                    buttons.Add(new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_ToHome)
                        });
                    break;
                case ButtonGroups.StatsByMuscleGroup:
                    buttons = new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("По упражнениям за всё время"),
                            new KeyboardButton("По упражнениям за два месяца")
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_ToHome)
                        }
                    };
                    break;
                case ButtonGroups.StatsByExercise:
                    buttons.Clear();
                    var exercisesForStat = Utils.GetExercisesByMuscleGroup(History.GetStatMuscleGroupId());
                    if (exercisesForStat != null)
                    {
                        for (int i = 0; i < exercisesForStat.Count() / buttonsPerRow + 1; i++)
                        {
                            var rowButtons = new KeyboardButton[Math.Min(buttonsPerRow, exercisesForStat.Count - i * buttonsPerRow)];
                            for (int j = 0; j < rowButtons.Length; j++)
                            {
                                rowButtons[j] = new KeyboardButton(exercisesForStat[i * buttonsPerRow + j]?.ExerciseName);
                            }
                            buttons.Add(rowButtons);
                        }
                    }
                    buttons.Add(new KeyboardButton[]
                        {
                            new KeyboardButton("По этой группе мышц"),
                            new KeyboardButton(Utils._btn_ToHome)
                        });
                    break;
                default:
                    buttons = new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_ToHome)
                        }
                    }; 
                    break;
            }

            var replyKeyboard = new ReplyKeyboardMarkup(buttons);
            return replyKeyboard;
        }
    }
}
