using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Controller.DataBase;
using RemBerBot_BL.Models;
using Telegram.Bot.Types;
using static MySuperUniversalBot_BL.Controller.BotControllerBase;

namespace RemBerBot_BL.Controller.Controller
{
    public class ObjectControllerBase
    {
        BotControllerBase botControllerBase = new();

        public enum OperationEnum
        {
            empty,
            addBirthDate,
            addBirthDateOfUsername,
            addDayOfBirthDate,
            addPeriod,
            addPeriodCycle,
            addPeriodMenstruation,
            addPeriodDateOfLastMenstruationMenstruation,
            addNotifyTheUser,
            addNotifyTheUserChatId,
            addNotifyTheUserName
        }
        public enum NavigationEnum
        {
            MainMenu = 1,
            addBirthDate,
            PeriodMenu,
            addPeriod,
            addNotifyTheUser
        }
     
        public void Delete<G>(CallbackQuery callbackQuery, DbContext dbContext, string messageText) where G : ModelBase
        {
            string[] temp = callbackQuery!.Data!.Split(new char[] { ' ' });
            int id = Convert.ToInt32(temp[1]);

            new DataBaseControllerBase<G>(dbContext).Load().Where(per => per.ChatId == callbackQuery.Message!.Chat.Id).Where(x => x.Id == id).ToList().ForEach(per =>
            {
                new DataBaseControllerBase<G>(dbContext).Remove(per);
                botControllerBase.PrintMessage($"{messageText} видалено.", callbackQuery.Message!.Chat.Id);
            });
        }

        public void Display<G>(long chatId, string falseMessage, CallbackQueryCommands callbackQuery) where G : ModelBase
        {
            var temps = new DataBaseControllerBase<G>(new RemPerContext()).Load().Where(x => x.ChatId == chatId).ToList();

            if (temps.Count != 0)
            {
                temps.ForEach(p =>
                {
                    botControllerBase.PrintInline(p.PrintData(), chatId, botControllerBase.SetupInLine(CallbackQueryCommands.Видалити.ToString(), $"{callbackQuery} {p.Id}"));
                });
            }
            else
            {
                botControllerBase.PrintMessage(falseMessage, chatId);
            }
        }
    }
}
