using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Alterful.Helper;
using System.Text.RegularExpressions;
using Alterful;
using Alterful.Functions;
namespace Alterful.Instruction
{
    public enum InstructionType
    {
        STARTUP, MACRO, CONST, BUILDIN, CMD
    }
    class InvalidInstructionException : FormatException { }
    class ConstQuoteParseError : FormatException{ }
    public abstract class AInstruction
    {
        public const char SYMBOL_MACRO = '@';
        public const char SYMBOL_CONST = '#';
        public const char SYMBOL_BUILDIN = '.';
        public const char SYMBOL_CONST_ADD = '+';
        public const char SYMBOL_CONST_CMD = '>';
        public const string ADD_CONST_INSTRUCTION = "ADD_CONST_INSTRUCTION";
        public const string UPDATE_INSTRUCTION = "UPDATE_INSTRUCTION";
        //public const string MSG_EXECUTE_SUCCESSFULLY = "执行成功。";
        public const string MSG_EXECUTE_SUCCESSFULLY = "Execute successfully.";
        public string Instruction { get; }
        public static List<string> ReportInfo { get; } = new List<string>();
        public static ReportType reportType = ReportType.OK;

        public enum ReportType { OK, WARNING, ERROR, NONE }

        protected AInstruction(string instruction)
        {
            Instruction = instruction;
        }

        /// <summary>
        /// 获取指令类型
        /// </summary>
        /// <param name="instruction">欲获取类型的指令</param>
        /// <returns></returns>
        public static InstructionType GetType(string instruction)
        {
            if (0 == instruction.Length) return InstructionType.STARTUP;
            switch (instruction[0])
            {
                case SYMBOL_MACRO: return InstructionType.MACRO;
                case SYMBOL_BUILDIN: return InstructionType.BUILDIN;
                case SYMBOL_CONST_CMD: return InstructionType.CMD;
                case SYMBOL_CONST:
                    if (instruction[instruction.Length - 1] == ')') // Const Function
                        return InstructionType.CONST;
                    else // Startup Instrcution with const quote
                        return InstructionType.STARTUP;
            }
            if ((instruction.IndexOf("(") < instruction.IndexOf(")")) && instruction.IndexOf(")") != -1 && instruction.IndexOf(")") == instruction.Length - 1) return InstructionType.CONST;
            else return InstructionType.STARTUP;
        }

        /// <summary>
        /// 获取可执行的Alterful指令
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public static AInstruction GetInstruction(string instruction)
        {
            switch (GetType(instruction))
            {
                case InstructionType.MACRO: return new AInstruction_Macro(instruction);
                case InstructionType.STARTUP: return new AInstruction_Startup(AConstQuote.ConstQuoteParse(instruction));
                case InstructionType.CONST: return new AInstruction_Const(instruction);
                case InstructionType.CMD: return new AInstruction_CMD(instruction);
                default: return new AInstruction_Startup(AConstQuote.ConstQuoteParse(instruction));
            }
        }

        /// <summary>
        /// 指令有效性检查
        /// </summary>
        /// <returns></returns>
        // public abstract bool Check();
        
        /// <summary>
        /// 执行指令
        /// </summary>
        /// <exception cref="AFile.StartupItemNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public abstract string Execute();
    }

    public struct StartupItem
    {
        public string StartupName { get; set; }
        public string StartupParameter { get; set; }
        public List<string> SuffixList { get; set; }
    }

    public struct ConstInstructionItem
    {
        public string ConstInstruction { get; set; }
        public List<string> ParameterList { get; set; }
    }


}
