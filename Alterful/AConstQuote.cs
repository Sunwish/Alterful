using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Alterful.Helper;
using System.IO;
namespace Alterful.Functions
{
    public class ConstQuoteMapConfigFormatException : FormatException { }
    public class ConstQuoteNameAlreadyExistsException : Exception { }
    public class ConstQuoteItemNotFoundException : Exception { }
    public static class AConstQuote
    {
        public static string CONSTQUOTE_FILE_PATH = AFile.BASE_PATH + @"\Config\ConstQuoteMap";
        public static char CONSTQUOTE_CONFIGLINE_DEVIDE_SYMBOL = '#';
        public struct ConstQuoteItem
        {
            public string constQuoteName;
            public string constQuoteString;
        }

        private static List<ConstQuoteItem> constQuoteItems = new List<ConstQuoteItem>();

        public static List<ConstQuoteItem> GetConstQuoteItems()
        {
            return constQuoteItems;
        }

        /// <summary>
        /// 判断常引用文件是否存在
        /// </summary>
        /// <returns></returns>
        public static bool IsConstQuoteFileExists()
        {
            return File.Exists(CONSTQUOTE_FILE_PATH);
        }

        /// <summary>
        /// 创建常引用文件，若原本已存在常引用文件则不进行操作
        /// </summary>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static void CreateConstQuoteFile()
        {
            if (!IsConstQuoteFileExists())
                using (StreamWriter streamWriter = new StreamWriter(CONSTQUOTE_FILE_PATH, false)) { }
        }

        /// <summary>
        /// 获取常引用映射配置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ConstQuoteMapConfigFormatException"></exception>
        public static List<ConstQuoteItem> GetConstQuoteMapConfig()
        {
            try
            {
                List<ConstQuoteItem> constQuoteMapConfig = new List<ConstQuoteItem>();
                using (StreamReader streamReader = new StreamReader(CONSTQUOTE_FILE_PATH))
                {
                    List<string> constQuoteMapConfigLines = GetConstQuoteMapConfigLines(streamReader.ReadToEnd());
                    foreach (var constQuoteMapConfigSingleLine in constQuoteMapConfigLines)
                        if ("" != constQuoteMapConfigSingleLine.Trim())
                            constQuoteMapConfig.Add(ConstQuoteItemParse(constQuoteMapConfigSingleLine));
                }
                return constQuoteMapConfig;
            }
            catch (Exception){ throw; }
        }

        /// <summary>
        /// 读取全部常引用映射配置
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ConstQuoteMapConfigFormatException"></exception>
        public static void ReadAllConfig()
        {
            try{ constQuoteItems = GetConstQuoteMapConfig(); }
            catch (Exception){throw;}
        }

        /// <summary>
        /// 获取常引用映射配置文件行
        /// </summary>
        /// <param name="constQuoteMapConfigString">常引用映射原文本</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static List<string> GetConstQuoteMapConfigLines(string constQuoteMapConfigString)
        {
            return new List<string>(constQuoteMapConfigString.Split('\n'));
        }

        /// <summary>
        /// 常引用项解析
        /// </summary>
        /// <param name="constQuoteMapConfigSingleLine">单行常引用映射配置文本行</param>
        /// <returns></returns>
        /// <exception cref="ConstQuoteMapConfigFormatException"></exception>
        public static ConstQuoteItem ConstQuoteItemParse(string constQuoteMapConfigSingleLine)
        {
            constQuoteMapConfigSingleLine = constQuoteMapConfigSingleLine.Trim();
            if ("" == constQuoteMapConfigSingleLine) return new ConstQuoteItem();
            int firstConstQuoteSymbolPosition = constQuoteMapConfigSingleLine.IndexOf(CONSTQUOTE_CONFIGLINE_DEVIDE_SYMBOL);
            if(-1 == firstConstQuoteSymbolPosition || constQuoteMapConfigSingleLine.Length - 1 == firstConstQuoteSymbolPosition) throw new ConstQuoteMapConfigFormatException();
            List<string> paramList = new List<string> {
                constQuoteMapConfigSingleLine.Substring(0, firstConstQuoteSymbolPosition),
                constQuoteMapConfigSingleLine.Substring(firstConstQuoteSymbolPosition + 1)
            };
            return new ConstQuoteItem()
            {
                constQuoteName = paramList[0].Trim(),
                constQuoteString = paramList[1].Trim()
            };
        }

