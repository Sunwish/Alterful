using Alterful.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alterful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Functions.Tests
{
    [TestClass()]
    public class AConstQuoteTests
    {
        [TestMethod()]
        public void GetConstQuoteMapConfigLinesTest()
        {
            string constQuoteMapConfigString = "testLine1\ntestLine2\ntestLine3";
            List<string> actual = AConstQuote.GetConstQuoteMapConfigLines(constQuoteMapConfigString);
            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual("testLine1", actual[0]);
            Assert.AreEqual("testLine2", actual[1]);
            Assert.AreEqual("testLine3", actual[2]);
        }

        [TestMethod()]
        public void ConstQuoteItemParseTest()
        {
            string constQuoteMapConfigSingleLine = "constQuoteName#constQuoteString";
            AConstQuote.ConstQuoteItem actual = AConstQuote.ConstQuoteItemParse(constQuoteMapConfigSingleLine);
            Assert.AreEqual("constQuoteName", actual.constQuoteName);
            Assert.AreEqual("constQuoteString", actual.constQuoteString);
        }
    }
}

namespace Alterful.Tests
{
    [TestClass()]
    public class AConstQuoteTests
    {
        [TestMethod()]
        [DataRow("test", "test")]
        [DataRow("#test", "CONSTQUOTE")]
        [DataRow("#test+#test", "CONSTQUOTECONSTQUOTE")]
        [DataRow("#test1+#test2+#test3", "CONSTQUOTECONSTQUOTECONSTQUOTE")]
        [DataRow("#test1+test2+#test3", "CONSTQUOTEtest2CONSTQUOTE")]
        [DataRow("test1+test2+#test3", "test1+test2CONSTQUOTE")]
        [DataRow("test1+#test2+test3", "test1CONSTQUOTEtest3")]
        [DataRow("#test1+test2+test3", "CONSTQUOTEtest2+test3")]
        public void ConstQuoteParseTest(string instruction, string expected)
        {
            Assert.AreEqual(expected, AConstQuote.ConstQuoteParse(instruction));
        }
    }
}