using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using IWshRuntimeLibrary;
using Alterful.Helper;
namespace Alterful.Functions
{
    public static class AFile
    {
        public static string BASE_PATH { get; } = AHelper.BASE_PATH;
        public static string APATH_PATH { get; } = AHelper.APATH_PATH;
        public const string LNK_EXTENTION = AHelper.LNK_EXTENTION;
        public static string ATEMP_PATH { get; } = AHelper.ATEMP_PATH;
        public class StartupItemNotFoundException : FileNotFoundException { }

        /// <summary>
        /// 获取快捷方式的目标路径
        /// </summary>
        /// <param name="shortcupFilePath">快捷方式的完整路径</param>
        /// <returns></returns>
        /// <exception cref="StartupItemNotFoundException"></exception>
        static public string GetTargetPathOfShortcutFile(string shortcupFilePath)
        {
            if (!System.IO.File.Exists(shortcupFilePath)) throw new StartupItemNotFoundException();
            IWshShell_Class wshShell = new IWshShell_Class();
            try
            {
                IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(shortcupFilePath);
                return shortcut.TargetPath;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return "";

        }

        /// <summary>
        /// 启动一个进程，且不检查其路径是否有效。一般在不清楚要启动的文件路径但其被包含在环境变量中因而可以被直接启动时调用此方法
        /// </summary>
        /// <param name="filePath">要启动的文件路径</param>
        /// <param name="fileArgs">附加启动参数，默认为空</param>
        static private void StartupProcessWithoutCheck(string filePath, string fileArgs = "", bool startupAsAdministrator = false)
        {
            Process newProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    Arguments = fileArgs,
                    FileName = filePath,
                }
            };
            if (startupAsAdministrator) newProcess.StartInfo.Verb = "runas";
            newProcess.Start();
        }

        /// <summary>
        /// 启动一个进程
        /// </summary>
        /// <param name="filePath">要启动的文件路径</param>
        /// <param name="fileArgs">附加启动参数，默认为空</param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void StartupProcess(string filePath, string fileArgs = "", bool startupAsAdministrator = false)
        {
            if (!System.IO.File.Exists(filePath) && !System.IO.Directory.Exists(filePath)) throw new FileNotFoundException();
            StartupProcessWithoutCheck(filePath, fileArgs, startupAsAdministrator);
        }

