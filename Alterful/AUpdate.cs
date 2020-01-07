using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alterful.Common.Helper;
using Alterful.Instruction;
namespace Alterful.Update
{
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
            using (System.IO.StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
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
            // if (AVersion.GetVersionNumberDiffer() >= 0) return;

            float count = 0.0f;
            bool updateSelf = false;
            AHelper.AppendString handler = msgHandler as AHelper.AppendString;
            using (var client = new WebClient())
            {
                handler("[update] Getting file list...", AInstruction.ReportType.NONE);
                List<FileInfo> differFiles = GetFilesDiffer();
                if (0 == differFiles.Count) { handler("The current version is already the latest version.", AInstruction.ReportType.OK); return; }
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
            if (updateSelf)
            {
                // Restart.
                handler("The update just now needs to be restarted to take full effect.", AInstruction.ReportType.WARNING);
                handler("[restart] Alterful will auto restart in 10 seconds.", AInstruction.ReportType.NONE);
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
                handler("Execute @restart to list what's new.", AInstruction.ReportType.NONE);
            }
        }

        public static List<FileInfo> GetFilesDiffer()
        {
            List<FileInfo> differFiles = new List<FileInfo>();
            foreach (FileInfo remoteFile in GetRemoteFileList())
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
            foreach (string info in fileListString.Split(FileDivide))
            {
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
}
