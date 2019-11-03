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
            Assert.AreEqual(ins.GetMacroType(), expected);
        }

        [TestMethod()]
        public void GetMacroInstructionPartsListTest()
        {
            AInstruction_Macro ins = new AInstruction_Macro("@new a.txt");
            List<string> actual = ins.GetMacroInstructionPartsList();
            Assert.AreEqual(actual.Count, 2);
            Assert.AreEqual(actual[0], "@new");
            Assert.AreEqual(actual[1], "a.txt");

            ins = new AInstruction_Macro("@add #const instruction");
            actual = ins.GetMacroInstructionPartsList();
            Assert.AreEqual(actual.Count, 3);
            Assert.AreEqual(actual[0], "@add");
            Assert.AreEqual(actual[1], "#const");
            Assert.AreEqual(actual[2], "instruction");
        }

        [TestMethod()]
        [DataRow("#constDemo", AInstruction_Macro.MacroAddType.CONST_QUOTE)]
        [DataRow("startupDemo", AInstruction_Macro.MacroAddType.STARTUP)]
        public void GetMacroAddTypeTest(string firstParamOfMacroAddInstruction, AInstruction_Macro.MacroAddType expected)
        {
            Assert.AreEqual(AInstruction_Macro.GetMacroAddType(firstParamOfMacroAddInstruction), expected);
        }
    }
}