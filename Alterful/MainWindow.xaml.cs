using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using Alterful.Functions;
using Alterful.Helper;

namespace Alterful
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public void MainTest()
        {
            // Global Initialization
            AHelper.Initialize();

            // Demo Initialization
            string FilesExamplePath = AHelper.BASE_PATH + @"\FilesExample", ExampleFileName = "demoFile.txt";
            Directory.CreateDirectory(FilesExamplePath);
            using (StreamWriter streamWriter = new StreamWriter(FilesExamplePath + @"\" + ExampleFileName, false))
            {
                streamWriter.WriteLine("Hello Alterful!");
            }
            
            AFile.Add("demo", FilesExamplePath + @"\" + ExampleFileName);

            // ----Demo Start----
            // new AInstruction_Startup("demo demo-f").Execute();

            // new AInstruction_Macro("@new a.txt b.txt").Execute();

            // new AInstruction_Macro(@"@add f f:\").Execute();

            // new AInstruction_Macro(@"@del chrome").Execute();

            // -----Demo End-----


            // ----For fun----
            // new AInstruction_Macro(@"@add chrome C:\Program Files (x86)\Google\Chrome\Application\chrome.exe").Execute();
            // new AInstruction_Macro(@"@add vs D:\Visual Studio 2017\Common7\IDE\devenv.exe").Execute();
            // new AInstruction_Startup("chrome-f-o/https://fanyi.baidu.com/").Execute();
            string yourChromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            AInstruction.GetInstruction("@add chrome " + yourChromePath).Execute();
            //AInstruction.GetInstruction("chrome-o/https://fanyi.baidu.com/#zh/en/%E9%94%AE%E7%9B%98%E5%A2%9E%E5%BC%BA").Execute();
            AConstQuote.Add("fy", "chrome-o/https://fanyi.baidu.com/#zh/en/");
            AInstruction.GetInstruction("#fy+键盘增强").Execute();
            //AConstQuote.Delete("addTest");
            //List<AConstQuote.ConstQuoteItem> list = AConstQuote.GetConstQuoteMapConfig();
            //Console.Read();
            // ----For fun----

        }
        public MainWindow()
        {
            InitializeComponent();

            MainTest();

            Close();
        }
    }
}
