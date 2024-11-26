using SportStats.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace SportStats
{
    public class ButtonsKit
    {
        private static int btnPerRaw = 3;

        public static ReplyKeyboardMarkup GetBtnsReply(ButtonsReply group)
        {
            var buttons = new List<KeyboardButton[]>();

            switch (group)
            {
                case ButtonsReply.None:
                    buttons = new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_Workout),
                            new KeyboardButton(Utils._btn_Stats)
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton(Utils._btn_ToHome)
                        }
                    };
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

        public static InlineKeyboardMarkup GetBtnsInline(ButtonsInline group, long UserId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup();
            
            switch (group)
            {
                case ButtonsInline.Workout:
                    
                    break;
                case ButtonsInline.YesOrNo:
                    inlineKeyboard = new InlineKeyboardMarkup();
                    inlineKeyboard.AddButton("Добавить", "AddStandard");
                    inlineKeyboard.AddButton("Отмена", "NotAddStandard");
                    break;
                default:
                    break;
            }

            return inlineKeyboard;
        }
    }
}
