using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alterful.Functions
{
    enum InstructionType
    {
        STARTUP, MACRO, CONST, BUILDIN
    }

    abstract class AInstruction
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
        static public InstructionType GetType(string instruction)
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
        abstract public void Execute();
    }

    struct StartupItem
    {
        public string StartupName { get; set; }
        public List<string> suffixList { get; set; }
    }

    class AInstruction_Startup : AInstruction
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
                    foreach (var suffix in item.suffixList)
                    {
                        switch (suffix)
                        {
                            case "f": AFile.ShowInExplorer(item.StartupName); break;
                            case "c": throw new NotImplementedException(); AFile.GetFullPath(item.StartupName);
                            case "o": AFile.Launch(item.StartupName); break;
                        }
                    }
                    if (item.suffixList.Count == 0)
                        AFile.Launch(item.StartupName);
                }
                else
                {
                    ReportInfo.Add(item.StartupName);
                }
            }
        }

        /// <summary>
        /// 获取指令中包含的启动项
        /// </summary>
        /// <returns></returns>
        public List<StartupItem> GetStartupItems()
        {
            List<string> startupItemOrignal = new List<string>(Instruction.Split(' '));
            List<StartupItem> startupItemList = new List<StartupItem>();
            foreach (var singleItem in startupItemOrignal)
                startupItemList.Add(StartupNameSuffixesParse(singleItem));
            return startupItemList;
        }

        /// <summary>
        /// 启动项解析
        /// </summary>
        /// <param name="singleStartupItem">单个的启动项原文本，可调用 GetStartupItems() 方法获得</param>
        /// <returns></returns>
        private StartupItem StartupNameSuffixesParse(string singleStartupItem)
        {
            if (singleStartupItem.IndexOf('-') == -1) return new StartupItem() { StartupName = singleStartupItem, suffixList = new List<string>() };

            StartupItem item = new StartupItem();
            List<string> suffixList = new List<string>(singleStartupItem.Split('-'));
            item.StartupName = suffixList[0];
            suffixList.RemoveAt(0);
            item.suffixList = suffixList;

            return item;
        }
    }
}
