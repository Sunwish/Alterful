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
            AHelper.CreateShortcut(AHelper.APATH_PATH + @"\demo" + AHelper.LNK_EXTENTION, FilesExamplePath + @"\" + ExampleFileName);
            
            // ----Demo Start----
            AInstruction_Startup ins = new AInstruction_Startup("demo demo-f");
            ins.Execute();
            // -----Demo End-----

        }
        public MainWindow()
        {
            InitializeComponent();

            MainTest();

            Close();
        }
    }
}
