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
using Alterful.GlobalHotKey;
using System.Collections.ObjectModel;
using System.Windows.Interop;

namespace Alterful
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 当前窗口句柄
        /// </summary>
        private IntPtr m_Hwnd = new IntPtr();

        /// <summary>
        /// 记录快捷键注册项的唯一标识符
        /// </summary>
        private Dictionary<AHotKeySetting, int> m_HotKeySettings = new Dictionary<AHotKeySetting, int>();

        double outputWidthMax = 0;
        bool showOutput = true;
        bool constInstructionInputMode = false;
        double constInstructionInputWidthBias = 120;
        ConstInstructionContentRange constInstructionContentRange = new ConstInstructionContentRange();

        struct ConstInstructionContentRange
        {
            public TextPointer ContentStart;
            public TextPointer ContentEnd;
        }
        /// <summary>
        /// 窗体加载完成后事件处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HotKeySettingsManager.Instance.RegisterGlobalHotKeyEvent += Instance_RegisterGlobalHotKeyEvent;
        }

        /// <summary>
        /// WPF窗体的资源初始化完成，并且可以通过WindowInteropHelper获得该窗体的句柄用来与Win32交互后调用
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 获取窗体句柄
            m_Hwnd = new WindowInteropHelper(this).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(m_Hwnd);
            // 添加处理程序
            if (hWndSource != null) hWndSource.AddHook(WndProc);
        }

        /// <summary>
        /// 所有控件初始化完成后调用
        /// </summary>
        /// <param name="e"></param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // 注册热键
            InitHotKey();
        }

        /// <summary>
        /// 通知注册系统快捷键事件处理函数
        /// </summary>
        /// <param name="hotKeyModelList"></param>
        /// <returns></returns>
        private bool Instance_RegisterGlobalHotKeyEvent(ObservableCollection<HotKeyModel> hotKeyModelList)
        {
            return InitHotKey(hotKeyModelList);
        }

        /// <summary>
        /// 初始化注册快捷键
        /// </summary>
        /// <param name="hotKeyModelList">待注册热键的项</param>
        /// <returns>true:保存快捷键的值；false:弹出设置窗体</returns>
        private bool InitHotKey(ObservableCollection<HotKeyModel> hotKeyModelList = null)
        {
            var list = hotKeyModelList ?? HotKeySettingsManager.Instance.LoadDefaultHotKey();
            // 注册全局快捷键
            string failList = HotKeyHelper.RegisterGlobalHotKey(list, m_Hwnd, out m_HotKeySettings);
            if (string.IsNullOrEmpty(failList))
                return true;
            MessageBoxResult mbResult = MessageBox.Show(string.Format("无法注册下列快捷键\n\r{0}", failList), "提示", MessageBoxButton.OK);
            return false;
        }

        /// <summary>
        /// 窗体回调函数，接收所有窗体消息的事件处理函数
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="msg">消息</param>
        /// <param name="wideParam">附加参数1</param>
        /// <param name="longParam">附加参数2</param>
        /// <param name="handled">是否处理</param>
        /// <returns>返回句柄</returns>
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            var hotkeySetting = new AHotKeySetting();
            // Console.WriteLine(wideParam.ToInt32());
            switch (msg)
            {
                case HotKeyManager.WM_HOTKEY:
                    int sid = wideParam.ToInt32();
                    if (sid == m_HotKeySettings[AHotKeySetting.AWAKEN])
                    {
                        hotkeySetting = AHotKeySetting.AWAKEN;
                        //TODO AWAKEN
                        Visibility = IsVisible ? Visibility.Hidden : Visibility.Visible;
                        showOutput = constInstructionInputMode; Resize();
                        if (Visibility == Visibility.Visible) InstructionTextBox.Focus();
                    }
                    else if(constInstructionInputMode && sid == m_HotKeySettings[AHotKeySetting.CANCEL_CONST_INSTRUCTION_INPUT])
                    {
                        constInstructionInputMode = false;
                        TestRichTextbox.IsReadOnly = true; InstructionTextBox.IsEnabled = true;

                        // Get const instruction lines and update max width.
                        foreach(string ciLine in GetConstInstructionInputLines()) { UpdateMaxWidth(ciLine); }

                        string outputMsg = "\nOperation [" + InstructionTextBox.Text + "] cancelled.";
                        UpdateMaxWidth(outputMsg);
                        AppendRTBLine(TestRichTextbox, outputMsg, Brushes.MintCream, Brushes.Red);
                        TestRichTextbox.BorderThickness = new Thickness(1, 1, 1, 0);
                        InstructionTextBox.Focus(); showOutput = true;
                        Resize();
                    }
                    else if(constInstructionInputMode && sid == m_HotKeySettings[AHotKeySetting.CONFIRM_CONST_INSTRUCTION_INPUT])
                    {
                        ConstInstructionItem ciItem = AInstruction_Macro.GetConstInstructionItem(InstructionTextBox.Text);
                        constInstructionInputMode = false;
                        TestRichTextbox.IsReadOnly = true; InstructionTextBox.IsEnabled = true;

                        string outputHead = ciItem.ConstInstruction + "(";
                        foreach (string paramName in ciItem.ParameterList)
                        {
                            outputHead += paramName + ",";
                        }
                        outputHead = (outputHead[outputHead.Length - 1] == ',' ? outputHead.Substring(0, outputHead.Length - 1) : outputHead) + ")";
                        string output = "";

                        // Get const instruction lines and update max width.
                        foreach (string ciLine in GetConstInstructionInputLines())
                        {
                            output += ciLine + "\n";
                            UpdateMaxWidth(ciLine);
                        }

                        string outputMsg = "";
                        SolidColorBrush colorBackground = Brushes.DarkGreen;
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(AHelper.CONST_INSTRUCTION_PATH + @"\" + outputHead, false))
                            {
                                writer.WriteLine(output);
                            }
                            outputMsg = "\nAdd const instruction [" + InstructionTextBox.Text + "] successfully.";
                        }
                        catch(Exception exception)
                        {
                            outputMsg = "\n" + exception.Message;
                            colorBackground = Brushes.Red;
                        }
                        finally
                        {
                            UpdateMaxWidth(outputMsg);
                            AppendRTBLine(TestRichTextbox, outputMsg, Brushes.MintCream, colorBackground);
                            TestRichTextbox.BorderThickness = new Thickness(1, 1, 1, 0);
                            InstructionTextBox.Focus(); showOutput = true;
                            InstructionTextBox.Text = "";
                            Resize();
                        }
                        
                    }
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 获取当前录入的常指令文本
        /// </summary>
        private string GetConstInstructionContent()
        {
            constInstructionContentRange.ContentEnd = TestRichTextbox.Document.ContentEnd;
            return new TextRange(constInstructionContentRange.ContentStart, constInstructionContentRange.ContentEnd).Text;
        }

        /// <summary>
        /// 虎丘当前录入的常指令文本行
        /// </summary>
        /// <returns></returns>
        private List<string> GetConstInstructionInputLines()
        {
            List<string> ciLines = new List<string>();
            foreach (string ciLine in GetConstInstructionContent().Split('\n'))
            {
                if (ciLine.Trim() != "") ciLines.Add(ciLine.Trim());
            }
            return ciLines;
        }

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
            UpdateMaxWidth(InstructionTextBox.Text);

            // Execute instruction.
            string retnInfo = ExecuteInstruction(InstructionTextBox.Text);
            if (AInstruction.ADD_CONST_INSTRUCTION == retnInfo)
            {
                constInstructionInputMode = true;
                TestRichTextbox.IsReadOnly = false; InstructionTextBox.IsEnabled = false;
                AppendRTBLine(TestRichTextbox, "Confirm: Alt + S / Cancel: Alt + Esc", Brushes.DarkBlue, Brushes.Gold);
                TestRichTextbox.BorderThickness = new Thickness(1);

                // If ci already exist
                string constInstruction = AInstruction_Const.GetConstInstructionFromMacroInstruction(InstructionTextBox.Text);
                if (AConstInstruction.Exist(constInstruction))
                {
                    ConstInstruction ci = new ConstInstruction();
                    if (AConstInstruction.GetConstInstructionFrame(constInstruction, ref ci))
                    {
                        foreach (string insLine in ci.instructionLines)
                        {
                            AppendRTBLine(TestRichTextbox, insLine, Brushes.MintCream, Brushes.Black);
                            UpdateMaxWidth(insLine);
                        }
                    }
                }
                constInstructionContentRange.ContentStart = TestRichTextbox.CaretPosition.Paragraph.ContentStart;
                TestRichTextbox.CaretPosition.Paragraph.IsEnabled = false;
                UpdateMaxWidth("Confirm: Alt + S / Cancel: Alt + Esc");
                Resize(true, constInstructionInputWidthBias);
                TestRichTextbox.Focus(); TestRichTextbox.CaretPosition = TestRichTextbox.Document.ContentEnd; showOutput = true;
                return;
            }

            // Append instruction line.
            AppendRTBLine(TestRichTextbox, InstructionTextBox.Text, Brushes.MintCream, Brushes.Black);

            // Print report.
            foreach (var reportInfo in AInstruction.ReportInfo)
            {
                AppendRTBLine(TestRichTextbox, reportInfo, Brushes.DarkBlue, Brushes.Gold);
                UpdateMaxWidth(reportInfo);
                // If have reportInfo, then show outputBox.
                showOutput = true;
            }

            // Print return information.
            SolidColorBrush bgcolor;
            SolidColorBrush fgcolor;
            switch (AInstruction.reportType)
            {
                case AInstruction.ReportType.OK: bgcolor = Brushes.DarkGreen; fgcolor = Brushes.MintCream; if (AInstruction.ReportInfo.Count == 0) showOutput = false; break;
                case AInstruction.ReportType.WARNING: bgcolor = Brushes.Gold; fgcolor = Brushes.DarkBlue; showOutput = true; break;
                case AInstruction.ReportType.ERROR: bgcolor = Brushes.Red; fgcolor = Brushes.MintCream; showOutput = true; break;
                default: bgcolor = Brushes.SlateGray; fgcolor = Brushes.DarkBlue; break;
            }
            if (retnInfo != "") AppendRTBLine(TestRichTextbox, retnInfo.Trim(), fgcolor, bgcolor);

            if (AInstruction.GetType(InstructionTextBox.Text) == InstructionType.CMD) { InstructionTextBox.Text = "> "; showOutput = true; }
            else InstructionTextBox.Text = "";
            InstructionTextBox.SelectionStart = InstructionTextBox.Text.Length;

            // Update width and resize.
            UpdateMaxWidth(retnInfo);
            Resize();
        }

        /// <summary>
        /// 更新最大行款记录
        /// </summary>
        /// <param name="line">欲参与更新的行文本</param>
        public void UpdateMaxWidth(string line)
        {
            if (MeasureString(InstructionTextBox, line).Width > outputWidthMax) outputWidthMax = MeasureString(InstructionTextBox, line).Width;
        }

        /// <summary>
        /// 重新调整窗口尺寸
        /// </summary>
        /// <param name="resizeHeight"></param>
        /// <param name="widthBias">测量宽度的人工偏移量</param>
        private void Resize(bool resizeHeight = true, double widthBias = 15, double heightBias = 0)
        {
            // Measure and set width, height.
            double insWidth = MeasureString(InstructionTextBox, InstructionTextBox.Text).Width;
            Width = (showOutput ? (insWidth > outputWidthMax ? insWidth : outputWidthMax) : insWidth) + widthBias;
            TestRichTextbox.Visibility = showOutput ? Visibility.Visible : Visibility.Hidden;
            if (resizeHeight) Height = (showOutput ? TestRichTextbox.ExtentHeight : 0) + InstructionTextBox.Height + heightBias;

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
            if (scrollToEnd) TestRichTextbox.ScrollToEnd();
        }

        /// <summary>
        /// 指令框按键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstructionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && "" != InstructionTextBox.Text)
            {
                e.Handled = true;
                if (!constInstructionInputMode) { ExecuteTextBoxInstrution(); }
                else { e.Handled = false; Resize(true, 12, 500); InstructionTextBox.Height = 500; InstructionTextBox.MaxLines = 500; }
                return;
            }
            if (e.Key == Key.Enter && "" == InstructionTextBox.Text) {
                Visibility = Visibility.Hidden; showOutput = false; Resize(); }
            if (e.Key == Key.Escape) { Visibility = Visibility.Hidden; showOutput = false; Resize(); }
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
                return;
            }
            if (e.Key == Key.Down)
            {
                if (-1 == AHelper.InstructionPointer) return;
                if (0 == AHelper.InstructionPointer) { AHelper.InstructionPointer--; InstructionTextBox.Text = ""; return; }
                AHelper.InstructionPointer--;
                InstructionTextBox.Text = AHelper.InstructionHistory[AHelper.InstructionPointer];
                InstructionTextBox.SelectionStart = InstructionTextBox.Text.Length;
                return;
            }
            AHelper.InstructionPointer = -1;
        }

        /// <summary>
        /// 输出框按键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestRichTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Visibility = Visibility.Hidden; showOutput = false; Resize(); }
        }

        /// <summary>
        /// 指令框文本改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstructionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Resize(constInstructionInputMode);
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
            if ("~" == instruction) Close();
            try { return AInstruction.GetInstruction(instruction).Execute(); }
            catch(Exception exception) { return exception.Message; }
        }

        private void TestRichTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (constInstructionInputMode) Resize(true, constInstructionInputWidthBias);
        }
    }
}
