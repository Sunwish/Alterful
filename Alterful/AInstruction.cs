using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Alterful.Helper;
using System.Text.RegularExpressions;

namespace Alterful.Functions
{
    public enum InstructionType
    {
        STARTUP, MACRO, CONST, BUILDIN
    }
    class ConstQuoteParseError : FormatException{ }
    public abstract class AInstruction
    {
        public const char SYMBOL_MACRO = '@';
        public const char SYMBOL_CONST = '#';
        public const char SYMBOL_BUILDIN = '.';
        public const char SYMBOL_CONST_ADD = '+';
        public string Instruction { get; }
        public List<string> ReportInfo { get; } = new List<string>();

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
            switch (instruction[0])
            {
                case SYMBOL_MACRO: return InstructionType.MACRO;
                case SYMBOL_CONST: return InstructionType.CONST;
                case SYMBOL_BUILDIN: return InstructionType.BUILDIN;
            }
            return InstructionType.STARTUP;
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
                case InstructionType.STARTUP: return new AInstruction_Startup(instruction);
                case InstructionType.CONST: throw new NotImplementedException();
                default: return new AInstruction_Startup(instruction);
            }
        }

        /// <summary>
        /// 执行指令
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// 常引用解析
        /// </summary>
        /// <param name="instructionString">解析前的指令</param>
        /// <returns>返回解析后的指令</returns>
        /// <exception cref="ConstQuoteParseError"></exception>
        public static string ConstQuoteParse(string instructionString)
        {
            string resultString = instructionString;
            int constQuoteSymbolPosition = instructionString.IndexOf(AInstruction.SYMBOL_CONST);
            int constQuoteEndPosition = instructionString.Length - 1;
            while (constQuoteSymbolPosition != -1)
            {
                int endPosition;
                int constIndex = instructionString.Substring(constQuoteSymbolPosition + 1).IndexOf(AInstruction.SYMBOL_CONST);
                int constAddIndex = instructionString.Substring(constQuoteSymbolPosition + 1).IndexOf(AInstruction.SYMBOL_CONST_ADD);
                if (-1 == constIndex) endPosition = constQuoteSymbolPosition + constAddIndex + 1;
                else if(-1 == constAddIndex) endPosition = constQuoteSymbolPosition + constIndex + 1;
                else endPosition = constQuoteSymbolPosition + Math.Min(constIndex, constAddIndex) + 1;

                if (endPosition != constQuoteSymbolPosition && endPosition != constQuoteEndPosition)
                {
                    constQuoteEndPosition = endPosition;
                    if(constQuoteSymbolPosition != 0)
                    {
                        if (instructionString[constQuoteSymbolPosition - 1] == AInstruction.SYMBOL_CONST_ADD)
                        {
                            int removeAddStuffix = 0;
                            if (instructionString.Length > constQuoteEndPosition + 1 && instructionString[constQuoteEndPosition + 1] != AInstruction.SYMBOL_CONST) removeAddStuffix = 1;
                            var regex = new Regex(Regex.Escape(instructionString.Substring(constQuoteSymbolPosition - 1, constQuoteEndPosition - constQuoteSymbolPosition + 1 + removeAddStuffix)));
                            resultString = regex.Replace(resultString, "CONSTQUOTE", 1);
                        }
                        else throw new ConstQuoteParseError();
                    }
                    else
                    {
                        int removeAddStuffix = 0;
                        if (instructionString.Length > constQuoteEndPosition + 1 && instructionString[constQuoteEndPosition + 1] != AInstruction.SYMBOL_CONST) removeAddStuffix = 1;
                        var regex = new Regex(Regex.Escape(instructionString.Substring(constQuoteSymbolPosition, constQuoteEndPosition - constQuoteSymbolPosition + removeAddStuffix)));
                        resultString = regex.Replace(resultString, "CONSTQUOTE", 1);
                    }

                    int moveForward = instructionString.Substring(constQuoteEndPosition + 1).IndexOf(AInstruction.SYMBOL_CONST) + 1;
                    if (0 == moveForward) break;
                    constQuoteSymbolPosition = constQuoteEndPosition + moveForward;
                    constQuoteEndPosition = instructionString.Length - 1;
                }
                else
                {
                    int includeBackChar = constQuoteSymbolPosition != 0 && instructionString[constQuoteSymbolPosition - 1] == AInstruction.SYMBOL_CONST_ADD ? -1 : 0;
                    resultString = resultString.Replace(instructionString.Substring(constQuoteSymbolPosition + includeBackChar, constQuoteEndPosition - constQuoteSymbolPosition + 1 - includeBackChar), "CONSTQUOTE");
                    break;
                }
            }
            return resultString;
        }
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
        public override void Execute()
        {
            ReportInfo.Clear();
            foreach (var item in GetStartupItems())
            {
                if (AFile.Exists(item.StartupName))
                {
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
                    ReportInfo.Add(item.StartupName);
                }
            }
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

    public class UnknowMacroType : FormatException { }
    public class MacroFormatException : FormatException { }
    public class AInstruction_Macro : AInstruction
    {
        public enum MacroType { ADD, NEW, DEL }
        public enum MacroAddType { STARTUP, CONST_QUOTE }
        public enum MacroDelType { STARTUP, CONST_QUOTE }

        public AInstruction_Macro(string instruction) : base(instruction) { GetMacroType(); }

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

            switch (macroInstructionParts[0].Substring(1))
            {
                case "add": return MacroType.ADD;
                case "new": return MacroType.NEW;
                case "del": return MacroType.DEL;
                default: throw new UnknowMacroType();
            }
        }

        /// <summary>
        /// 执行宏指令
        /// </summary>
        /// <exception cref="MacroFormatException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="AFile.StartupItemNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public override void Execute()
        {
            switch (GetMacroType())
            {
                case MacroType.ADD: ExecuteMacroAdd(); break;
                case MacroType.DEL: ExecuteMacroDel(); break;
                case MacroType.NEW: ExecuteMacroNew(); break;
            }
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
                using (StreamWriter streamWriter = new StreamWriter(AFile.ATEMP_PATH + @"\" + newFileName, false)) { }
                AFile.LaunchTempFile(newFileName);
            }
        }

        /// <summary>
        /// 执行宏添加指令
        /// </summary>
        /// <exception cref="MacroFormatException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        private void ExecuteMacroAdd()
        {
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
            switch (addType)
            {
                case MacroAddType.STARTUP: ExecuteMacroAddStartup(macroInstructionParameters); break;
                case MacroAddType.CONST_QUOTE: ExecuteMacroAddConstQuote(); break;
            }
        }

        /// <summary>
        /// 执行宏启动项添加指令
        /// </summary>
        /// <param name="macroInstructionParameters">宏添加指令参数列表</param>
        /// <exception cref="FileNotFoundException"></exception>
        private void ExecuteMacroAddStartup(List<string> macroInstructionParameters)
        {
            AFile.Add(macroInstructionParameters[0], macroInstructionParameters[1]);
        }

        private void ExecuteMacroAddConstQuote()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 执行宏删除指令
        /// </summary>
        /// <exception cref="MacroFormatException"></exception>
        /// <exception cref="AFile.StartupItemNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void ExecuteMacroDel()
        {
            List<string> macroInstructionParametersRaw = GetMacroInstructionParametersList();
            if (macroInstructionParametersRaw.Count < 1) throw new MacroFormatException();
            foreach(string delItemString in macroInstructionParametersRaw)
            {
                switch(GetMacroDelType(delItemString))
                {
                    case MacroDelType.STARTUP: AFile.Delete(delItemString); break;
                    case MacroDelType.CONST_QUOTE: ExecuteMacroDelConstQuote();  break;
                }
            }
        }

        private void ExecuteMacroDelConstQuote()
        {
            throw new NotImplementedException();
        }

    }
}
