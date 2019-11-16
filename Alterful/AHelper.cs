using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary;
using System.Diagnostics;
using Alterful.Functions;
namespace Alterful.Helper
{
    public static class AHelper
    {
        public static string BASE_PATH { get; } = System.IO.File.Exists(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location)))) + @"\Alterful.sln") ? System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location)))) : System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
        public static string APATH_PATH { get; } = BASE_PATH + @"\APath";
        public static string ATEMP_PATH { get; } = BASE_PATH + @"\ATemp";
        public const string LNK_EXTENTION = ".lnk";
        public static void Initialize()
        {
            // Floder Check
            Directory.CreateDirectory(BASE_PATH + @"\APath");
            Directory.CreateDirectory(BASE_PATH + @"\ATemp");
            Directory.CreateDirectory(BASE_PATH + @"\Config");

            // File Check
            AConstQuote.CreateConstQuoteFile();
            
            // Config Check
            AConstQuote.ReadAllConfig();

            // Enviroment Check
            SysEnviroment.SetPathAfter(APATH_PATH);

            // Regedit Check

        }

        /// <summary>
        /// 创建一个快捷方式
        /// </summary>
        /// <param name="lnkPath">创建的快捷方式完整路径</param>
        /// <param name="targetPath">目标完整路径</param>
        public static void CreateShortcut(string lnkPath, string targetPath)
        {
            IWshShell_Class wshShell = new IWshShell_Class();
            IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(lnkPath.Trim());
            shortcut.TargetPath = targetPath.Trim();
            shortcut.Save();
        }

        /// <summary>
        /// 执行命令行
        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns>命令行返回文本</returns>
        public static string ExecuteCommand(string commandLine)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/c " + commandLine)
                {
                    // The following command are needed to redirect the standard output.
                    // This means that it will be redirected to the Process.StandardOutput StreamReader.
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                Process process = new Process() { StartInfo = startInfo };
                process.Start();
                // Return the output.
                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();
                if (standardError != "") throw new Exception(standardError);
                return standardOutput;
            }
            catch (Exception exception)
            {
                // Console.WriteLine(exception.Message);
                throw;
            }
        }
    }

    static class SysEnviroment
    {
        /// <summary>
        /// 打开系统环境变量注册表
        /// </summary>
        /// <returns>RegistryKey</returns>
        private static RegistryKey OpenSysEnvironment()
        {
            RegistryKey regLocalMachine = Registry.LocalMachine;
            RegistryKey regSYSTEM = regLocalMachine.OpenSubKey("SYSTEM", true);//打开HKEY_LOCAL_MACHINE下的SYSTEM 
            RegistryKey regControlSet001 = regSYSTEM.OpenSubKey("ControlSet001", true);//打开ControlSet001 
            RegistryKey regControl = regControlSet001.OpenSubKey("Control", true);//打开Control 
            RegistryKey regManager = regControl.OpenSubKey("Session Manager", true);//打开Control 

            RegistryKey regEnvironment = regManager.OpenSubKey("Environment", true);
            return regEnvironment;
        }

        /// <summary>
        /// 获取系统环境变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSysEnvironmentByName(string name)
        {
            string result = string.Empty;
            try{ result = OpenSysEnvironment().GetValue(name).ToString(); }
            catch (Exception){ return string.Empty;}
            return result;
        }

        /// <summary>
        /// 设置系统环境变量
        /// </summary>
        /// <param name="name">变量名</param>
        /// <param name="strValue">值</param>
        public static void SetSysEnvironment(string name, string strValue)
        {
            OpenSysEnvironment().SetValue(name, strValue);
        }

        /// <summary>
        /// 添加到PATH环境变量（会检测路径是否存在，存在就不重复）
        /// </summary>
        /// <param name="strPath"></param>
        public static void SetPathAfter(string strHome)
        {
            string pathlist;
            pathlist = GetSysEnvironmentByName("PATH");

            //检测是否以;结尾
            if (pathlist.Substring(pathlist.Length - 1, 1) != ";")
            {
                SetSysEnvironment("PATH", pathlist + ";");
                pathlist = GetSysEnvironmentByName("PATH");
            }
            string[] list = pathlist.Split(';');
            bool isPathExist = false;

            foreach (string item in list)
            {
                if (item == strHome)
                    isPathExist = true;
            }
            if (!isPathExist)
            {
                SetSysEnvironment("PATH", pathlist + strHome + ";");
            }
        }
    }
}