        /// <summary>
        /// 判断常引用名是否存在
        /// </summary>
        /// <param name="constQuoteName"></param>
        /// <returns></returns>
        public static bool IsQuoteNameExists(string constQuoteName)
        {
            foreach(var constQuoteItem in constQuoteItems)
            {
                if (constQuoteItem.constQuoteName == constQuoteName) return true;
            }
            return false;
        }

        /// <summary>
        /// 添加一个常引用，成功后将自动重新读取常引用配置信息
        /// </summary>
        /// <param name="name">常引用名</param>
        /// <param name="quote">常引用内容</param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ConstQuoteNameAlreadyExistsException"></exception>
        public static void Add(string name, string quote)
        {
            try
            {
                if (!IsConstQuoteFileExists()) throw new FileNotFoundException();
                if (IsQuoteNameExists(name))
                {
                    if (GetQuote(name) != quote)
                        throw new ConstQuoteNameAlreadyExistsException();
                    else
                        return;
                }
                List<ConstQuoteItem> tempList = new List<ConstQuoteItem>(constQuoteItems);
                tempList.Add(new ConstQuoteItem()
                {
                    constQuoteName = name,
                    constQuoteString = quote
                });
                WriteConfigToFile(tempList);
                ReadAllConfig();
            }
            catch (Exception){ throw; }            
        }

        /// <summary>
        /// 获取引用内容
        /// </summary>
        /// <param name="constQuoteName">欲获取引用内容的常引用名</param>
        /// <returns></returns>
        /// <exception cref="ConstQuoteItemNotFoundException"></exception>
        public static string GetQuote(string constQuoteName)
        {
            foreach (var constQuoteItem in constQuoteItems)
                if (constQuoteItem.constQuoteName == constQuoteName)
                    return constQuoteItem.constQuoteString;
            throw new ConstQuoteItemNotFoundException();
        }

        /// <summary>
        /// 删除常引用项
        /// </summary>
        /// <param name="name">欲删除常引用项的引用名</param>
        /// <exception cref="ConstQuoteItemNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static void Delete(string name)
        {
            if (!IsQuoteNameExists(name)) throw new ConstQuoteItemNotFoundException();
            List<ConstQuoteItem> tempList = new List<ConstQuoteItem>(constQuoteItems);
            int targetIndex = GetIndexOfConstQuote(name);
            if (-1 == targetIndex) throw new ConstQuoteItemNotFoundException();
            tempList.RemoveAt(targetIndex);
            WriteConfigToFile(tempList);
            constQuoteItems.RemoveAt(targetIndex);
        }

        /// <summary>
        /// 获取常引用在程序内部常引用列表中的下标
        /// </summary>
        /// <param name="name">欲获取下标的常引用名</param>
        /// <returns></returns>
        public static int GetIndexOfConstQuote(string name)
        {
            foreach(var constQuoteItem in constQuoteItems)
            {
                if (constQuoteItem.constQuoteName == name)
                    return constQuoteItems.IndexOf(constQuoteItem);
            }
            return -1;
        }

