using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alterful.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Functions.Tests
{
    [TestClass()]
    public class AInstruction_StartupTests
    {
        [TestMethod()]
        public void GetStartupItemStringListTest()
        {
            string instruction = "item1 item2-f item3-c-o";
            List<string> actual = new AInstruction_Startup(instruction).GetStartupItemStringList();

            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual("item1", actual[0]);
            Assert.AreEqual("item2-f", actual[1]);
            Assert.AreEqual("item3-c-o", actual[2]);
        }

        [TestMethod()]
        public void GetStartupItemsTest()
        {
            string instruction = "item1 item2-f item3-c-o item4/param item5-f/param item6/param-f";
            List<StartupItem> actual = new AInstruction_Startup(instruction).GetStartupItems();

            Assert.AreEqual(6, actual.Count);

            Assert.AreEqual("item1", actual[0].StartupName);
            Assert.AreEqual("item2", actual[1].StartupName);
            Assert.AreEqual("item3", actual[2].StartupName);
            Assert.AreEqual("item4", actual[3].StartupName);
            Assert.AreEqual("item5", actual[4].StartupName);
            Assert.AreEqual("item6", actual[5].StartupName);

            Assert.AreEqual(null, actual[0].SuffixList);
            Assert.AreEqual("", actual[0].StartupParameter);

            Assert.AreEqual(1, actual[1].SuffixList.Count);
            Assert.AreEqual("f", actual[1].SuffixList[0]);
            Assert.AreEqual("", actual[1].StartupParameter);

            Assert.AreEqual(2, actual[2].SuffixList.Count);
            Assert.AreEqual("c", actual[2].SuffixList[0]);
            Assert.AreEqual("o", actual[2].SuffixList[1]);
            Assert.AreEqual("", actual[2].StartupParameter);

            Assert.AreEqual(null, actual[3].SuffixList);
            Assert.AreEqual("param", actual[3].StartupParameter);

            Assert.AreEqual(1, actual[4].SuffixList.Count);
            Assert.AreEqual("f", actual[4].SuffixList[0]);
            Assert.AreEqual("param", actual[4].StartupParameter);

            Assert.AreEqual(null, actual[5].SuffixList);
            Assert.AreEqual("param-f", actual[5].StartupParameter);
        }

        [TestMethod()]
        public void StartupNameSuffixesParseTest1()
        {
            string singleStartupItem = "item";
            StartupItem actual = AInstruction_Startup.StartupNameSuffixesParse(singleStartupItem);
            Assert.AreEqual("item", actual.StartupName);
            Assert.AreEqual(null, actual.SuffixList);
        }
        [TestMethod()]
        public void StartupNameSuffixesParseTest2()
        {
            string singleStartupItem = "item-o";
            StartupItem actual = AInstruction_Startup.StartupNameSuffixesParse(singleStartupItem);
            Assert.AreEqual("item", actual.StartupName);
            Assert.AreEqual(1, actual.SuffixList.Count);
            Assert.AreEqual("o", actual.SuffixList[0]);
        }
        [TestMethod()]
        public void StartupNameSuffixesParseTest3()
        {
            string singleStartupItem = "item-o-f";
            StartupItem actual = AInstruction_Startup.StartupNameSuffixesParse(singleStartupItem);
            Assert.AreEqual("item", actual.StartupName);
            Assert.AreEqual(2, actual.SuffixList.Count);
            Assert.AreEqual("o", actual.SuffixList[0]);
            Assert.AreEqual("f", actual.SuffixList[1]);
        }

        [TestMethod()]
        public void GetStartupItemParameterDepartTest()
        {
            string singleStartupItemString = "chrome-f/alterful.com";
            string param = AInstruction_Startup.GetStartupItemParameterDepart(ref singleStartupItemString);
            Assert.AreEqual("chrome-f", singleStartupItemString);
            Assert.AreEqual("alterful.com", param);
        }
    }
}