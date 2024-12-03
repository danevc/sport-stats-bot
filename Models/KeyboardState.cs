using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace SportStats.Models
{
    public class KeyboardState
    {
        public string? MessageText { get; set; }
        public InlineKeyboardMarkup? KeyboardMarkup { get; set; }
    }
}