        /// <summary>
        /// 获取文件的快捷方式完整路径，目标文件必须是Alterful启动项
        /// </summary>
        /// <param name="startupName">启动名</param>
        /// <param name="Apath">环境目录，可取AHelper.APATH_PATH</param>
        /// <param name="lnkExtention">扩展名，默认为AHelper.LNK_EXTENTION</param>
        /// <returns></returns>
        static public string GetFullPathOfShortcutFile(string startupName, string Apath, string lnkExtention = AHelper.LNK_EXTENTION)
        {
            return Apath + (Apath[Apath.Length-1] == '\\' ? "" : @"\") + (startupName[0] == '\\' ? startupName.Substring(1) : startupName) + lnkExtention;
        }

        /// <summary>
        /// 获取文件的目标完整路径，目标文件必须是Alterful启动项
        /// </summary>
        /// <param name="startupName">启动名</param>
        /// <returns></returns>
        static public string GetFullPath(string startupName)
        {
            return GetTargetPathOfShortcutFile(GetFullPathOfShortcutFile(startupName, AHelper.APATH_PATH));
        }

        /// <summary>
        /// 启动一个文件，目标文件必须是Alterful启动项
        /// </summary>
        /// <param name="startupName">启动名</param>
        /// <param name="startupParameter">附加启动参数，默认为空</param>
        /// <exception cref="StartupItemNotFoundException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static void Launch(string startupName, string startupParameter = "", bool startupAsAdministrator = false)
        {
            if (!Exists(startupName)) throw new StartupItemNotFoundException();
            string fullPathOfLnkFile = GetFullPathOfShortcutFile(startupName, AHelper.APATH_PATH);
            string targetFilePath = GetTargetPathOfShortcutFile(fullPathOfLnkFile);
            StartupProcess(targetFilePath, startupParameter, startupAsAdministrator);
        }

        /// <summary>
        /// 从ATemp目录启动一个临时文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="startupParameter"></param>
        public static void LaunchTempFile(string fileName, string startupParameter = "", bool startupAsAdministrator = false)
        {
            try
            {
                StartupProcess(ATEMP_PATH + @"\" + fileName, startupParameter, startupAsAdministrator);
            }
            catch (StartupItemNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 在资源管理器中显示文件，目标文件必须是Alterful启动项
        /// </summary>
        /// <param name="startupName">启动名</param>
        public static void ShowInExplorer(string startupName)
        {
            StartupProcessWithoutCheck("explorer.exe", "/select, \"" + GetFullPath(startupName) + "\"");
        }

        /// <summary>
        /// 获取APath目录下所有文件的FileInfo
        /// </summary>
        /// <returns></returns>
        private static FileInfo[] GetLnkFilesInfo()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AHelper.APATH_PATH);
            return directoryInfo.GetFiles();
        }

        /// <summary>
        /// 获取当前所有Alterful启动项的启动名
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLnkList()
        {
            List<string> lnkList = new List<string>();
            FileInfo[] fileInfo = GetLnkFilesInfo();
            foreach (FileInfo file in fileInfo)
                if (file.Extension == AHelper.LNK_EXTENTION)
                    lnkList.Add(file.Name.Substring(0, file.Name.IndexOf(AHelper.LNK_EXTENTION)));
            return lnkList;
        }

        /// <summary>
        /// 获取常指令文件信息
        /// </summary>
        /// <returns></returns>
        private static FileInfo[] GetConstInstructionFilesInfo()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(AHelper.CONST_INSTRUCTION_PATH);
            return directoryInfo.GetFiles();
        }

        /// <summary>
        /// 获取当前所有常指令列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetConstInstructionList()
        {
            List<string> constInstructionList = new List<string>();
            FileInfo[] fileInfo = GetConstInstructionFilesInfo();
            foreach (FileInfo file in fileInfo)
                if (file.Extension == "")
                    constInstructionList.Add(file.Name);
            return constInstructionList;
        }

        /// <summary>
        /// 判断启动名是否存在
        /// </summary>
        /// <param name="startupName">欲判断的启动名</param>
        /// <returns></returns>
        public static bool Exists(string startupName)
        {
            List<string> startupNameList = GetLnkList();
            return startupNameList.IndexOf(startupName) != -1;
        }

        /// <summary>
        /// 删除Alterful启动项
        /// </summary>
        /// <param name="startupName">启动名</param>
        /// <exception cref="StartupItemNotFoundException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static void Delete(string startupName)
        {
            if (!Exists(startupName)) throw new StartupItemNotFoundException();
            string fullPathOfLnkFile = GetFullPathOfShortcutFile(startupName, APATH_PATH);
            System.IO.File.Delete(fullPathOfLnkFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startupName">启动名</param>
        /// <param name="targetPath">目标路径</param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void Add(string startupName, string targetPath)
        {
            if (!System.IO.File.Exists(targetPath))
            {
                if (!Directory.Exists(targetPath))
                {
                    if (!Exists(targetPath)) throw new FileNotFoundException();
                    else { Copy(startupName, targetPath); return; }                        
                }
            }
            AHelper.CreateShortcut(AFile.APATH_PATH + @"\" + startupName + AFile.LNK_EXTENTION, targetPath);
        }

        /// <summary>
        /// 复制一个具有相同指向的启动项
        /// </summary>
        /// <param name="newStartupName">新启动名</param>
        /// <param name="origStartupName">被复制项的启动名</param>
        /// <exception cref="StartupItemNotFoundException"></exception>
        public static void Copy(string newStartupName, string origStartupName)
        {
            if (!Exists(origStartupName)) throw new StartupItemNotFoundException();
            try{ Add(newStartupName, GetFullPath(origStartupName)); }
            catch (Exception){ throw; }
        }

        /// <summary>
        /// 将一个Alterful启动项的目标文件复制到ATemp目录
        /// </summary>
        /// <param name="startupName">启动名</param>
        /// <exception cref="StartupItemNotFoundException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static void MoveToATemp(string startupName)
        {
            if (!Exists(startupName)) throw new StartupItemNotFoundException();
            string targetFullPath = GetFullPath(startupName);
            if (!System.IO.File.Exists(targetFullPath)) throw new FileNotFoundException();
            System.IO.File.Copy(targetFullPath, ATEMP_PATH + @"\" + System.IO.Path.GetFileName(targetFullPath));
        }
    }
}
