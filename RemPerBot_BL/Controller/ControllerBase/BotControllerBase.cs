﻿using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using RemBerBot_BL.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MySuperUniversalBot_BL.Controller
{
    public class BotControllerBase
    {
        #region Variables

        TelegramBotClient botClient = new("5268015233:AAFtYMakBaqz-SvLgrmN14IByvkLTP2-404");
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
        public enum CallbackQueryCommands
        {
            deletePeriod,
            deleteReminder,
            deleteNotifyTheUser,
            Видалити,
            saveReminder,
            savePeriod,
            Зберегти,
            sendInviteNotifyTheUser,
            Запросити,
            confirmInvite,
            Підтвердити,
            tutorialReminder,
            tutorialPeriod,
            tutorialNotifyTheUser,
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

        /// <summary>
        /// Enumeration for app navigation.
        /// </summary>
        public enum NavigationEnum
        {
            ReminderMenu = 1,
            AddReminder,
            StartPeriodMenu,
            PeriodMenu,
            addPeriod,
            addNotifyTheUser,
        }

        /// <summary>
        /// Enumeration to add a reminder.
        /// </summary>
        public enum AddReminderEnum
        {
            empty,
            addReminder,
            addReminderTopic,
            addReminderDateTime
        }

        /// <summary>
        /// Enumeration to add a period.
        /// </summary>
        public enum AddPeriodEnum
        {
            empty,
            addPeriod,
            addPeriodCycle,
            addPeriodMenstruation,
            addPeriodDateOfLastMenstruationMenstruation
        }

        /// <summary>
        /// Enumeration to add a notifyTheUser.
        /// </summary>
        public enum AddNotifyTheUserEnum
        {
            empty,
            addNotifyTheUser,
            addNotifyTheUserChatId,
            addNotifyTheUserName
        }


        #endregion

        /// <summary>
        /// User keyboard output.
        /// </summary>
        /// <param name="messageText">Message text.</param>
        /// <param name="chatId">Chat id.</param>
        /// <param name="replyKeyboardMarkup">Keyboard for output.</param>
        /// <param name="cancellationToken">Token.</param>
        public async Task PrintKeyboard(string messageText, long? chatId, ReplyKeyboardMarkup replyKeyboardMarkup, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken);
            Task.Delay(500).Wait();

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Вивід inline кнопок.
        /// </summary>
        /// <param name="messageText">Повідомлення.</param>
        /// <param name="chatId">Id чату.</param>
        /// <param name="inlineKeyboardMarkup">Вид inline кнопок.</param>
        /// <param name="cancellationToken"></param>
        public async Task PrintInline(string messageText, long? chatId, InlineKeyboardMarkup inlineKeyboardMarkup)
        {
            await botClient.SendChatActionAsync(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken);
            Task.Delay(500).Wait();

            await botClient.SendTextMessageAsync(
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
        public InlineKeyboardMarkup SetupInLine(string button1, string callBackData)
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

            await botClient.SendTextMessageAsync(
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
        public async Task PrintMessage(string messageText, long chatId, int typing)
        {
            await botClient.SendChatActionAsync(chatId, Telegram.Bot.Types.Enums.ChatAction.Typing, cancellationToken);
            Task.Delay(typing).Wait();

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                cancellationToken: cancellationToken);
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
