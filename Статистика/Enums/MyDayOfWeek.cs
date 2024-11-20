using System;
using System.ComponentModel;
using System.Reflection;

namespace Stats.Enums
{
    public enum MyDayOfWeek
    {
        [Description("Понедельник")]
        Monday = 1,
        [Description("Вторник")]
        Tuesday,
        [Description("Среда")]
        Wednesday,
        [Description("Четверг")]
        Thursday,
        [Description("Пятница")]
        Friday,
        [Description("Суббота")]
        Saturday,
        [Description("Воскресенье")]
        Sunday
    }
}
