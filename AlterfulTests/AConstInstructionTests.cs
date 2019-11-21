using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alterful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Tests
{
    [TestClass()]
    public class AConstInstructionTests
    {
        [TestMethod()]
        public void ConstInstructionFileNameParseTest()
        {
            ConstInstruction item = AConstInstruction.ConstInstructionFileNameParse("test(param1,param2)");
            Assert.AreEqual("test", item.constInstructionName);

            Assert.AreEqual(2, item.parameterList.Count);
            Assert.AreEqual("param1", item.parameterList[0]);
            Assert.AreEqual("param2", item.parameterList[1]);

            Assert.AreEqual(1, item.instructionLines.Count);
        }

        [TestMethod()]
        public void ExistTest()
        {
            Assert.AreEqual(false, AConstInstruction.Exist("test(p1,p2,p3)"));
            Assert.AreEqual(false, AConstInstruction.Exist("test1()"));
            Assert.AreEqual(true, AConstInstruction.Exist("test()"));
            Assert.AreEqual(true, AConstInstruction.Exist("test(p1,p2)"));
        }

        [TestMethod()]
        public void DeleteTest()
        {
            string fileName1 = AConstInstruction.Delete("test(p1,p2)");
            Assert.AreEqual("test(param1,param2)", fileName1);
            string fileName2 = AConstInstruction.Delete("test()");
            Assert.AreEqual("test()", fileName2);
        }

        [TestMethod()]
        public void ConstInstructionParameterParseTest()
        {
            ConstInstruction ci = new ConstInstruction
            {
                constInstructionName = "test",
                instructionLines = new List<string> {
                    "param1 param2 param3"
                },
                parameterList = new List<string>{
                    "param1",
                    "param2"
                }
            };
            List<string> valueList = new List<string>{
                "a", "b"
            };
            string parseAcutual = AConstInstruction.ConstInstructionParameterParse(ci, "param1 param2 param3", valueList);
            Assert.AreEqual("a b param3", parseAcutual);
        }

        [TestMethod()]
        public void GetFileNameFromConstInstructionTest()
        {
            ConstInstruction ci = new ConstInstruction
            {
                constInstructionName = "test",
                parameterList = new List<string> { "param1", "param2" }
            };
            string actual = AConstInstruction.GetFileNameFromConstInstruction(ci);
            Assert.AreEqual("test(param1,param2)", actual);
        }
    }
}