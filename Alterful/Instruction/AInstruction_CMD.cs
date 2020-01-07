using Alterful.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Instruction
{
    public class UnknowMacroType : FormatException { public UnknowMacroType(string unknowType) : base(AInstruction_Macro.MSG_UNKNOW_MACRO_TYPE + " [" + unknowType + "].") { } }
    public class MacroFormatException : FormatException { public MacroFormatException() : base(AInstruction_Macro.MSG_MACRO_FORMAT_EXCEPTION) { } }

    public class AInstruction_CMD : AInstruction
    {
        public AInstruction_CMD(string instruction) : base(instruction) { }

        public static string GetCompletion(string part) => "";

        public override string Execute()
        {
            ReportInfo.Clear();
            AHelper.InstructionHistory.Insert(0, Instruction);
            try
            {
                string output = AHelper.ExecuteCommand(Instruction.Substring(1).Trim());

                // cd detect
                List<string> cmdParts = new List<string>(Instruction.Split(' '));
                cmdParts.RemoveAt(0); // Remove symbol '>'
                if (cmdParts.Count > 1 && "cd" == cmdParts[0])
                    System.IO.Directory.SetCurrentDirectory(cmdParts[1]);

                reportType = ReportType.OK;
                return output;
            }
            catch (Exception exception)
            {
                reportType = ReportType.ERROR;
                return exception.Message;
            }
        }
    }

}
