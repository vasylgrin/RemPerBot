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
    public class PeriodControllerTests
    {
        //[TestMethod()]
        //public void AddTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void DisplayCurrentPeriodTest()
        //{
        //    Assert.Fail();
        //}

        [TestMethod()]
        public void SaveTest()
        {
            PeriodController periodController = new();

            long chatId = 111111111;
            int durationMenstruation = 7;
            int dirationCycle = 30;
            DateTime dateOfLastMenstruation = DateTime.Today.AddDays(-3);

            periodController.Save(chatId, out int id, durationMenstruation, dirationCycle, dateOfLastMenstruation);
            new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Load().Where(per=>per.Id == id).ToList().ForEach(per =>
            {
                Assert.AreEqual(chatId, per.ChatId);
                Assert.AreEqual(durationMenstruation, per.DurationMenstruation);
                Assert.AreEqual(dirationCycle, per.DurationСycle);
                Assert.AreEqual(dateOfLastMenstruation, per.DateOfLastMenstruation);
                new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Remove(per);
            });
        }

        //[TestMethod()]
        //public void CheckPeriodThreadTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void ChangeIsNotifyThreadTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void UpdateTest()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod()]
        //public void StartMenstruationThreadTest()
        //{
        //    Assert.Fail();
        //}
    }
}