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
            // new AInstruction_Macro("f").Execute();
            // new AInstruction_Macro(@"@del f").Execute();
            // -----Demo End-----


            // ----Test For fun----
            string yourChromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            AInstruction.GetInstruction("@add chrome " + yourChromePath).Execute();
            AInstruction.GetInstruction("@add #fy chrome-o/https://fanyi.baidu.com/#zh/en/").Execute();
            AInstruction.GetInstruction("#fy+键盘增强").Execute();
            // ----Test For fun----

        }
        public MainWindow()
        {
            // Global Initialization
            AHelper.Initialize();

            InitializeComponent();

            // MainTest();

            // Close();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteInstruction(InstructionTextBox.Text);
            InstructionTextBox.Text = "";
        }

        private void InstructionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ExecuteInstruction(InstructionTextBox.Text);
                InstructionTextBox.Text = "";
            }
        }

        private void ExecuteInstruction(string instruction)
        {
            AInstruction.GetInstruction(instruction).Execute();
        }
    }
}
