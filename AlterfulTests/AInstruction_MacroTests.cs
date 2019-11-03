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
    public class AInstruction_MacroTests
    {
        [TestMethod()]
        [DataRow("@new a.txt", AInstruction_Macro.MacroType.NEW)]
        [DataRow("@add #const instruction", AInstruction_Macro.MacroType.ADD)]
        [DataRow("@del #const", AInstruction_Macro.MacroType.DEL)]
        public void GetMacroTypeTest(string instruction, AInstruction_Macro.MacroType expected)
        {
            AInstruction_Macro ins = new AInstruction_Macro(instruction);
            Assert.AreEqual(expected, ins.GetMacroType());
        }

        [TestMethod()]
        public void GetMacroInstructionPartsListTest()
        {
            AInstruction_Macro ins = new AInstruction_Macro("@new a.txt");
            List<string> actual = ins.GetMacroInstructionPartsList();
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual("@new", actual[0]);
            Assert.AreEqual("a.txt", actual[1]);

            ins = new AInstruction_Macro("@add #const instruction");
            actual = ins.GetMacroInstructionPartsList();
            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual("@add", actual[0]);
            Assert.AreEqual("#const", actual[1]);
            Assert.AreEqual("instruction", actual[2]);
        }

        [TestMethod()]
        [DataRow("#constDemo", AInstruction_Macro.MacroAddType.CONST_QUOTE)]
        [DataRow("startupDemo", AInstruction_Macro.MacroAddType.STARTUP)]
        public void GetMacroAddTypeTest(string firstParamOfMacroAddInstruction, AInstruction_Macro.MacroAddType expected)
        {
            Assert.AreEqual(expected, AInstruction_Macro.GetMacroAddType(firstParamOfMacroAddInstruction));
        }

        [TestMethod()]
        [DataRow("#constDemo", AInstruction_Macro.MacroDelType.CONST_QUOTE)]
        [DataRow("startupDemo", AInstruction_Macro.MacroDelType.STARTUP)]
        public void GetMacroDelTypeTest(string paramOfMacroDelItemString, AInstruction_Macro.MacroDelType expected)
        {
            Assert.AreEqual(expected, AInstruction_Macro.GetMacroDelType(paramOfMacroDelItemString));
        }
    }
}