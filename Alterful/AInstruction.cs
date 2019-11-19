using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Alterful.Helper;
using System.Text.RegularExpressions;
using Alterful;
namespace Alterful.Functions
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
        public const string MSG_EXECUTE_SUCCESSFULLY = "Execute successfully.";
        public string Instruction { get; }
        public static List<string> ReportInfo { get; } = new List<string>();
        public static ReportType reportType = ReportType.OK;

        public enum ReportType { OK, WARNING, ERROR }

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
            if ((instruction.IndexOf("(") < instruction.IndexOf(")")) && instruction.IndexOf(")") != -1) return InstructionType.CONST;
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

    public class AInstruction_Startup : AInstruction
    {
        /// <summary>
        /// 指令中包含的启动项个数
        /// </summary>
        public int Count { get; }
        public AInstruction_Startup(string instruction) : base(instruction) { Count = instruction.Split(' ').Length; }

        /// <summary>
        /// 执行指令，启动失败的启动项在ReportInfo中查看
        /// </summary>
        public override string Execute()
        {
            int existCount = 0;
            ReportInfo.Clear();
            AHelper.InstructionHistory.Insert(0, Instruction);
            foreach (var item in GetStartupItems())
            {
                if (item.StartupName == "") continue;

                if (AFile.Exists(item.StartupName))
                {
                    existCount++;
                    if (item.SuffixList == null)
                    {
                        AFile.Launch(item.StartupName, item.StartupParameter);
                        continue;
                    }
                    foreach (var suffix in item.SuffixList)
                    {
                        switch (suffix)
                        {
                            case "f": AFile.ShowInExplorer(item.StartupName); break;
                            case "c": throw new NotImplementedException(); AFile.GetFullPath(item.StartupName);
                            case "o": AFile.Launch(item.StartupName, item.StartupParameter); break;
                            case "oa": AFile.Launch(item.StartupName, item.StartupParameter, true); break;
                            case "t": AFile.MoveToATemp(item.StartupName); break;
                        }
                    }
                }
                else
                {
                    ReportInfo.Add("Starup item [" + item.StartupName + "] is not exist.");
                }
            }
            reportType = ReportType.OK;
            return existCount!=0 ? AInstruction.MSG_EXECUTE_SUCCESSFULLY : "";
        }

        /// <summary>
        /// 获取指令包含的启动项文本列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetStartupItemStringList()
        {
            return new List<string>(Instruction.Split(' '));
        }

        /// <summary>
        /// 获取分离参数的启动项列表
        /// </summary>
        /// <param name="singleStartupItemString"></param>
        /// <returns></returns>
        public static string GetStartupItemParameterDepart(ref string singleStartupItemString)
        {
            int paramSymbolPosition = singleStartupItemString.IndexOf('/');
            if (-1 == paramSymbolPosition) return "";
            string param = singleStartupItemString.Substring(paramSymbolPosition + 1);
            singleStartupItemString = singleStartupItemString.Substring(0, paramSymbolPosition);
            return param;
        }

        /// <summary>
        /// 获取指令中包含的启动项
        /// </summary>
        /// <returns></returns>
        public List<StartupItem> GetStartupItems()
        {
            List<string> startupItemStringList = GetStartupItemStringList();
            List<StartupItem> startupItemList = new List<StartupItem>();
            for(int i = 0; i < startupItemStringList.Count; i++)
            {
                string singleItem = startupItemStringList[i];
                string param = GetStartupItemParameterDepart(ref singleItem);
                StartupItem item = StartupNameSuffixesParse(singleItem);
                item.StartupParameter = param;
                startupItemList.Add(item);
            }
            return startupItemList;
        }

        /// <summary>
        /// 启动项解析
        /// </summary>
        /// <param name="singleStartupItem">单个的启动项原文本，可调用 GetStartupItems() 方法获得</param>
        /// <returns></returns>
        public static StartupItem StartupNameSuffixesParse(string singleStartupItem)
        {
            if (singleStartupItem.IndexOf('-') == -1) return new StartupItem() { StartupName = singleStartupItem, SuffixList = null };

            StartupItem item = new StartupItem();
            List<string> suffixList = new List<string>(singleStartupItem.Split('-'));
            item.StartupName = suffixList[0];
            suffixList.RemoveAt(0);
            item.SuffixList = suffixList;

            return item;
        }
    }

    public struct ConstInstructionItem
    {
        public string ConstInstruction { get; set; }
        public List<string> ParameterList { get; set; }
    }

    public class UnknowMacroType : FormatException { public UnknowMacroType(string unknowType) : base(AInstruction_Macro.MSG_UNKNOW_MACRO_TYPE + " [" + unknowType + "].") { } }
    public class MacroFormatException : FormatException { public MacroFormatException() : base(AInstruction_Macro.MSG_MACRO_FORMAT_EXCEPTION) { } }
    public class AInstruction_Macro : AInstruction
    {
        public enum MacroType { ADD, NEW, DEL }
        public enum MacroAddType { STARTUP, CONST_QUOTE }
        public enum MacroDelType { STARTUP, CONST_QUOTE }
        public static string MSG_UNKNOW_MACRO_TYPE { get; } = "Unknow macro type";
        public static string MSG_MACRO_FORMAT_EXCEPTION { get; } = "Unknow macro instruction format.";
        public AInstruction_Macro(string instruction) : base(instruction) {
            /*try
            {
                GetMacroType();
            }
            catch (Exception)
            {
                reportType = ReportType.ERROR;
                throw;
            }*/
        }

        /// <summary>
        /// 获取宏指令类型
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MacroFormatException"></exception>
        /// <exception cref="UnknowMacroType"></exception>
        public MacroType GetMacroType()
        {
            List<string> macroInstructionParts = new List<string>(Instruction.Split(' '));
            if (macroInstructionParts.Count < 2) throw new MacroFormatException();
            string macroType = macroInstructionParts[0].Substring(1);
            switch (macroType)
            {
                case "add": return MacroType.ADD;
                case "new": return MacroType.NEW;
                case "del": return MacroType.DEL;
                default: throw new UnknowMacroType(macroType);
            }
        }

        /// <summary>
        /// 执行宏指令
        /// </summary>
        /// <exception cref="MacroFormatException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="AFile.StartupItemNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public override string Execute()
        {
            ReportInfo.Clear();
            AHelper.InstructionHistory.Insert(0, Instruction);
            try
            {
                switch (GetMacroType())
                {
                    case MacroType.ADD: ExecuteMacroAdd(); break;
                    case MacroType.DEL: ExecuteMacroDel(); break;
                    case MacroType.NEW: ExecuteMacroNew(); break;
                }
            }
            catch (Exception)
            {
                reportType = ReportType.ERROR;
                throw;
            }
            reportType = ReportType.OK;
            return MSG_EXECUTE_SUCCESSFULLY;
        }

        /// <summary>
        /// 取宏指令块列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetMacroInstructionPartsList()
        {
            return new List<string>(Instruction.Split(' '));
        }

        /// <summary>
        /// 取宏指令参数列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetMacroInstructionParametersList()
        {
            List<string> paramList = GetMacroInstructionPartsList();
            paramList.RemoveAt(0);
            return paramList;
        }

        /// <summary>
        /// 获取宏添加指令的添加类型
        /// </summary>
        /// <param name="firstParamOfMacroAddInstruction"></param>
        /// <returns></returns>
        public static MacroAddType GetMacroAddType(string firstParamOfMacroAddInstruction)
        {
            if (firstParamOfMacroAddInstruction[0] == '#') return MacroAddType.CONST_QUOTE;
            else return MacroAddType.STARTUP;
        }

        /// <summary>
        /// 获取宏删除指令的删除类型
        /// </summary>
        /// <param name="paramOfMacroDelItemString"></param>
        /// <returns></returns>
        public static MacroDelType GetMacroDelType(string paramOfMacroDelItemString)
        {
            if (paramOfMacroDelItemString[0] == '#') return MacroDelType.CONST_QUOTE;
            else return MacroDelType.STARTUP;
        }

        /// <summary>
        /// 执行宏新建指令
        /// </summary>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void ExecuteMacroNew()
        {
            List<string> macroInstructionParameters = GetMacroInstructionParametersList();
            foreach (string newFileName in macroInstructionParameters)
            {
                string fileName = AConstQuote.ConstQuoteParse(newFileName);
                using (StreamWriter streamWriter = new StreamWriter(AFile.ATEMP_PATH + @"\" + fileName, false)) { }
                AFile.LaunchTempFile(fileName);
            }
        }

        /// <summary>
        /// 获取（解析）常指令宏中的常指令项目
        /// </summary>
        /// <param name="instruction">指令</param>
        /// <returns></returns>
        public static ConstInstructionItem GetConstInstructionItem(string instruction)
        {
            ConstInstructionItem ciItem = new ConstInstructionItem() { ParameterList = new List<string>() };
            int startSymbol = instruction.IndexOf("(");
            int endSymbol = instruction.LastIndexOf(")");
            ciItem.ConstInstruction = instruction.Substring(instruction.IndexOf(" ") + 1, startSymbol - instruction.IndexOf(" ") - 1);
            string paramText = instruction.Substring(instruction.IndexOf("(") + 1, endSymbol - startSymbol - 1);
            foreach(string param in paramText.Split(','))
            {
                if ("" != param.Trim()) ciItem.ParameterList.Add(param.Trim());
            }
            return ciItem;
        }

        /// <summary>
        /// 执行宏添加指令
        /// </summary>
        /// <exception cref="MacroFormatException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        private void ExecuteMacroAdd()
        {
            // MacroAddType - Const Instruction
            if ((Instruction.IndexOf("(") < Instruction.IndexOf(")")) && Instruction.IndexOf(")") != -1) throw new Exception(AInstruction.ADD_CONST_INSTRUCTION);

            // MacroAddType - Startup / Const Quote
            List<string> macroInstructionParametersRaw = GetMacroInstructionParametersList();
            if (macroInstructionParametersRaw.Count < 2) throw new MacroFormatException();
            List<string> macroInstructionParameters = new List<string> { macroInstructionParametersRaw[0] };
            string secondParam = macroInstructionParametersRaw[1];
            for (int i = 2; i < macroInstructionParametersRaw.Count; i++)
            {
                secondParam += " " + macroInstructionParametersRaw[i];
            }
            macroInstructionParameters.Add(secondParam);

            MacroAddType addType = GetMacroAddType(macroInstructionParameters[0]);
            try
            {
                switch (addType)
                {
                    case MacroAddType.STARTUP: ExecuteMacroAddStartup(macroInstructionParameters); break;
                    case MacroAddType.CONST_QUOTE: ExecuteMacroAddConstQuote(macroInstructionParameters); break;
                }
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// 执行宏启动项添加指令
        /// </summary>
        /// <param name="macroInstructionParameters">宏添加指令参数列表</param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ConstQuoteParseError"></exception>
        private void ExecuteMacroAddStartup(List<string> macroInstructionParameters)
        {
            try
            {
                AFile.Add(AConstQuote.ConstQuoteParse(macroInstructionParameters[0]), AConstQuote.ConstQuoteParse(macroInstructionParameters[1]));
                reportType = ReportType.OK;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (ConstQuoteParseError)
            {
                throw;
            }
        }

        /// <summary>
        /// 执行宏添加常引用指令
        /// </summary>
        /// <param name="macroInstructionParameters"></param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ConstQuoteNameAlreadyExistsException"></exception>
        private void ExecuteMacroAddConstQuote(List<string> macroInstructionParameters)
        {
            try
            {
                if (macroInstructionParameters.Count != 2) throw new MacroFormatException();
                string constQuoteName = AConstQuote.ConstQuoteNamePull(macroInstructionParameters[0]);
                AConstQuote.Add(constQuoteName, macroInstructionParameters[1]);
            }
            catch (Exception){ throw; }
        }

        /// <summary>
        /// 执行宏删除指令
        /// </summary>
        /// <exception cref="MacroFormatException"></exception>
        /// <exception cref="AFile.StartupItemNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void ExecuteMacroDel()
        {
            // MacroDeleteType - Const Instruction
            if ((Instruction.IndexOf("(") < Instruction.IndexOf(")")) && Instruction.IndexOf(")") != -1)
            {
                try { AConstInstruction.Delete(AInstruction_Const.GetConstInstructionFromMacroInstruction(Instruction)); return; }
                catch (Exception exception) { throw exception; }
            }

            List<string> macroInstructionParametersRaw = GetMacroInstructionParametersList();
            if (macroInstructionParametersRaw.Count < 1) throw new MacroFormatException();
            foreach(string delItemString in macroInstructionParametersRaw)
            {
                switch(GetMacroDelType(delItemString))
                {
                    case MacroDelType.STARTUP: AFile.Delete(delItemString); break;
                    case MacroDelType.CONST_QUOTE: ExecuteMacroDelConstQuote(delItemString);  break;
                }
            }
        }

        private void ExecuteMacroDelConstQuote(string delConstQuoteName)
        {
            AConstQuote.Delete(AConstQuote.ConstQuoteNamePull(delConstQuoteName));
        }
    }

    public class AInstruction_CMD : AInstruction
    {
        public AInstruction_CMD(string instruction) : base(instruction) {}

        public override string Execute()
        {
            ReportInfo.Clear();
            AHelper.InstructionHistory.Insert(0, Instruction);
            try
            {
                string output = AHelper.ExecuteCommand(Instruction.Substring(1).Trim());
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

        /// <summary>
        /// 执行指令，启动失败的启动项在ReportInfo中查看
        /// </summary>
        public override string Execute()
        {
            ReportInfo.Clear();
            AHelper.InstructionHistory.Insert(0, Instruction);

            ConstInstruction ci = new ConstInstruction();
            if(AConstInstruction.GetConstInstructionFrame(Instruction, ref ci))
            {
                bool allRight = true;
                foreach(string instructionLine in ci.instructionLines)
                {
                    AInstruction.GetInstruction(instructionLine).Execute();
                    if (reportType != ReportType.OK) allRight = false;
                }
                if(!allRight) reportType = ReportType.WARNING;
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
