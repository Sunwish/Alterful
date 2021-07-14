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
using System.Windows.Forms;
using System.Drawing;
using Alterful.Instruction;

namespace Alterful.Helper
{
    public static class AHelper
    {
        public static string BASE_PATH { get; } = System.IO.File.Exists(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location)))) + @"\Alterful.sln") ? System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location)))) : System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location);
        public static string APATH_PATH { get; } = BASE_PATH + @"\APath";
        public static string ATEMP_PATH { get; } = BASE_PATH + @"\ATemp";
        public static bool IS_FIRST_START = false;
        public static bool HAS_ANEW = false;
        public static string CONST_INSTRUCTION_PATH { get; } = BASE_PATH + @"\Config\ConstInstruction";
        public const string RemoteUrl = @"https://alterful.com/bin/";
        public const string LNK_EXTENTION = ".lnk";
        public static List<string> InstructionHistory = new List<string>();
        public static int InstructionPointer = -1;
        public delegate void AppendString(string content, AInstruction.ReportType type);

        public static void Initialize(AppendString appendString)
        {
            // Floder Check
            if(!Directory.Exists(BASE_PATH + @"\Config"))
            {
                // First start alterful
                ASettings.DisplayRightMenu = true;
                ASettings.AutoRunWithSystem = true;
                CreateShortcut(APATH_PATH + @"\alterful"  + AFile.LNK_EXTENTION, BASE_PATH + @"\Alterful.exe");
                CreateShortcut(APATH_PATH + @"\atemp" + AFile.LNK_EXTENTION, ATEMP_PATH);
                CreateShortcut(APATH_PATH + @"\desktop" + AFile.LNK_EXTENTION, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                IS_FIRST_START = true;
            }
            
            // Old version auto startup alterful.exe but new version startup AlterfulPiple.exe, 
            // so over execute to overwrite the old auto startup register configurate.
            if (ASettings.AutoRunWithSystem) ASettings.AutoRunWithSystem = true;

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
            ShowANew(appendString);
            ShowANotification("Alterful 正在后台运行", "Press Alt+A to active Alterful");
        }

        private static void ShowANew(AppendString appendString)
        {
            string ANewPath = @".\ANew.ini";
            if (System.IO.File.Exists(ANewPath))
            {
                HAS_ANEW = true;
                using (StreamReader reader = new StreamReader(ANewPath))
                {
                    appendString("在新版本 " + Properties.Settings.Default.localVersion + " 中有以下更新: ", AInstruction.ReportType.OK);
                    //appendString("What's new in version " + Properties.Settings.Default.localVersion + ":", AInstruction.ReportType.OK);
                    foreach (string line in reader.ReadToEnd().Split('\n'))
                    {
                        if (0 == line.Length) continue;
                        if ('*' != line[0])
                        {
                            appendString(line.Trim(), AInstruction.ReportType.NONE);
                        }
                        else if (0 == line.IndexOf("***"))
                        {
                            appendString(line.Trim().Substring(3), AInstruction.ReportType.ERROR);
                        }
                        else if (0 == line.IndexOf("**"))
                        {
                            appendString(line.Trim().Substring(2), AInstruction.ReportType.WARNING);
                        }
                        else if (0 == line.IndexOf("*"))
                        {
                            appendString(line.Trim().Substring(1), AInstruction.ReportType.OK);
                        }
                    }
                }
                System.IO.File.Delete(ANewPath);
            }
        }

        public static void ShowANotification(string title, string text)
        {
            try
            {
                NotifyIcon icon = new NotifyIcon()
                {
                    Text = "Alterful",
                    Icon = new Icon(BASE_PATH + @"\AIcon.ico"),
                    BalloonTipIcon = ToolTipIcon.None,
                    BalloonTipTitle = title,
                    BalloonTipText = text
                };
                icon.Visible = true;
                icon.ShowBalloonTip(3600);
            }
            catch {}
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

        public static void Restart()
        {
            using (StreamWriter writer = new StreamWriter(@".\restart.bat", true))
            {
                // Write out restart.bat
                writer.WriteLine(@"@echo off & cls");
                writer.WriteLine(@"taskkill /f /im alterful.exe");
                writer.WriteLine(@"ping 127.0.01 -n 2");
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

        public struct SoftwareInstalled
        {
            public SoftwareInstalled(string displayName, string installLocation) { DisplayName = displayName; InstallLocation = installLocation; }
            public string DisplayName { get; private set; }
            public string InstallLocation { get; private set; }
        }

        public static List<SoftwareInstalled> GetInstalledSoftwareList()
        {
            string displayName;
            string installLocation;
            List<SoftwareInstalled> gInstalledSoftware = new List<SoftwareInstalled>();

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false))
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    installLocation = subkey.GetValue("InstallLocation") as string;
                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(installLocation))
                        continue;
                    
                    gInstalledSoftware.Add(new SoftwareInstalled(displayName.ToLower(), installLocation.ToLower()));
                }
            }

            //using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false))
            using (var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                var key = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false);
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    installLocation = subkey.GetValue("InstallLocation") as string;
                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(installLocation))
                        continue;

                    gInstalledSoftware.Add(new SoftwareInstalled(displayName.ToLower(), installLocation.ToLower()));
                }
            }

            using (var localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                var key = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false);
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    installLocation = subkey.GetValue("InstallLocation") as string;
                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(installLocation))
                        continue;

                    gInstalledSoftware.Add(new SoftwareInstalled(displayName.ToLower(), installLocation.ToLower()));
                }
            }

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false))
            {
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    displayName = subkey.GetValue("DisplayName") as string;
                    installLocation = subkey.GetValue("InstallLocation") as string;
                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(installLocation))
                        continue;

                    gInstalledSoftware.Add(new SoftwareInstalled(displayName.ToLower(), installLocation.ToLower()));
                }
            }
            return gInstalledSoftware;
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
        public static string GetRemoteVersionNumber(AHelper.AppendString appendString)
        {
            HttpWebRequest request = WebRequest.Create(RemoteVersionUrl) as HttpWebRequest;
            try
            {
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    string versionInfoBody = reader.ReadToEnd();
                    return versionInfoBody.Substring(versionInfoBody.IndexOf("=") + 1);
                }
            }
            catch (Exception)
            {
                // Do nothing, skip update checking.
                appendString("检查更新出错，请检查网络连接状况，\n可使用@restart来重启Alterful检查更新，或使用@update来检查并执行更新。", AInstruction.ReportType.ERROR);
                return "0.0.0.0";
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
        public static int GetVersionNumberDiffer(AHelper.AppendString appendString) => GetVersionNumberDiffer(GetLocalVersionNumber(), GetRemoteVersionNumber(appendString));

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
                if (vnsLocal.SecVersionNumber < vnsRemote.SecVersionNumber)
                    return -1;
                if (vnsLocal.SecVersionNumber == vnsRemote.SecVersionNumber)
                {
                    if (vnsLocal.MidVersionNumber > vnsRemote.MidVersionNumber)
                        return 1;
                    if (vnsLocal.MidVersionNumber < vnsRemote.MidVersionNumber)
                        return -1;
                    if (vnsLocal.MidVersionNumber == vnsRemote.MidVersionNumber)
                        return 0;
                    /* Ignore MinVersion
                    if (vnsLocal.MidVersionNumber == vnsRemote.MidVersionNumber)
                    {
                        if (vnsLocal.MinVersionNumber > vnsRemote.MinVersionNumber)
                            return 1;
                        if (vnsLocal.MinVersionNumber < vnsRemote.MinVersionNumber)
                            return -1;
                        if (vnsLocal.MinVersionNumber == vnsRemote.MinVersionNumber)
                            return 0;
                    }*/
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
            // if (AVersion.GetVersionNumberDiffer() >= 0) return;

            float count = 0.0f;
            bool updateSelf = false;
            AHelper.AppendString handler = msgHandler as AHelper.AppendString;
            using (var client = new WebClient())
            {
                handler("[更新] 正在获取文件列表...", AInstruction.ReportType.OK);
                //handler("[update] Getting file list...", AInstruction.ReportType.NONE);
                List<FileInfo> differFiles = new List<FileInfo>();
                try
                {
                    differFiles = GetFilesDiffer(handler);
                }
                catch (Exception)
                {
                    return;
                }
                
                if (0 == differFiles.Count) {
                    handler("当前版本已经是最新版。", AInstruction.ReportType.OK);
                    //handler("The current version is already the latest version.", AInstruction.ReportType.OK); 
                    return;
                }
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
                        handler("[更新] 正在更新 " + differFile.FileName + "... (" + count + "/" + differFiles.Count + ", " + (100 * count / differFiles.Count) + "%)", AInstruction.ReportType.NONE);
                        //handler("[update] Updating " + differFile.FileName + "... (" + count + "/" + differFiles.Count + ", " + (100 * count / differFiles.Count) + "%)", AInstruction.ReportType.NONE);
                    }
                    handler("更新已完成。", AInstruction.ReportType.OK);
                    //handler("Update finished.", AInstruction.ReportType.OK);
                }
                catch (Exception e) { handler("[错误] " + e.Message, AInstruction.ReportType.ERROR); }
                
            }
            if(updateSelf)
            {
                // Restart.
                handler("此次更新需要重新启动 Alterful 才能完全生效。", AInstruction.ReportType.WARNING);
                handler("[重启] Alterful 将在 10 秒后自动重新启动。", AInstruction.ReportType.NONE);
                //handler("The update just now needs to be restarted to take full effect.", AInstruction.ReportType.WARNING);
                //handler("[restart] Alterful will auto restart in 10 seconds.", AInstruction.ReportType.NONE);
                Thread.Sleep(10000);

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
            else
            {
                handler("你可以执行 @restart 来看看更新了些什么内容。", AInstruction.ReportType.NONE);
                //handler("Execute @restart to list what's new.", AInstruction.ReportType.NONE);
            }
        }

        public static List<FileInfo> GetFilesDiffer(AHelper.AppendString appendString)
        {
            List<FileInfo> differFiles = new List<FileInfo>();
            try
            {
                foreach (FileInfo remoteFile in GetRemoteFileList(appendString))
                {
                    string localFileMd5 = AHelper.GetFileMd5(remoteFile.FileRoute + remoteFile.FileName).ToLower();
                    if (!localFileMd5.Equals(remoteFile.FileMd5.ToLower()))
                        differFiles.Add(remoteFile);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return differFiles;
        }

        /// <summary>
        /// 获取远程文件列表
        /// </summary>
        /// <param name="fileListString"></param>
        /// <returns></returns>
        public static List<FileInfo> GetRemoteFileList(AHelper.AppendString appendString)
        {
            try
            {
                return GetRemoteFileList(GetRemoteFileListString(appendString));
            }
            catch (Exception)
            {
                throw;
            }
        }


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
        public static string GetRemoteFileListString(AHelper.AppendString appendString)
        {
            string fileListBody = "";
            HttpWebRequest request = WebRequest.Create(RemoteFileListUrl) as HttpWebRequest;
            try
            {
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                    fileListBody = reader.ReadToEnd();
            }
            catch (Exception e)
            {
                appendString("[错误] 获取文件列表失败，请检查网络状况。", AInstruction.ReportType.ERROR);
                throw;
            }
            
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
