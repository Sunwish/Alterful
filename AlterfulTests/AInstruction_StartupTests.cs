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
            
            Assert.AreEqual(actual.Count, 3);
            Assert.AreEqual(actual[0], "item1");
            Assert.AreEqual(actual[1], "item2-f");
            Assert.AreEqual(actual[2], "item3-c-o");
        }

        [TestMethod()]
        public void GetStartupItemsTest()
        {
            string instruction = "item1 item2-f item3-c-o";
            List<StartupItem> actual = new AInstruction_Startup(instruction).GetStartupItems();

            Assert.AreEqual(actual.Count, 3);

            Assert.AreEqual(actual[0].StartupName, "item1");
            Assert.AreEqual(actual[1].StartupName, "item2");
            Assert.AreEqual(actual[2].StartupName, "item3");

            Assert.AreEqual(actual[0].suffixList, null);

            Assert.AreEqual(actual[1].suffixList.Count, 1);
            Assert.AreEqual(actual[1].suffixList[0], "f");

            Assert.AreEqual(actual[2].suffixList.Count, 2);
            Assert.AreEqual(actual[2].suffixList[0], "c");
            Assert.AreEqual(actual[2].suffixList[1], "o");
        }

        [TestMethod()]
        public void StartupNameSuffixesParseTest1()
        {
            string singleStartupItem = "item";
            StartupItem actual = AInstruction_Startup.StartupNameSuffixesParse(singleStartupItem);
            Assert.AreEqual(actual.StartupName, "item");
            Assert.AreEqual(actual.suffixList, null);
        }
        [TestMethod()]
        public void StartupNameSuffixesParseTest2()
        {
            string singleStartupItem = "item-o";
            StartupItem actual = AInstruction_Startup.StartupNameSuffixesParse(singleStartupItem);
            Assert.AreEqual(actual.StartupName, "item");
            Assert.AreEqual(actual.suffixList.Count, 1);
            Assert.AreEqual(actual.suffixList[0], "o");
        }
        [TestMethod()]
        public void StartupNameSuffixesParseTest3()
        {
            string singleStartupItem = "item-o-f";
            StartupItem actual = AInstruction_Startup.StartupNameSuffixesParse(singleStartupItem);
            Assert.AreEqual(actual.StartupName, "item");
            Assert.AreEqual(actual.suffixList.Count, 2);
            Assert.AreEqual(actual.suffixList[0], "o");
            Assert.AreEqual(actual.suffixList[1], "f");
        }


    }
}