        /// <summary>
        /// 将当前程序内存储的常引用映射配置信息覆盖写出至常引用文件中
        /// </summary>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static void WriteConfigToFile(List<ConstQuoteItem> configList)
        {
            string outputString = "";
            foreach (var constQuoteItem in configList)
            {
                outputString += constQuoteItem.constQuoteName + CONSTQUOTE_CONFIGLINE_DEVIDE_SYMBOL + constQuoteItem.constQuoteString + "\n";
            }
            using (StreamWriter streamWriter = new StreamWriter(CONSTQUOTE_FILE_PATH, false))
            {
                streamWriter.WriteLine(outputString);
            }
        }

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
            string constQuoteName;
            while (constQuoteSymbolPosition != -1)
            {
                int endPosition;
                int constIndex = instructionString.Substring(constQuoteSymbolPosition + 1).IndexOf(AInstruction.SYMBOL_CONST);
                int constAddIndex = instructionString.Substring(constQuoteSymbolPosition + 1).IndexOf(AInstruction.SYMBOL_CONST_ADD);
                if (-1 == constIndex) endPosition = constQuoteSymbolPosition + constAddIndex + 1;
                else if (-1 == constAddIndex) endPosition = constQuoteSymbolPosition + constIndex + 1;
                else endPosition = constQuoteSymbolPosition + Math.Min(constIndex, constAddIndex) + 1;

                if (endPosition != constQuoteSymbolPosition && endPosition != constQuoteEndPosition)
                {
                    constQuoteEndPosition = endPosition;
                    if (constQuoteSymbolPosition != 0)
                    {
                        if (instructionString[constQuoteSymbolPosition - 1] == AInstruction.SYMBOL_CONST_ADD)
                        {
                            int removeAddStuffix = 0;
                            if (instructionString.Length > constQuoteEndPosition + 1 && instructionString[constQuoteEndPosition + 1] != AInstruction.SYMBOL_CONST) removeAddStuffix = 1;
                            constQuoteName = instructionString.Substring(constQuoteSymbolPosition - 1, constQuoteEndPosition - constQuoteSymbolPosition + 1 + removeAddStuffix);
                            var regex = new Regex(Regex.Escape(constQuoteName));
                            resultString = regex.Replace(resultString, GetQuote(ConstQuoteNamePull(constQuoteName)), 1);
                        }
                        else throw new ConstQuoteParseError();
                    }
                    else
                    {
                        int removeAddStuffix = 0;
                        if (instructionString.Length > constQuoteEndPosition + 1 && instructionString[constQuoteEndPosition + 1] != AInstruction.SYMBOL_CONST) removeAddStuffix = 1;
                        constQuoteName = instructionString.Substring(constQuoteSymbolPosition, constQuoteEndPosition - constQuoteSymbolPosition + removeAddStuffix);
                        var regex = new Regex(Regex.Escape(constQuoteName));
                        resultString = regex.Replace(resultString, GetQuote(ConstQuoteNamePull(constQuoteName)), 1);
                    }

                    int moveForward = instructionString.Substring(constQuoteEndPosition + 1).IndexOf(AInstruction.SYMBOL_CONST) + 1;
                    if (0 == moveForward) break;
                    constQuoteSymbolPosition = constQuoteEndPosition + moveForward;
                    constQuoteEndPosition = instructionString.Length - 1;
                }
                else
                {
                    int includeBackChar = constQuoteSymbolPosition != 0 && instructionString[constQuoteSymbolPosition - 1] == AInstruction.SYMBOL_CONST_ADD ? -1 : 0;
                    constQuoteName = instructionString.Substring(constQuoteSymbolPosition + includeBackChar, constQuoteEndPosition - constQuoteSymbolPosition + 1 - includeBackChar);
                    resultString = resultString.Replace(constQuoteName, GetQuote(ConstQuoteNamePull(constQuoteName)));
                    break;
                }
            }
            return resultString;
        }

        /// <summary>
        /// 从可能带有前后缀的常引用名文本串中拉去纯净的常引用名，如：#constName -> constName
        /// </summary>
        /// <param name="constQuoteName"></param>
        /// <returns></returns>
        public static string ConstQuoteNamePull(string constQuoteName)
        {
            if (constQuoteName.Length == 0) return "";
            if ('+' == constQuoteName[constQuoteName.Length - 1])
                constQuoteName = constQuoteName.Substring(0, constQuoteName.Length - 1);
            if (CONSTQUOTE_CONFIGLINE_DEVIDE_SYMBOL == constQuoteName[0])
                constQuoteName = constQuoteName.Substring(1);
            return constQuoteName;
        }
    }
}

