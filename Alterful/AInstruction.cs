using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Functions
{
    public enum InstructionType
    {
        STARTUP, MACRO, CONST, BUILDIN
    }

    public abstract class AInstruction
    {
        public const char SYMBOL_MACRO = '@';
        public const char SYMBOL_CONST = '#';
        public const char SYMBOL_BUILDIN = '.';
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
        /// 执行指令
        /// </summary>
        public abstract void Execute();
    }

    public struct StartupItem
    {
        public string StartupName { get; set; }
        public List<string> suffixList { get; set; }
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
                    if (item.suffixList == null)
                    {
                        AFile.Launch(item.StartupName);
                        continue;
                    }
                    foreach (var suffix in item.suffixList)
                    {
                        switch (suffix)
                        {
                            case "f": AFile.ShowInExplorer(item.StartupName); break;
                            case "c": throw new NotImplementedException(); AFile.GetFullPath(item.StartupName);
                            case "o": AFile.Launch(item.StartupName); break;
                        }
                    }
                }
                else
                {
                    ReportInfo.Add(item.StartupName);
                }
            }
        }

        public List<string> GetStartupItemStringList()
        {
            return new List<string>(Instruction.Split(' '));
        }

        /// <summary>
        /// 获取指令中包含的启动项
        /// </summary>
        /// <returns></returns>
        public List<StartupItem> GetStartupItems()
        {
            List<string> startupItemStringList = GetStartupItemStringList();
            List<StartupItem> startupItemList = new List<StartupItem>();
            foreach (var singleItem in startupItemStringList)
                startupItemList.Add(StartupNameSuffixesParse(singleItem));
            return startupItemList;
        }

        /// <summary>
        /// 启动项解析
        /// </summary>
        /// <param name="singleStartupItem">单个的启动项原文本，可调用 GetStartupItems() 方法获得</param>
        /// <returns></returns>
        public static StartupItem StartupNameSuffixesParse(string singleStartupItem)
        {
            if (singleStartupItem.IndexOf('-') == -1) return new StartupItem() { StartupName = singleStartupItem, suffixList = null };

            StartupItem item = new StartupItem();
            List<string> suffixList = new List<string>(singleStartupItem.Split('-'));
            item.StartupName = suffixList[0];
            suffixList.RemoveAt(0);
            item.suffixList = suffixList;

            return item;
        }
    }
}
