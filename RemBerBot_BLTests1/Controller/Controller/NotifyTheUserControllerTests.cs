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
    public class NotifyTheUserControllerTests
    {
        [TestMethod()]
        public void SaveTest()
        {
            NotifyTheUserController notifyTheUserController = new();
            long chatId = 111111111;
            long chatIdAdded = 222222222;
            string username = Guid.NewGuid().ToString();

            PeriodController periodController = new();
            int durationMenstruation = 7;
            int dirationCycle = 30;
            DateTime dateOfLastMenstruation = DateTime.Today.AddDays(-3);

            periodController.Save(chatId, out int periodId, durationMenstruation, dirationCycle, dateOfLastMenstruation);
            notifyTheUserController.Save(chatId, out int id, chatIdAdded, username);
            
            new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).Load().Where(ntu=>ntu.Id == id).ToList().ForEach(ntu=>
            {
                Assert.AreEqual(chatId, ntu.ChatId);
                Assert.AreEqual(chatIdAdded, ntu.ChatIdAdded);
                Assert.AreEqual(username, ntu.Name);
                new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).Remove(ntu);
            });
        }

        //[TestMethod()]
        //public void SendInviteTest()
        //{
        //    Assert.Fail();
        //}
    }
}