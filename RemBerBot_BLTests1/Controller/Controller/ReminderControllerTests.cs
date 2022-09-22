using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySuperUniversalBot_BL.Controller;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySuperUniversalBot_BL.Controller.Tests
{
    [TestClass()]
    public class ReminderControllerTests
    {
        [TestMethod()]
        public void SaveTest()
        {
            ReminderController reminderController = new();
            var topic = Guid.NewGuid().ToString();
            var dateTime = DateTime.Now;
            long chatId = 111111111;

            reminderController.Save(chatId, out int id, topic, dateTime);
            new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).Load().Where(x=>x.Id == id).ToList().ForEach(x =>
            {
                Assert.AreEqual(x.ChatId, chatId);
                Assert.AreEqual(x.Topic, topic);
                Assert.AreEqual(x.DateTime, dateTime);
                new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).Remove(x);
            });
        }
    }
}