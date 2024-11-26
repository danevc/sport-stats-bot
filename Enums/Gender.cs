using System.ComponentModel;

namespace SportStats.Enums
{
    public enum Gender
    {
        [Description("Не выбран")]
        Unknown = 0,
        [Description("Мужчина")]
        Male,
        [Description("Женщина")]
        Female
    }
}
