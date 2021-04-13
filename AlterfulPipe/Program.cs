using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Threading;
using System.Security.Principal;
using System.IO;
using System.Diagnostics;

namespace AlterfulPipe
{
    class Program
    {
        static string targetPath = "";
        static void Main(string[] args)
        {
            string selfLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            string alterfulPath = Path.GetDirectoryName(selfLocation) + @"\Alterful.exe";

            // No arg: just startup alterful
            if (args.Count() != 1) { StartAlterful(alterfulPath, ""); return; }

            // One arg: add as alterful startup item
            targetPath = args[0].Trim();
            if (!System.IO.File.Exists(targetPath) && !System.IO.Directory.Exists(targetPath)) return;

            // Is alterful running
            bool flag = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Alterful", out flag);
            if (!flag) // running
            {
                Thread pipeThread = new Thread(new ThreadStart(SendData));
                pipeThread.IsBackground = true;
                pipeThread.Start();
                Thread.Sleep(251);
            }
            else // not running
            {
                mutex.Close();

                // Start alterful
                StartAlterful(alterfulPath, targetPath);
                Thread.Sleep(996);
            }
        }

        private static void SendData()
        {
            try
            {
                NamedPipeClientStream _pipeClient = new NamedPipeClientStream(".", "StartupAddPipe", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                _pipeClient.Connect();
                StreamWriter sw = new StreamWriter(_pipeClient);
                sw.WriteLine(targetPath);
                sw.Flush();
                Thread.Sleep(1000);
                sw.Close();
            }
            catch (Exception) { /* Who cares? */ }
        }

        private static void StartAlterful(string alterfulPath, string arg)
        {
            if (!File.Exists(alterfulPath)) return;
            Process newProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = true,
                    Arguments = arg,
                    FileName = alterfulPath,
                }
            };
            newProcess.StartInfo.Verb = "runas";
            newProcess.Start();
        }
    }
}
