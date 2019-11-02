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
    public class AFileTests
    {
        private static string UNITTEST_DIRECTORY_APATH = AFile.APATH_PATH;

        [TestMethod()]
        [DataRow(@"g", @"E:\Apath", @"E:\Apath\g.lnk")]
        public void GetFullPathOfShortcutFileTest1(string startupName, string Apath, string expected)
        {
            Assert.AreEqual(expected, AFile.GetFullPathOfShortcutFile(startupName, Apath));
        }

        [TestMethod()]
        [DataRow(@"\g", @"E:\Apath\", @"E:\Apath\g.lnk")]
        public void GetFullPathOfShortcutFileTest2(string startupName, string Apath, string expected)
        {
            Assert.AreEqual(expected, AFile.GetFullPathOfShortcutFile(startupName, Apath));
        }

        [TestMethod()]
        [DataRow("g", @"E:\Apath\", @"E:\Apath\g.lnk")]
        public void GetFullPathOfShortcutFileTest3(string startupName, string Apath, string expected)
        {
            Assert.AreEqual(expected, AFile.GetFullPathOfShortcutFile(startupName, Apath));
        }

        [TestMethod()]
        [DataRow(@"\g", @"E:\Apath", @"E:\Apath\g.lnk")]
        public void GetFullPathOfShortcutFileTest4(string startupName, string Apath, string expected)
        {
            Assert.AreEqual(expected, AFile.GetFullPathOfShortcutFile(startupName, Apath));
        }

        [TestMethod()]
        public void GetTargetPathOfShortcutFileTest()
        {
            string lnkFilePath = AFile.GetFullPathOfShortcutFile(@"\startupItemTest", UNITTEST_DIRECTORY_APATH);
            string expected = @"C:\";
            string actual = AFile.GetTargetPathOfShortcutFile(lnkFilePath);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DataRow(@"startupItemTest", @"C:\")]
        public void GetFullPathTest(string startupName, string expected)
        {
            Assert.AreEqual(AFile.GetFullPath(startupName), expected);
        }

        [TestMethod()]
        [DataRow(@"startupItemTest", true)]
        public void ExistsTest(string startupName, bool expected)
        {
            Assert.AreEqual(AFile.Exists(startupName), expected);
        }
    }
}