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
using System.Threading;
using System.Net;
using System.Security.Cryptography;

namespace Alterful.Helper
{
    public static class AHelper
    {
        public static string BASE_PATH { get; } = System.IO.File.Exists(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location)))) + @"\Alterful.sln") ? System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location)))) : System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
        public static string APATH_PATH { get; } = BASE_PATH + @"\APath";
        public static string ATEMP_PATH { get; } = BASE_PATH + @"\ATemp";
        public static string CONST_INSTRUCTION_PATH { get; } = BASE_PATH + @"\Config\ConstInstruction";
        public const string RemoteUrl = @"https://alterful.com/bin/";
        public const string LNK_EXTENTION = ".lnk";
        public static List<string> InstructionHistory = new List<string>();
        public static int InstructionPointer = -1;
        public delegate void AppendString(string content, AInstruction.ReportType type);
        public static void Initialize()
        {
            // Floder Check
            if(!Directory.Exists(BASE_PATH + @"\Config"))
            {
                // First start alterful
                ASettings.DisplayRightMenu = true;
                ASettings.AutoRunWithSystem = true;
                CreateShortcut(APATH_PATH + @"\alterful"  + AFile.LNK_EXTENTION, BASE_PATH + @"\Alterful.exe");
            }
            Directory.CreateDirectory(BASE_PATH + @"\APath");
            Directory.CreateDirectory(BASE_PATH + @"\ATemp");
            Directory.CreateDirectory(BASE_PATH + @"\Config");
            Directory.CreateDirectory(CONST_INSTRUCTION_PATH);

            // File Check
            AConstQuote.CreateConstQuoteFile();
            ATheme.CreateThemeConfigFile();

            // Config Check
            AConstQuote.ReadAllConfig();
            ATheme.ReadAllConfig();

            // Enviroment Check
            SysEnviroment.SetPathAfter(APATH_PATH);

            // Regedit Check

            // Others
            System.IO.File.Delete(@".\restart.bat");
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

        public static string FindCompletion(List<string> list, string part)
        {
            foreach (string full in list)
            {
                if (full.IndexOf(part).Equals(0))
                    return full;
            }
            return "";
        }

        public static string GetFileMd5(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return "";
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
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
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 异步执行命令行
        /// </summary>
        /// <param name="command"></param>
        public static void ExecuteCommandAsync(string command)
        {
            /*
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart());
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            */
        }
    }

    class VersionNumberFormatException : Exception { }
    public static class AVersion
    {
        public const string RemoteVersionUrl = AHelper.RemoteUrl + @"AVersionInfo.ini";
        public struct VersionNumberStruct
        {
            public VersionNumberStruct(List<string> versionNumberStructList)
            {
                if (versionNumberStructList.Count != 4) throw new VersionNumberFormatException();
                MainVersionNumber = int.Parse(versionNumberStructList[0]);
                SecVersionNumber = int.Parse(versionNumberStructList[1]);
                MidVersionNumber = int.Parse(versionNumberStructList[2]);
                MinVersionNumber = int.Parse(versionNumberStructList[3]);
            }
            public int MainVersionNumber;
            public int SecVersionNumber;
            public int MidVersionNumber;
            public int MinVersionNumber;
        }

        /// <summary>
        /// 获取远程版本号
        /// </summary>
        /// <returns></returns>
        public static string GetRemoteVersionNumber()
        {
            HttpWebRequest request = WebRequest.Create(RemoteVersionUrl) as HttpWebRequest;
            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                string versionInfoBody = reader.ReadToEnd();
                return versionInfoBody.Substring(versionInfoBody.IndexOf("=") + 1);
            }
        }

        /// <summary>
        /// 获取本地版本号
        /// </summary>
        /// <returns></returns>
        public static string GetLocalVersionNumber() => Properties.Settings.Default.localVersion;

        /// <summary>
        /// 获取版本号结构
        /// </summary>
        /// <param name="versionNumber">版本号文本</param>
        /// <returns></returns>
        /// <exception cref="VersionNumberFormatException"></exception>
        private static VersionNumberStruct GetVersionNumberStruct(string versionNumber)
        {
            List<string> versionNumberStructList = new List<string>(versionNumber.Split('.'));
            try { return new VersionNumberStruct(versionNumberStructList); }
            catch (VersionNumberFormatException) { throw; }
        }

        /// <summary>
        /// 获取版本号差异。版本偏低返回负数，最新版本返回0，内测版本返回正数
        /// </summary>
        /// <param name="local"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        public static int GetVersionNumberDiffer() => GetVersionNumberDiffer(GetLocalVersionNumber(), GetRemoteVersionNumber());

        /// <summary>
        /// 获取版本号差异。版本偏低返回负数，最新版本返回0，内测版本返回正数
        /// </summary>
        /// <param name="local"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        public static int GetVersionNumberDiffer(string local, string remote) => VersionNumberDiffer(local, remote);

        private static int VersionNumberDiffer(string local, string remote)
        {
            VersionNumberStruct vnsLocal = GetVersionNumberStruct(local);
            VersionNumberStruct vnsRemote = GetVersionNumberStruct(remote);
            if (vnsLocal.MainVersionNumber > vnsRemote.MainVersionNumber)
                return 1;
            if (vnsLocal.MainVersionNumber < vnsRemote.MainVersionNumber)
                return -1;
            if (vnsLocal.MainVersionNumber == vnsRemote.MainVersionNumber)
            {
                if (vnsLocal.SecVersionNumber > vnsRemote.SecVersionNumber)
                    return 1;
                if (vnsLocal.MainVersionNumber < vnsRemote.MainVersionNumber)
                    return -1;
                if (vnsLocal.MainVersionNumber == vnsRemote.MainVersionNumber)
                {
                    if (vnsLocal.MidVersionNumber > vnsRemote.MidVersionNumber)
                        return 1;
                    if (vnsLocal.MidVersionNumber < vnsRemote.MidVersionNumber)
                        return -1;
                    if (vnsLocal.MidVersionNumber == vnsRemote.MidVersionNumber)
                    {
                        if (vnsLocal.MinVersionNumber > vnsRemote.MinVersionNumber)
                            return 1;
                        if (vnsLocal.MinVersionNumber < vnsRemote.MinVersionNumber)
                            return -1;
                        if (vnsLocal.MinVersionNumber == vnsRemote.MinVersionNumber)
                            return 0;
                    }
                }
            }
            throw new NotImplementedException();
        }
    }

    public static class AUpdate
    {
        public const char FileDivide = '>';
        public const char FileInfoDivide = '|';
        public const string RemoteFileListUrl = AHelper.RemoteUrl + @"AFileList.ini";
        public struct FileInfo
        {
            public FileInfo(string fileName, string fileMd5, string fileRoute)
            {
                FileName = fileName;
                FileMd5 = fileMd5;
                FileRoute = fileRoute;
            }
            public string FileRoute { get; private set; }
            public string FileName { get; private set; }
            public string FileMd5 { get; private set; }
        }

        /// <summary>
        /// 更新并重启Alterful，若无更新则不操作
        /// </summary>
        /// <exception cref="WebException"></exception>
        public static void UpdateAndRestart(object msgHandler)
        {
            // 已是最新或内测版本
            if (AVersion.GetVersionNumberDiffer() >= 0) return;

            float count = 0.0f;
            bool updateSelf = false;
            AHelper.AppendString handler = msgHandler as AHelper.AppendString;
            using (var client = new WebClient())
            {
                handler("[update] Getting file list...", AInstruction.ReportType.NONE);
                List<FileInfo> differFiles = GetFilesDiffer();
                Console.WriteLine((100 * count / differFiles.Count) + "%");
                try
                {
                    foreach (FileInfo differFile in differFiles)
                    {
                       if ("Alterful.exe".ToLower() == differFile.FileName.Trim().ToLower())
                        {
                            updateSelf = true;
                            client.DownloadFile(AHelper.RemoteUrl + differFile.FileName, differFile.FileRoute + "Alterful.exe.temp");
                        }
                        else
                        {
                            Directory.CreateDirectory(differFile.FileRoute);
                            client.DownloadFile(AHelper.RemoteUrl + differFile.FileName, differFile.FileRoute + differFile.FileName);
                        }
                        count++;
                        handler("[update] Updating " + differFile.FileName + "... (" + count + "/" + differFiles.Count + ", " + (100 * count / differFiles.Count) + "%)", AInstruction.ReportType.NONE);
                    }
                    handler("Update finished.", AInstruction.ReportType.OK);
                }
                catch (Exception) { throw; }
                
            }
            if(updateSelf)
            {
                // Restart.
                handler("The update just now needs to be restarted to take full effect.", AInstruction.ReportType.WARNING);
                handler("[restart] Alterful will auto restart in 3 seconds.", AInstruction.ReportType.NONE);
                Thread.Sleep(3000);

                System.IO.File.Delete(@".\restart.bat");
                using (StreamWriter writer = new StreamWriter(@".\restart.bat", true))
                {
                    // Write out restart.bat
                    writer.WriteLine(@"@echo off & cls");
                    writer.WriteLine(@"taskkill /f /im alterful.exe");
                    writer.WriteLine(@"ping 127.0.01 -n 1");
                    writer.WriteLine(@"copy /y " + AHelper.BASE_PATH + @"\Alterful.exe.temp " + AHelper.BASE_PATH + @"\Alterful.exe");
                    writer.WriteLine(@"del /f /s /q " + AHelper.BASE_PATH + @"\Alterful.exe.temp");
                    writer.WriteLine(@"start " + AHelper.BASE_PATH + @"\Alterful.exe");

                    // Execute restart.bat
                    Process newProcess = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = true,
                            Arguments = "",
                            FileName = @".\restart.bat",
                        }
                    };
                    newProcess.StartInfo.Verb = "runas";
                    newProcess.Start();
                }
            }
        }

        public static List<FileInfo> GetFilesDiffer()
        {
            List<FileInfo> differFiles = new List<FileInfo>();
            foreach(FileInfo remoteFile in GetRemoteFileList())
            {
                string localFileMd5 = AHelper.GetFileMd5(remoteFile.FileRoute + remoteFile.FileName).ToLower();
                if (!localFileMd5.Equals(remoteFile.FileMd5.ToLower()))
                    differFiles.Add(remoteFile);
            }
            return differFiles;
        }

        /// <summary>
        /// 获取远程文件列表
        /// </summary>
        /// <param name="fileListString"></param>
        /// <returns></returns>
        public static List<FileInfo> GetRemoteFileList() => GetRemoteFileList(GetRemoteFileListString());

        /// <summary>
        /// 获取远程文件列表
        /// </summary>
        /// <param name="fileListString"></param>
        /// <returns></returns>
        public static List<FileInfo> GetRemoteFileList(string fileListString)
        {
            List<FileInfo> fileInfo = new List<FileInfo>();
            foreach(string info in fileListString.Split(FileDivide)){
                List<string> infoDetile = new List<string>(info.Split(FileInfoDivide));
                if (infoDetile.Count != 3) continue;
                fileInfo.Add(new FileInfo(infoDetile[0], infoDetile[1], infoDetile[2]));
            }
            return fileInfo;
        }

        /// <summary>
        /// 获取远程文件列表文本
        /// </summary>
        /// <returns></returns>
        public static string GetRemoteFileListString()
        {
            string fileListBody = "";
            HttpWebRequest request = WebRequest.Create(RemoteFileListUrl) as HttpWebRequest;
            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                fileListBody = reader.ReadToEnd();
            return fileListBody;
        }
    }

    public static class SysEnviroment
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

    public static class SysRegedit
    {
        public static string GetKeyValue(string keyName, string valueName)
        {
            return Registry.GetValue(keyName, valueName, null) as  string;
        }

        public static void SetKeyValue(string keyName, string valueName, string value)
        {
            Registry.SetValue(keyName, valueName, value);
        }
        
        public static void DeleteValue(RegistryKey rk, string subKeyName, string valueName)
        {
            rk.OpenSubKey(subKeyName, true).DeleteValue(valueName);
        }

        public static void DeleteKeyTree(RegistryKey rk, string subKeyName,string subkey)
        {
            rk.OpenSubKey(subKeyName, true).DeleteSubKeyTree(subkey, true);
        }
    }
}
