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
        double outputWidthMax = 0;
        bool showOutput = true;
        public void MainTest()
        {
            // Sample Initialization
            string FilesExamplePath = AHelper.BASE_PATH + @"\FilesExample", ExampleFileName = "demoFile.txt";
            Directory.CreateDirectory(FilesExamplePath);
            using (StreamWriter streamWriter = new StreamWriter(FilesExamplePath + @"\" + ExampleFileName, false))
            {
                streamWriter.WriteLine("Hello Alterful!");
            }
            AFile.Add("demo", FilesExamplePath + @"\" + ExampleFileName);

            // ----Instruction Sample----
            // new AInstruction_Startup("demo demo-f").Execute();
            // new AInstruction_Macro("@new a.txt b.txt").Execute();
            // new AInstruction_Macro(@"@add f f:\").Execute();
            // new AInstruction_Macro("f").Execute();
            // new AInstruction_Macro(@"@del f").Execute();
            // -----Demo End-----

            // ----Sample For fun----
            string yourChromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            AInstruction.GetInstruction("@add chrome " + yourChromePath).Execute();
            AInstruction.GetInstruction("@add #fy chrome-o/https://fanyi.baidu.com/#zh/en/").Execute();
            AInstruction.GetInstruction("#fy+键盘增强").Execute();
            // ----Sample For fun----

        }
        public MainWindow()
        {
            // Global Initialization.
            AHelper.Initialize();
            InitializeComponent();
            InitializeGUI();

            // Instruction Test.
            // MainTest();
            // Close();
        }

        private void InitializeGUI()
        {
            Resize();
            InstructionTextBox.Focus();
        }

        /// <summary>
        /// 执行指令框中的指令
        /// </summary>
        private void ExecuteTextBoxInstrution()
        {
            // Append instruction line.
            AppendRTBLine(TestRichTextbox, InstructionTextBox.Text, Brushes.MintCream, Brushes.Black);

            // Execute instruction.
            string retnInfo = ExecuteInstruction(InstructionTextBox.Text);
            if (AInstruction.GetType(InstructionTextBox.Text) == InstructionType.CMD) { InstructionTextBox.Text = "> "; showOutput = true; }
            else InstructionTextBox.Text = "";

            // Print report.
            InstructionTextBox.SelectionStart = InstructionTextBox.Text.Length;
            foreach (var reportInfo in AInstruction.ReportInfo)
            {
                AppendRTBLine(TestRichTextbox, reportInfo, Brushes.DarkBlue, Brushes.Gold);
                UpdateMaxWidth(reportInfo);
                // If have reportInfo, then show outputBox.
                showOutput = true;
            }

            // Print return information.
            SolidColorBrush bgcolor;
            switch (AInstruction.reportType)
            {
                case AInstruction.ReportType.OK: bgcolor = Brushes.DarkGreen; if(AInstruction.ReportInfo.Count == 0) showOutput = false; break;
                case AInstruction.ReportType.WARNING: bgcolor = Brushes.Gold; showOutput = true; break;
                case AInstruction.ReportType.ERROR: bgcolor = Brushes.Red; showOutput = true; break;
                default: bgcolor = Brushes.SlateGray; break;
            }
            if (retnInfo != "") AppendRTBLine(TestRichTextbox, retnInfo.Trim(), Brushes.MintCream, bgcolor);

            // Update width and resize.
            UpdateMaxWidth(retnInfo);
            Resize();
        }

        /// <summary>
        /// 更新最大行款记录
        /// </summary>
        /// <param name="line">欲参与更新的行文本</param>
        private void UpdateMaxWidth(string line)
        {
            if (MeasureString(InstructionTextBox, line).Width > outputWidthMax) outputWidthMax = MeasureString(InstructionTextBox, line).Width;
        }

        /// <summary>
        /// 重新调整窗口尺寸
        /// </summary>
        /// <param name="resizeHeight"></param>
        /// <param name="bias">测量宽度的人工偏移量</param>
        private void Resize(bool resizeHeight = true, double bias = 12)
        {
            // Measure and set width, height.
            double insWidth = MeasureString(InstructionTextBox, InstructionTextBox.Text).Width;
            Width = (showOutput ? (insWidth > outputWidthMax ? insWidth : outputWidthMax) : insWidth) + bias;
            TestRichTextbox.Visibility = showOutput ? Visibility.Visible : Visibility.Hidden;
            if (resizeHeight) Height = (showOutput ? TestRichTextbox.ExtentHeight : 0) + InstructionTextBox.Height;

            // Window Left-Bottom.
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            Top = desktopWorkingArea.Height - Math.Min(this.Height, this.MaxHeight) - 40;
            Left = 0;
        }

        /// <summary>
        /// 向富文本框中添加新行
        /// </summary>
        /// <param name="rtb">欲添加新行的富文本框</param>
        /// <param name="appendString">欲添加的新行</param>
        /// <param name="foregroundColor">新行文本前景色</param>
        /// <param name="backgroundColor">新行文本背景色</param>
        /// <param name="scrollToEnd">自动滚动至最后</param>
        private void AppendRTBLine(RichTextBox rtb, string appendString, SolidColorBrush foregroundColor, SolidColorBrush backgroundColor, bool scrollToEnd = true)
        {
            TextRange tr = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
            tr.Text = appendString + "\n";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, foregroundColor);
            tr.ApplyPropertyValue(TextElement.BackgroundProperty, backgroundColor);
            if(scrollToEnd) TestRichTextbox.ScrollToEnd();
        }

        /// <summary>
        /// 指令框按键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstructionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && "" != InstructionTextBox.Text) { ExecuteTextBoxInstrution(); return; }
            if (e.Key == Key.Escape) Close();
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt) { e.Handled = true; showOutput = !showOutput; Resize(); return; }
            if(e.Key == Key.Tab) throw new NotImplementedException("Instruction completion");
        }
        private void InstructionTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right && "" == InstructionTextBox.Text) { InstructionTextBox.Text = "> "; InstructionTextBox.SelectionStart = InstructionTextBox.Text.Length; return; }
            if (e.Key == Key.Up)
            {
                if (AHelper.InstructionHistory.Count - 1 == AHelper.InstructionPointer) return;
                AHelper.InstructionPointer++;
                InstructionTextBox.Text = AHelper.InstructionHistory[AHelper.InstructionPointer];
                InstructionTextBox.SelectionStart = InstructionTextBox.Text.Length;
            }
            if (e.Key == Key.Down)
            {
                if (-1 == AHelper.InstructionPointer) return;
                if (0 == AHelper.InstructionPointer) { AHelper.InstructionPointer--; InstructionTextBox.Text = ""; return; }
                AHelper.InstructionPointer--;
                InstructionTextBox.Text = AHelper.InstructionHistory[AHelper.InstructionPointer];
                InstructionTextBox.SelectionStart = InstructionTextBox.Text.Length;
            }
        }

        /// <summary>
        /// 输出框按键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestRichTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        /// <summary>
        /// 指令框文本改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstructionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Resize(false);
        }

        /// <summary>
        /// 测量文本尺寸
        /// </summary>
        /// <param name="textbox">提供字体参数的文本框</param>
        /// <param name="candidate">待测量文本行</param>
        /// <returns></returns>
        private Size MeasureString(TextBox textbox, string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textbox.FontFamily, textbox.FontStyle, textbox.FontWeight, textbox.FontStretch),
                textbox.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode.Display);
            return new Size(formattedText.Width, formattedText.Height);
        }

        /// <summary>
        /// 执行指令
        /// </summary>
        /// <param name="instruction">欲执行的指令</param>
        /// <returns></returns>
        private string ExecuteInstruction(string instruction)
        {
            try { return AInstruction.GetInstruction(instruction).Execute(); }
            catch(Exception exception) { return exception.Message; }
        }
    }
}
