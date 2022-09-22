using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using RemBerBot_BL.Models;
using Telegram.Bot.Types;

namespace RemBerBot_BL.Controller.Controller
{
    public class ObjectControllerBase
    {
        BotControllerBase botControllerBase = new();

        /// <summary>
        /// Sets the initial value for multiple dictionaries.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="firstDictionary">First Dictionary.</param>
        /// <param name="secondDictionary">Second Dictionary.</param>
        /// <param name="thirdDictionary">Third Dictionary.</param>
        public void SetsTheInitialValue(long chatId, Dictionary<long, string> firstDictionary, Dictionary<long, string> secondDictionary, Dictionary<long, bool> thirdDictionary)
        {
            if (!firstDictionary.ContainsKey(chatId))
            {
                firstDictionary[chatId] = "";
            }
            if (!secondDictionary.ContainsKey(chatId))
            {
                secondDictionary[chatId] = "";
            }
            if (!thirdDictionary.ContainsKey(chatId))
            {
                thirdDictionary[chatId] = true;
            }
        }

        /// <summary>
        /// Sets the value in the dictionary.
        /// </summary>
        /// <typeparam name="G">The data type that is the value for the dictionary.</typeparam>
        /// <param name="chatId">Chat id.</param>
        /// <param name="value">The value that we set in the dictionary.</param>
        /// <param name="dictionary">Dictionary in which data is stored.</param>
        public void SetDictionary<G>(long chatId, G value, Dictionary<long, G> dictionary)
        {
            if (dictionary.ContainsKey(chatId))
            {
                dictionary[chatId] = value;
            }
            else
            {
                dictionary.Add(chatId, value);
            }
        }   

        #region Overload GetDictionary

        /// <summary>
        /// Gets the dictionary element if it exists.
        /// </summary>
        /// <typeparam name="G">The type of data we will receive.</typeparam>
        /// <param name="chatId">Chat id.</param>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public G GetDictionary<G>(long chatId, Dictionary<long, G> dictionary)
        {
            if (dictionary.ContainsKey(chatId))
            {
                return dictionary[chatId];
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Gets the dictionary element if it exists.
        /// </summary>
        /// <typeparam name="G">The type of data we will receive.</typeparam>
        /// <param name="chatId">Chat id.</param>
        /// <param name="dictionary">Dictionary.</param>
        /// <param name="res">Result.</param>
        /// <returns></returns>
        public bool GetDictionary<G>(long chatId, Dictionary<long, G> dictionary, out G res)
        {
            bool isOk = false;
            if (dictionary.ContainsKey(chatId))
            {
                res = dictionary[chatId];
                isOk = true;
            }
            else
            {
                res = default;
            }
            return isOk;
        }

        #endregion

        public void Delete<G>(CallbackQuery callbackQuery, DbContext dbContext, string messageText) where G : ModelBase
        {
            string[] temp = callbackQuery.Data.Split(new char[] { ' ' });
            int id = Convert.ToInt32(temp[1]);

            new DataBaseControllerBase<G>(dbContext).Load().Where(per => per.ChatId == callbackQuery.Message.Chat.Id).Where(x => x.Id == id).ToList().ForEach(async per =>
            {
                if (new DataBaseControllerBase<G>(dbContext).Remove(per))
                    await botControllerBase.PrintMessage($"{messageText} видалено.", callbackQuery.Message.Chat.Id);
                else
                    await botControllerBase.PrintMessage($"{messageText} не видалено.", callbackQuery.Message.Chat.Id);
                return;
            });
        }

        public async void Display<G>(long chatId, string falseMessage, BotControllerBase.CallbackQueryCommands callbackQuery, DbContext dbContext) where G : ModelBase
        {
            var temps = new DataBaseControllerBase<G>(dbContext).Load().Where(x => x.ChatId == chatId).ToList();


            if (temps.Count != 0)
            {
                temps.ForEach(async p =>
                {
                    await botControllerBase.PrintInline(p.PrintData(), chatId, botControllerBase.SetupInLine(BotControllerBase.CallbackQueryCommands.Видалити.ToString(), $"{callbackQuery} {p.Id}"));
                });
            }
            else
            {
                await botControllerBase.PrintMessage(falseMessage, chatId);
            }
        }
    }
}
