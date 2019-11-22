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
    public class ACompletionTests
    {
        [TestMethod()]
        public void GetInstructionCompletionDepartTest()
        {
            InputAttribution ia = new InputAttribution
            {
                content = "@del chro aaa",
                caretPosition = 9,
            };
            string tl, t, tr;
            ACompletion.GetInstructionCompletionDepart(ia, out tl, out t, out tr);
            Assert.AreEqual("@del ", tl);
            Assert.AreEqual("chro", t);
            Assert.AreEqual(" aaa", tr);

            ia.content = "@haha"; ia.caretPosition = 3;
            ACompletion.GetInstructionCompletionDepart(ia, out tl, out t, out tr);
            Assert.AreEqual("", tl);
            Assert.AreEqual("@ha", t);
            Assert.AreEqual("ha", tr);
        }

        [TestMethod()]
        public void GetInstructionCompletionPartTypeTest()
        {
            Assert.AreEqual(InstructionType.STARTUP, ACompletion.GetInstructionCompletionPartType("chro"));
            Assert.AreEqual(InstructionType.MACRO, ACompletion.GetInstructionCompletionPartType("@ha"));
            Assert.AreEqual(InstructionType.STARTUP, ACompletion.GetInstructionCompletionPartType("#ha"));
            Assert.AreEqual(InstructionType.BUILDIN, ACompletion.GetInstructionCompletionPartType(".ha"));
        }
    }
}