using System.ComponentModel;

namespace SportStats.Enums
{
    public enum MyDayOfWeek
    {
        [Description("Не выбран")]
        None = 0,
        [Description("Понедельник")]
        Monday,
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
