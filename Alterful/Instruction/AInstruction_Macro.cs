﻿using Alterful.Functions;
using Alterful.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Instruction
{
    public class AInstruction_Macro : AInstruction
    {
        public enum MacroType { ADD, NEW, DEL, SET, UPDATE, RESTART, LOCATE, VERSION }
        public enum MacroAddType { STARTUP, CONST_QUOTE }
        public enum MacroDelType { STARTUP, CONST_QUOTE }
        //public static string MSG_UNKNOW_MACRO_TYPE { get; } = "未知的宏指令类型";
        //public static string MSG_MACRO_FORMAT_EXCEPTION { get; } = "未知的宏指令格式。";
        public static string MSG_UNKNOW_MACRO_TYPE { get; } = "Unknow macro type";
        public static string MSG_MACRO_FORMAT_EXCEPTION { get; } = "Unknow macro instruction format.";
        public AInstruction_Macro(string instruction) : base(instruction)
        {
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
        /// 获取补全
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public static string GetCompletion(string part) => SYMBOL_MACRO + AHelper.FindCompletion(new List<string> { "add", "del", "new", "set", "update", "restart", "locate", "version" }, part.Substring(1));

        /// <summary>
        /// 获取宏指令类型
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MacroFormatException"></exception>
        /// <exception cref="UnknowMacroType"></exception>
        public MacroType GetMacroType()
        {
            List<string> macroInstructionParts = new List<string>(Instruction.Split(' '));
            // if (macroInstructionParts.Count < 2) throw new MacroFormatException();
            string macroType = macroInstructionParts[0].Substring(1);
            switch (macroType)
            {
                case "add": return MacroType.ADD;
                case "new": return MacroType.NEW;
                case "del": return MacroType.DEL;
                case "set": return MacroType.SET;
                case "update": return MacroType.UPDATE;
                case "restart": return MacroType.RESTART;
                case "locate": return MacroType.LOCATE;
                case "version": return MacroType.VERSION;
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
                    case MacroType.SET: ExecuteMacroSet(); break;
                    case MacroType.UPDATE: ExecuteMacroUpdate(); break;
                    case MacroType.RESTART: ExecuteMacroRestart(); break;
                    case MacroType.LOCATE: ExecuteMacroLocate(); break;
                    case MacroType.VERSION: ExecuteMacroVersion(); break;
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
            return new List<string>(Instruction.Trim().Split(' '));
        }

        /// <summary>
        /// 取宏指令参数列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetMacroInstructionParametersList()
        {
            List<string> paramList = new List<string>(GetMacroInstructionPartsList().Where(p => p != ""));
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

            // Handle [in] sytex
            string inWhere = AFile.ATEMP_PATH;
            if(macroInstructionParameters.Count >= 3 && "in" == macroInstructionParameters[macroInstructionParameters.Count - 2])
            {
                string lastParam = macroInstructionParameters[macroInstructionParameters.Count - 1];
                if (AFile.Exists(lastParam))
                {
                    // If the item is already a directory
                    string itemPath = AFile.GetFullPath(lastParam);
                    if (Directory.Exists(itemPath)) inWhere = itemPath;
                    // If the item is exist
                    else if (File.Exists(itemPath)) inWhere = Path.GetDirectoryName(itemPath);
                    else throw new NotImplementedException("The startup item [" + lastParam + "] is not found in disk.");
                }
                else if (Directory.Exists(lastParam)) inWhere = lastParam;
                else throw new NotImplementedException("The directory [" + lastParam + "] is not exist.");
                macroInstructionParameters.RemoveRange(macroInstructionParameters.Count - 2, 2);
            }
            // Remove the last '\'
            if (inWhere.Last() == '\\') inWhere = inWhere.Substring(0, inWhere.Length - 1);

            foreach (string newFileName in macroInstructionParameters)
            {
                string fileName = AConstQuote.ConstQuoteParse(newFileName);
                string filePath = inWhere + @"\" + fileName;
                using (StreamWriter streamWriter = new StreamWriter(filePath, false)) { }
                AFile.StartupProcess(filePath);
                //AFile.LaunchTempFile(fileName);
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
            foreach (string param in paramText.Split(','))
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
            if ((Instruction.IndexOf("(") < Instruction.IndexOf(")")) && Instruction.IndexOf(")") != -1 && Instruction.IndexOf(")") == Instruction.Length - 1) throw new Exception(AInstruction.ADD_CONST_INSTRUCTION);

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
                string startupName = AConstQuote.ConstQuoteParse(macroInstructionParameters[0]);
                string targetPath = AConstQuote.ConstQuoteParse(macroInstructionParameters[1]);
                if (AFile.Exists(startupName))
                {
                    //throw new NotImplementedException("添加失败，因为启动名 [" + startupName + "] 已经存在。");
                    throw new NotImplementedException("Failed to add, because item [" + startupName + "] is already exist.");
                }
                AFile.Add(startupName, targetPath);
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
            catch (Exception) { throw; }
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
            if ((Instruction.IndexOf("(") < Instruction.IndexOf(")")) && Instruction.IndexOf(")") != -1 && Instruction.IndexOf(")") == Instruction.Length - 1)
            {
                try { AConstInstruction.Delete(AInstruction_Const.GetConstInstructionFromMacroInstruction(Instruction)); return; }
                catch (Exception exception) { throw exception; }
            }

            List<string> macroInstructionParametersRaw = GetMacroInstructionParametersList();
            if (macroInstructionParametersRaw.Count < 1) throw new MacroFormatException();
            foreach (string delItemString in macroInstructionParametersRaw)
            {
                switch (GetMacroDelType(delItemString))
                {
                    case MacroDelType.STARTUP: AFile.Delete(delItemString); break;
                    case MacroDelType.CONST_QUOTE: ExecuteMacroDelConstQuote(delItemString); break;
                }
            }
        }

        /// <summary>
        /// 执行宏删除常引用指令
        /// </summary>
        /// <param name="delConstQuoteName"></param>
        private void ExecuteMacroDelConstQuote(string delConstQuoteName)
        {
            AConstQuote.Delete(AConstQuote.ConstQuoteNamePull(delConstQuoteName));
        }

        /// <summary>
        /// 执行宏设置指令
        /// </summary>
        private void ExecuteMacroSet()
        {
            // get: @set settingItem
            // set: @set settingItem value
            List<string> paramList = GetMacroInstructionParametersList();
            switch (paramList.Count)
            {
                case 0:
                    // List setting items.
                    //string retnMessage = "可用的设置项: \n";
                    string retnMessage = "Available setting items: ";
                    foreach (string propertyName in ASettings.GetSettingsPropertiesName())
                        retnMessage += propertyName + " ";
                    throw new NotImplementedException(retnMessage);
                case 1:
                    // List available value and current value.
                    if (0 == ASettings.GetSettingsPropertiesName().Where(p => p == paramList[0]).Count())
                        //throw new NotImplementedException("设置项 [" + paramList[0] + "] 没有找到。");
                        throw new NotImplementedException("Setting item [" + paramList[0] + "] is not found.");
                    string targetPropertyName = paramList[0].Trim();
                    //string availableValues = "可选值: ";
                    string availableValues = "Available values: ";
                    Type t = typeof(ASettings).GetProperty(paramList[0]).GetValue(null).GetType();
                    if (t.Equals(true.GetType()))
                    {
                        availableValues += "True / False";
                    }
                    else if (t.Equals(typeof(AlterfulTheme)))
                    {
                        foreach (AlterfulTheme theme in Enum.GetValues(typeof(AlterfulTheme)))
                            availableValues += theme + " / ";
                        availableValues = availableValues.Substring(0, availableValues.Length - 3);
                    }
                    throw new NotImplementedException(targetPropertyName + ": " + typeof(ASettings).GetProperty(paramList[0]).GetValue(null) as string + Environment.NewLine + availableValues);
                case 2:
                    // Set value.
                    if (0 == ASettings.GetSettingsPropertiesName().Where(p => p == paramList[0]).Count())
                        //throw new NotImplementedException("设置项 [" + paramList[0] + "] 没有找到。");
                        throw new NotImplementedException("Setting item [" + paramList[0] + "] is not found.");
                    string targetPropertyName2 = paramList[0].Trim();
                    string setValue = paramList[1].Trim();
                    //string availableValues2 = "可选值: ";
                    string availableValues2 = "Available values: ";
                    Type t2 = typeof(ASettings).GetProperty(paramList[0]).GetValue(null).GetType();
                    if (t2.Equals(true.GetType()))
                    {
                        switch (setValue)
                        {
                            case "True":
                            case "true":
                                typeof(ASettings).GetProperty(paramList[0]).SetValue(null, true);
                                break;
                            case "False":
                            case "false":
                                typeof(ASettings).GetProperty(paramList[0]).SetValue(null, false);
                                break;
                            default:
                                // throw new NotImplementedException("无效的值。");
                                throw new NotImplementedException("Invalid value.");
                        }
                        availableValues2 += "True / False";
                    }
                    else if (t2.Equals(typeof(AlterfulTheme)))
                    {
                        bool found = false;
                        foreach (AlterfulTheme theme in Enum.GetValues(typeof(AlterfulTheme)))
                        {
                            string a = theme.ToString();
                            if (theme.ToString().ToLower().Trim() == setValue.ToLower())
                            {
                                ATheme.Theme = theme;
                                found = true;
                                //ReportInfo.Add("主题设置已经生效，但之前的内容样式不会更新，可用 @restart 来重启 Alterful。");
                                ReportInfo.Add("Theme config have changed, but early content won't be appply, you can @restart Alterful.");
                            }
                        }
                        if (!found) throw new NotImplementedException("Theme [" + setValue + "] is not found."); //throw new NotImplementedException("主题 [" + setValue + "] 没有找到。");
                    }
                    break;
                default:
                    throw new MacroFormatException();
            }
        }

        private void ExecuteMacroUpdate() => throw new Exception(AInstruction.UPDATE_INSTRUCTION);
        private void ExecuteMacroRestart() => AHelper.Restart();
        private void ExecuteMacroLocate()
        {
            List<string> macroInstructionParametersRaw = GetMacroInstructionParametersList();
            foreach(string item in macroInstructionParametersRaw)
            {
                if (AFile.Exists(item)) AFile.ShowInExplorer(item);
                else
                {
                    reportType = ReportType.ERROR;
                    ReportInfo.Add("Startup item [" + item + "] is not exist.");
                }
            }
        }
        private void ExecuteMacroVersion()
        {
            string locVerNum = AVersion.GetLocalVersionNumber();
            throw new Exception("Local version: " + locVerNum);
            // ReportInfo.Add("Local version: " + locVerNum);
        }
    }

}
