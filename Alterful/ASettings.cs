using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alterful.Helper;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace Alterful.Functions
{
    class ASettings
    {
        private static readonly string autoRunSubKeyName = @"\software\microsoft\windows\CurrentVersion\Run\";
        private static readonly string autoRunKeyName = Registry.LocalMachine + autoRunSubKeyName;
        private static readonly string autoRunValueName = "Alterful";
        private static readonly string selfLocation = System.Reflection.Assembly.GetEntryAssembly().Location;

        private static readonly string rightMenuSubKeyName_1 = @"\*\shell\" + autoRunValueName + @"Startup\";
        private static readonly string rightMenuKeyName_1 = Registry.ClassesRoot + rightMenuSubKeyName_1;
        private static readonly string rightMenuSubKeyName_2 = @"\Directory\shell\" + autoRunValueName + @"Startup\";
        private static readonly string rightMenuKeyName_2 = Registry.ClassesRoot + rightMenuSubKeyName_2;
        private static readonly string rightMenuString = "添加为 Alterful 启动项";

        /// <summary>
        /// 获取所有的设置项名称
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSettingsPropertiesName()
        {
            MemberInfo[] members = typeof(ASettings).GetMembers();
            List<string> propertiesName = new List<string>();
            foreach (MemberInfo memberInfo in members.Where(p => p.MemberType == MemberTypes.Property))
                propertiesName.Add(memberInfo.Name);
            return propertiesName;
        }

        /// <summary>
        /// 开机自动启动
        /// </summary>
        public static bool AutoRunWithSystem
        {
            get
            {
                string path = SysRegedit.GetKeyValue(autoRunKeyName, autoRunValueName);
                return path != null && path.Equals(selfLocation);
            }
            set
            {
                // if (value == AutoRunWithSystem) return;
                if (value)
                    SysRegedit.SetKeyValue(autoRunKeyName, autoRunValueName, selfLocation);
                else
                    SysRegedit.DeleteValue(Registry.LocalMachine, autoRunSubKeyName, autoRunValueName);
            }
        }

        /// <summary>
        /// 在系统右键菜单中显示
        /// </summary>
        public static bool DisplayRightMenu
        {
            get
            {
                string sIcon_1 = SysRegedit.GetKeyValue(rightMenuKeyName_1, "icon");
                string sCommand_1 = SysRegedit.GetKeyValue(rightMenuKeyName_1 + "Command", null);
                string sIcon_2 = SysRegedit.GetKeyValue(rightMenuKeyName_2, "icon");
                string sCommand_2 = SysRegedit.GetKeyValue(rightMenuKeyName_2 + "Command", null);
                return sIcon_1 != null && sCommand_1 != null && sIcon_2 != null && sCommand_2 != null;
            }
            set
            {
                // if (value == DisplayRightMenu) return;
                if (value)
                {
                    string pipePath = Path.GetDirectoryName(selfLocation) + @"\AlterfulPipe.exe";
                    SysRegedit.SetKeyValue(rightMenuKeyName_1, null, rightMenuString);
                    SysRegedit.SetKeyValue(rightMenuKeyName_1, "icon", "\"" + Directory.GetCurrentDirectory() + @"\Alterful.ico" + "\"");
                    SysRegedit.SetKeyValue(rightMenuKeyName_1 + "Command", null, "\"" + pipePath + "\"" + " " + "\"" + "%1" + "\"");

                    SysRegedit.SetKeyValue(rightMenuKeyName_2, null, rightMenuString);
                    SysRegedit.SetKeyValue(rightMenuKeyName_2, "icon", "\"" + Directory.GetCurrentDirectory() + @"\Alterful.ico" + "\"");
                    SysRegedit.SetKeyValue(rightMenuKeyName_2 + "Command", null, "\"" + pipePath + "\"" + " " + "\"" + "%1" + "\"");
                }
                else
                {
                    SysRegedit.DeleteKeyTree(Registry.ClassesRoot, @"\*\shell", autoRunValueName + @"Startup");
                    SysRegedit.DeleteKeyTree(Registry.ClassesRoot, @"\Directory\shell", autoRunValueName + @"Startup");
                }
                
            }
        }

        public static AlterfulTheme Theme
        {
            get { return ATheme.Theme; }
            set { ATheme.Theme = value; }
        }
    }
}
