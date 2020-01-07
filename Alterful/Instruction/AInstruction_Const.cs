using Alterful.Functions;
using Alterful.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Instruction
{
    public class AInstruction_Const : AInstruction
    {
        public AInstruction_Const(string instruction) : base(instruction) { }

        /// <summary>
        /// 从宏指令中分离出常指令部分
        /// </summary>
        /// <param name="macroInstruction"></param>
        /// <returns></returns>
        public static string GetConstInstructionFromMacroInstruction(string macroInstruction)
        {
            return macroInstruction.Substring(macroInstruction.IndexOf(" ") + 1, macroInstruction.Length - macroInstruction.IndexOf(" ") - 1);
        }

        public static string GetCompletion(string part) => throw new NotImplementedException();

        /// <summary>
        /// 执行指令，启动失败的启动项在ReportInfo中查看
        /// </summary>
        /// <exception cref="ConstInstructionParameterParseException"></exception>
        public override string Execute()
        {
            ReportInfo.Clear();
            AHelper.InstructionHistory.Insert(0, Instruction);

            ConstInstruction ci = new ConstInstruction();
            if (AConstInstruction.GetConstInstructionFrame(Instruction, ref ci))
            {
                bool allRight = true;
                try
                {
                    foreach (string instructionLine in ci.instructionLines)
                    {
                        // Instruction here firstly need to be parse (const quote / parameter parse).
                        // Const quote parse.
                        string instructionLine_cqp = AConstQuote.ConstQuoteParse(instructionLine);
                        // Parameter parse
                        ConstInstruction instructionAttribute = AConstInstruction.ConstInstructionFileNameParse(Instruction, false);
                        string instructionLine_cpq_pp = AConstInstruction.ConstInstructionParameterParse(ci, instructionLine_cqp, instructionAttribute.parameterList);
                        // Execute
                        AInstruction.GetInstruction(instructionLine_cpq_pp).Execute();
                        if (reportType != ReportType.OK) allRight = false;
                    }
                }
                catch (Exception exception) { throw exception; }
                finally { if (!allRight) reportType = ReportType.WARNING; }
                return AInstruction.MSG_EXECUTE_SUCCESSFULLY;
            }
            else
            {
                reportType = ReportType.ERROR;
                throw new ConstInstructionNotFoundException(Instruction);
            }
        }
    }

}
