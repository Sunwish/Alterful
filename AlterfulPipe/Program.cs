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
            if (args.Count() != 1) return;

            targetPath = args[0].Trim();
            if (!System.IO.File.Exists(targetPath) && !System.IO.Directory.Exists(targetPath)) return;

            // Is alterful is running
            string selfLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            string alterfulPath = Path.GetDirectoryName(selfLocation) + @"\Alterful.exe";

            bool flag = false;
            for(int i = 0; i < 2; i++)
            {
                System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Alterful", out flag);
                if (!flag) // running
                {
                    Thread pipeThread = new Thread(new ThreadStart(SendData));
                    pipeThread.IsBackground = true;
                    pipeThread.Start();
                    Thread.Sleep(251);
                    break;
                }
                else // not running
                {
                    mutex.Close();

                    // Start alterful
                    if (!File.Exists(alterfulPath)) break;
                    Process newProcess = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            UseShellExecute = true,
                            Arguments = "",
                            FileName = alterfulPath,
                        }
                    };
                    newProcess.StartInfo.Verb = "runas";
                    newProcess.Start();

                    Thread.Sleep(996);
                }
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
    }
}
