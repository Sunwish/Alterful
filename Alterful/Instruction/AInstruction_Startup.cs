using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alterful.Functions;
using Alterful.Helper;
namespace Alterful.Instruction
{

    public class AInstruction_Startup : AInstruction
    {
        /// <summary>
        /// 指令中包含的启动项个数
        /// </summary>
        public int Count { get; }
        public AInstruction_Startup(string instruction) : base(instruction) => Count = instruction.Split(' ').Length;

        /// <summary>
        /// 获取补全
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public static string GetCompletion(string part)
        {
            List<string> list = AFile.GetLnkList();
            list.AddRange(ASettings.GetSettingsPropertiesName());
            return AHelper.FindCompletion(list, part);
        }

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
                            case "c": throw new NotImplementedException(); /*AFile.GetFullPath(item.StartupName);*/
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
            return existCount != 0 ? AInstruction.MSG_EXECUTE_SUCCESSFULLY : "";
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
            for (int i = 0; i < startupItemStringList.Count; i++)
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
}
