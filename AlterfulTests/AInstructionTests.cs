﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alterful.Functions;
using Alterful.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Functions.Tests
{
    [TestClass()]
    public class AInstructionTests
    {
        [TestMethod()]
        [DataRow("hello", InstructionType.STARTUP)]
        [DataRow(@"\hello", InstructionType.STARTUP)]
        [DataRow("@add", InstructionType.MACRO)]
        [DataRow("#get", InstructionType.STARTUP)]
        [DataRow(".paint", InstructionType.BUILDIN)]
        [DataRow(">ipconfig", InstructionType.CMD)]
        public void GetTypeTest(string instruction, InstructionType excepted)
        {
            Assert.AreEqual(excepted, AInstruction.GetType(instruction));
        }
    }
}