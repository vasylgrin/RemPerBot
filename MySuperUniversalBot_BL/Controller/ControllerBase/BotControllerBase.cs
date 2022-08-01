using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MySuperUniversalBot_BL.Controller
{
    public class BotControllerBase
    {
        #region Variables

        TelegramBotClient botClient = new("5268015233:AAFtYMakBaqz-SvLgrmN14IByvkLTP2-404");
        long chatId;
        CancellationToken cancellationToken;

        #endregion

        #region Enum Commands

        /// <summary>
        /// General enumeration.
        /// </summary>
        protected enum GeneralCommands
        {
            myId,
            Назад,
        }

        /// <summary>
        /// Enumeration for callbacks.
        /// </summary>
        protected enum CallbackQueryCommands
        {
            deletePeriod,
            deleteReminder,
            deleteNotifyTheUser,
            Видалити,
        }

        /// <summary>
        /// Enumeration for ReminderCommands.
        /// </summary>
        protected enum ReminderCommands
        {
            Нагадування,
            Додати,
            Переглянути,
        }

        /// <summary>
        /// Enumeration for PeriodCommands.
        /// </summary>
        protected enum PeriodCommands
        {
            Період,
            Перiод,
            Створити,
            Проглянути,
            Розпочались,
        }

        /// <summary>
        /// Enumeration for NotifyTheUserCommands.
        /// </summary>
        protected enum NotifyTheUserCommands
        {
            Продивитись,
            Добавити,
            Користувачі,
        }

        #endregion

        /// <summary>
        /// We get the Сhat ID and CancellationToken.
        /// </summary>
        /// <param name="chatID">Chat id.</param>
        /// <param name="_cancellationToken">Token.</param>
        public void vBotControllerBase(long chatID,CancellationToken _cancellationToken)
        {
            chatId = chatID;
            cancellationToken = _cancellationToken;
        }

        /// <summary>
        /// User keyboard output.
        /// </summary>
        /// <param name="messageText">Message text.</param>
        /// <param name="chatId">Chat id.</param>
        /// <param name="replyKeyboardMarkup">Keyboard for output.</param>
        /// <param name="cancellationToken">Token.</param>
        public async Task PrintKeyboard(string messageText, long? chatId, ReplyKeyboardMarkup replyKeyboardMarkup,CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken);
            Task.Delay(500).Wait();
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes the keyboard from the user.
        /// </summary>
        /// <param name="messageText">Message text.</param>
        /// <param name="chatId">Chat id.</param>
        /// <param name="cancellationToken">Token.</param>
        public async void RemoveKeyboard(string messageText, long? chatId, CancellationToken cancellationToken)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }      

        /// <summary>
        /// Вивід inline кнопок.
        /// </summary>
        /// <param name="messageText">Повідомлення.</param>
        /// <param name="chatId">Id чату.</param>
        /// <param name="inlineKeyboardMarkup">Вид inline кнопок.</param>
        /// <param name="cancellationToken"></param>
        public async Task PrintInline(string messageText, long? chatId, InlineKeyboardMarkup inlineKeyboardMarkup, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken);
            Task.Delay(500).Wait();

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Keyboard settings and output for the user.
        /// </summary>
        /// <param name="button1">First button.</param>
        /// <returns></returns>
        public InlineKeyboardMarkup SetupInLine(string button1,string callBackData)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                // first row
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: button1, callbackData: callBackData),
                }
            });
            return inlineKeyboard;
        }

        #region Method overload PrintMessage     

        /// <summary>
        /// Displays a message to the user.
        /// </summary>
        /// <param name="messageText">Message.</param>
        /// <param name="chatId">Id chat/</param>
        public async Task PrintMessage(string messageText, long chatId)
        {
            await botClient.SendChatActionAsync(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing);
            Task.Delay(500).Wait();

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Displays a message to the user.
        /// </summary>
        /// <param name="messageText">Message.</param>
        /// <param name="chatId">Id chat/</param>
        /// <param name="token">Token.</param>
        public async void PrintMessage(string messageText, long chatId, int typing, CancellationToken token)
        {
            await botClient.SendChatActionAsync(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing, token);
            Task.Delay(typing).Wait();

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                cancellationToken: token);
        }

        #endregion

        #region Method overload SetupKeyboard

        /// <summary>
        /// Keyboard settings and output for the user.
        /// </summary>
        /// <param name="button1">First button.</param>
        /// <returns></returns>
        public ReplyKeyboardMarkup SetupKeyboard(string button1)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[]{ button1 }
                    })
            { ResizeKeyboard = true };
            return replyKeyboardMarkup;
        }

        /// <summary>
        /// Keyboard settings and output for the user.
        /// </summary>
        /// <param name="buttonName1">First button.</param>
        /// <param name="buttonName2">Second button.</param>
        /// <returns></returns>
        public ReplyKeyboardMarkup SetupKeyboard(string buttonName1, string buttonName2)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[]{ buttonName1, buttonName2}
                    })
            { ResizeKeyboard = true };
            return replyKeyboardMarkup;
        }

        /// <summary>
        /// Keyboard settings and output for the user.
        /// </summary>
        /// <param name="buttonName1">First button.</param>
        /// <param name="buttonName2">Second button.</param>
        /// <param name="buttonName3">Third button.</param>
        /// <returns></returns>
        public ReplyKeyboardMarkup SetupKeyboard(string buttonName1, string buttonName2, string buttonName3)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[]{buttonName1, buttonName2},
                        new KeyboardButton[]{ buttonName3 }

                    })
            { ResizeKeyboard = true };
            return replyKeyboardMarkup;
        }

        /// <summary>
        /// Keyboard settings and output for the user.
        /// </summary>
        /// <param name="buttonName1">First button.</param>
        /// <param name="buttonName2">Second button.</param>
        /// <param name="buttonName3">Third button.</param>
        /// <param name="buttonName4">Fourth button.</param>
        /// <returns></returns>
        public ReplyKeyboardMarkup SetupKeyboard(string buttonName1, string buttonName2, string buttonName3, string buttonName4)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[]{ buttonName1, buttonName2, buttonName3 },
                        new KeyboardButton[]{ buttonName4 }
                    })
            { ResizeKeyboard = true };
            return replyKeyboardMarkup;
        }
        #endregion
    }
}
