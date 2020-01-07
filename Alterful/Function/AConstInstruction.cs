using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Alterful.Helper;

namespace Alterful.Functions
{
    public class ConstInstructionNameUnkonwException : Exception { public ConstInstructionNameUnkonwException() : base("Const instruction parse error.") { } }
    public class ConstInstructionNotFoundException : Exception { public ConstInstructionNotFoundException(string ins) : base("Const instruction [" + ins + "] is not found.") { } }
    public class ConstInstructionParameterParseException : Exception { public ConstInstructionParameterParseException() : base("Const instruction parameter parse error.") { } }
    public struct ConstInstruction
    {
        public string constInstructionName;
        public List<string> parameterList;
        public List<string> instructionLines;
    }

    public static class AConstInstruction
    {
        private static List<string> ConstInstructionFileList = AFile.GetConstInstructionList();
        public const char SYMBOL_PARAMETER_START = '(';
        public const char SYMBOL_PARAMETER_END = ')';
        public const char SYMBOL_PARAMETER_DEVIDE = ',';
        public static void Update() { ConstInstructionFileList = AFile.GetConstInstructionList(); }
        
        /// <summary>
        /// 获取常指令项目列表
        /// </summary>
        /// <returns></returns>
        public static List<ConstInstruction> GetConstInstrcutionItemList()
        {
            Update();
            List<ConstInstruction> itemList = new List<ConstInstruction>();
            foreach(string item in ConstInstructionFileList)
                itemList.Add(ConstInstructionFileNameParse(item));
            return itemList;
        }

        /// <summary>
        /// 获取补全
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public static string GetCompletion(string part)
        {
            List<string> list = new List<string>();
            foreach(ConstInstruction ci in GetConstInstrcutionItemList())
                list.Add(ci.constInstructionName);
            return AHelper.FindCompletion(list, part);
        }

        /// <summary>
        /// 常指令文件解析
        /// </summary>
        /// <param name="fileName">欲解析的常引用文件名</param>
        /// <param name="readInstructionLines">是否读取文件内的指令，若为 false 则仅解析文件名</param>
        /// <returns></returns>
        public static ConstInstruction ConstInstructionFileNameParse(string fileName, bool readInstructionLines = true)
        {
            ConstInstruction item = new ConstInstruction() { parameterList = new List<string>(), instructionLines = new List<string>() };
            if ("" == fileName) return item;

            // Get Parameter List
            int paramStart = fileName.IndexOf(SYMBOL_PARAMETER_START);
            int paramEnd = fileName.IndexOf(SYMBOL_PARAMETER_END);
            if (-1 == paramEnd || -1 == paramStart || paramEnd <= paramStart) throw new ConstInstructionNameUnkonwException();
            string paramString = fileName.Substring(paramStart + 1, paramEnd - paramStart - 1);
            foreach (string paramBlock in paramString.Split(SYMBOL_PARAMETER_DEVIDE))
                item.parameterList.Add(paramBlock.Trim());
            if (1 == item.parameterList.Count && "" == item.parameterList[0].Trim()) item.parameterList.Clear();

            // Get Instruction Name
            item.constInstructionName = fileName.Substring(0, paramStart).Trim();

            if (!readInstructionLines) return item;
            // Get Instruction Lines
            using (StreamReader reader = new StreamReader(AHelper.CONST_INSTRUCTION_PATH + @"\" + fileName))
            {
                string constInstructionsRaw = reader.ReadToEnd();
                foreach (string line in constInstructionsRaw.Split('\n'))
                    if ("" != line.Trim()) item.instructionLines.Add(line.Trim());
            }
            return item;
        }

        /// <summary>
        /// 获取常指令项目结构，被获取结构的常指令必须已经存在
        /// </summary>
        /// <param name="constInstruction"></param>
        /// <param name="retn"></param>
        /// <returns></returns>
        public static bool GetConstInstructionFrame(string constInstruction, ref ConstInstruction retn)
        {
            ConstInstruction sourceInstruction = ConstInstructionFileNameParse(constInstruction, false);
            List<ConstInstruction> instructionItemList = GetConstInstrcutionItemList();
            foreach (ConstInstruction item in instructionItemList)
                if (sourceInstruction.constInstructionName == item.constInstructionName && sourceInstruction.parameterList.Count == item.parameterList.Count)
                { retn = item; return true; }
            return false;
        }

        /// <summary>
        /// 常指令是否存在
        /// </summary>
        /// <param name="constInstruction">指令原型，若有参数则参数可以取任意值</param>
        /// <returns></returns>
        public static bool Exist(string constInstruction)
        {
            ConstInstruction item = new ConstInstruction();
            return GetConstInstructionFrame(constInstruction, ref item);
        }

        /// <summary>
        /// 删除常指令
        /// </summary>
        /// <param name="constInstruction"></param>
        /// <returns></returns>
        /// <exception cref="ConstInstructionNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static string Delete(string constInstruction)
        {
            if (!Exist(constInstruction)) throw new ConstInstructionNotFoundException(constInstruction);
            try
            {
                ConstInstruction item = new ConstInstruction();
                GetConstInstructionFrame(constInstruction, ref item);
                string targetFileName = item.constInstructionName + SYMBOL_PARAMETER_START;
                foreach (string param in item.parameterList)
                    targetFileName += param + SYMBOL_PARAMETER_DEVIDE;
                targetFileName = targetFileName.Substring(0, targetFileName.Length - 1) + SYMBOL_PARAMETER_END;
                File.Delete(AHelper.CONST_INSTRUCTION_PATH + @"\" + targetFileName);
                return targetFileName;
            }
            catch (Exception excption) { throw excption; }
        }

        /// <summary>
        /// 常指令参数解析
        /// </summary>
        /// <param name="constInstruction">常指令结构</param>
        /// <param name="instructionLine">常指令指令行</param>
        /// <param name="paramValueList">参数值列表</param>
        /// <exception cref="ConstInstructionParameterParseException"></exception>
        /// <returns></returns>
        public static string ConstInstructionParameterParse(ConstInstruction constInstruction, string instructionLine, List<string> paramValueList, bool constQuoteParse = true)
        {
            if (paramValueList.Count != constInstruction.parameterList.Count) throw new ConstInstructionParameterParseException();
            for (int i = 0; i < constInstruction.parameterList.Count; i++)
            {
                string value = paramValueList[i];
                if (constQuoteParse) value = AConstQuote.ConstQuoteParse(value);
                instructionLine = instructionLine.Replace(constInstruction.parameterList[i], value);
            }
            return instructionLine;
        }

        public static string GetFileNameFromConstInstruction(ConstInstruction ci)
        {
            string fileName = ci.constInstructionName + "(";
            foreach(string paramName in ci.parameterList)
            {
                fileName += paramName + ",";
            }
            fileName = fileName.Substring(0, fileName.Length - 1) + ")";
            return fileName;
        }
    }
}
