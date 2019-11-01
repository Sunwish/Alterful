﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary;
namespace Alterful.Helper
{
    static class AHelper
    {
        public static string BASE_PATH { get; } = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location))));
        public static string APATH_PATH { get; } = BASE_PATH + @"\APath";
        public const string LNK_EXTENTION = ".lnk";
        public static void Initialize()
        {
            // Floder Check
            Directory.CreateDirectory(BASE_PATH + @"\APath");

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
            IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(lnkPath);
            shortcut.TargetPath = targetPath;
            shortcut.Save();
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
            try
            {
                result = OpenSysEnvironment().GetValue(name).ToString();//读取
            }
            catch (Exception)
            {

                return string.Empty;
            }
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