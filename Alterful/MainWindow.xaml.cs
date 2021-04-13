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
using Microsoft.Win32;
using System.Threading;
using System.IO.Pipes;
using Alterful.Instruction;

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
        string OldConstInstructionFileName = "";
        bool completing = false;
        AThemeConfig themeConfig = ATheme.GetThemeConfig();
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
                        AppendRTBLine(TestRichTextbox, outputMsg, themeConfig.ForegroundOutputError, themeConfig.BackgroundOutputError);
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
                        SolidColorBrush colorBackground = themeConfig.BackgroundOutputOk;
                        try
                        {
                            try { File.Delete(AHelper.CONST_INSTRUCTION_PATH + @"\" + OldConstInstructionFileName); } catch { }
                            using (StreamWriter writer = new StreamWriter(AHelper.CONST_INSTRUCTION_PATH + @"\" + outputHead, false))
                            {
                                writer.WriteLine(output);
                            }
                            outputMsg = "\nAdd const instruction [" + InstructionTextBox.Text + "] successfully.";
                        }
                        catch(Exception exception)
                        {
                            outputMsg = "\n" + exception.Message;
                            colorBackground = themeConfig.BackgroundOutputError;
                        }
                        finally
                        {
                            UpdateMaxWidth(outputMsg);
                            AppendRTBLine(TestRichTextbox, outputMsg, themeConfig.BackgroundOutput, colorBackground);
                            TestRichTextbox.BorderThickness = new Thickness(1, 1, 1, 0);
                            InstructionTextBox.Focus(); showOutput = true;
                            InstructionTextBox.Text = "";
                            Resize();
                        }
                        
                    }
                    handled = true;
                    break;
            }
            hotkeySetting = hotkeySetting + 0; // Meaningless, just for clearing up warnning
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
        /// 获取当前录入的常指令文本行
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

        static bool GotMutex = false;
        System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Alterful", out GotMutex);
        public MainWindow()
        {
            /*
             * TODO: Automatically initialize default startup items for users who first use Alterful.
            foreach (AHelper.SoftwareInstalled software in AHelper.GetInstalledSoftwareList())
            {
                Console.WriteLine(software.DisplayName + " (" + software.InstallLocation + ")");
            }
            */

            if (!GotMutex)
            {
                AHelper.ShowANotification("Alterful 已在运行中", "Alterful is already running");
                Environment.Exit(1);//退出程序
            }

            // Global Initialization.
            InitializeComponent();
            AHelper.Initialize(delegate (string content, AInstruction.ReportType type)
            {
                // InstructionTextBox 被主线程占用，利用 Dispatcher 进行操作
                TestRichTextbox.Dispatcher.BeginInvoke((Action)(() => {
                    switch (type)
                    {
                        case AInstruction.ReportType.NONE: AppendRTBLine(TestRichTextbox, content, themeConfig.ForegroundOutput, themeConfig.BackgroundOutput); break;
                        case AInstruction.ReportType.WARNING: AppendRTBLine(TestRichTextbox, content, themeConfig.ForegroundOutputWarning, themeConfig.BackgroundOutputWarning); break;
                        case AInstruction.ReportType.OK: AppendRTBLine(TestRichTextbox, content, themeConfig.ForegroundOutputOk, themeConfig.BackgroundOutputOk); break;
                        case AInstruction.ReportType.ERROR: AppendRTBLine(TestRichTextbox, content, themeConfig.ForegroundOutputError, themeConfig.BackgroundOutputError); break;
                    }
                    InstructionTextBox.Text = "";
                    UpdateMaxWidth(content);
                    Resize();
                }));
            });
            InitializeGUI(ATheme.GetThemeConfig(), true);
            InitializePipe();
            CheckCommandLine();

            Thread thread = new Thread(new ThreadStart(CheckUpdate));
            thread.Start();

            // If is first startup, give user some tips.
            if (AHelper.IS_FIRST_START)
            {
                string outInfo = "欢迎来到 Alterful，以下是初次上手帮助！\n- 在[指令框输入框]按 Alt 来隐藏/显示回显框；\n- 使用组合键 [Alt+A] 来唤醒/隐藏指令框；\n- 右键任意文件/文件夹来将其添加为启动项；\n- 在指令输入框键入启动项名并回车来启动它；\n- 需要时键入波浪号(~)并回车来结束 Alterful；\n- 前往 help.alterful.com 了解详细功能和使用技巧。";
                TestRichTextbox.Dispatcher.BeginInvoke((Action)(() => {
                    UpdateMaxWidth(outInfo);
                    AppendRTBLine(TestRichTextbox, outInfo, themeConfig.ForegroundOutputWarning, themeConfig.BackgroundOutputWarning);
                    Visibility = Visibility.Visible;
                    showOutput = true;
                    Resize();
                }));
            }

            // Instruction Test.
            // MainTest();
            // Close();
        }

        private void CheckUpdate()
        {
            if (AVersion.GetVersionNumberDiffer() >= 0) return;
            //string outInfo = "检测到有新版本, 你可以执行 @update 来启用更新程序。"/*"检查到有新版本，执行 @update 来启用更新程序"*/;
            string outInfo = "A new version is detected, execute @update to enable the Updater."/*"检查到有新版本，执行 @update 来启用更新程序"*/;

            // InstructionTextBox 被主线程占用，利用 Dispatcher 进行操作
            TestRichTextbox.Dispatcher.BeginInvoke((Action)(() => {
                UpdateMaxWidth(outInfo);
                AppendRTBLine(TestRichTextbox, outInfo, themeConfig.ForegroundOutputWarning, themeConfig.BackgroundOutputWarning);
                Visibility = Visibility.Visible;
                showOutput = true;
                Resize();
            }));
        }

        private void CheckCommandLine()
        {
            List<string> commandLineArgList = new List<string>(Environment.GetCommandLineArgs());
            if (commandLineArgList.Count == 0) return;
            string path = commandLineArgList[commandLineArgList.Count - 1].Trim();

            if (path != "" && path != AHelper.BASE_PATH + @"\Alterful.exe" && (File.Exists(path) || Directory.Exists(path)))
            {
                string defaultName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                InstructionTextBox.Text = "@add" + " " + defaultName + " " + path;
                InstructionTextBox.SelectionStart = ("@add" + " ").Length;
                InstructionTextBox.SelectionLength = defaultName.Length;
                InstructionTextBox.Focus();
            }
        }

        private void InitializePipe()
        {
            Thread receiveDataThread = new Thread(new ThreadStart(ReceiveDataFromClient));
            receiveDataThread.IsBackground = true;
            receiveDataThread.Start();
           
        }

        private void ReceiveDataFromClient()
        {
            while (true)
            {
                try
                {
                    PipeSecurity pse = new PipeSecurity();
                    pse.SetAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));//设置访问规则
                    NamedPipeServerStream _pipeServer = new NamedPipeServerStream("StartupAddPipe", PipeDirection.InOut, 10, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 1024, 1024, pse, HandleInheritability.None);

                    _pipeServer.WaitForConnection(); //Waiting
                    using (StreamReader sr = new StreamReader(_pipeServer))
                    {
                        string path = sr.ReadLine().Trim();
                        string defaultName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                        // InstructionTextBox 被主线程占用，利用 Dispatcher 进行操作
                        InstructionTextBox.Dispatcher.BeginInvoke((Action)(() => {
                            InstructionTextBox.Text = "@add" + " " + defaultName + " " + path;
                            InstructionTextBox.SelectionStart = ("@add" + " ").Length;
                            InstructionTextBox.SelectionLength = defaultName.Length;
                            Visibility = Visibility.Visible;
                            InstructionTextBox.Focus();
                        }));
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception) { /* I don't care */ }
            }
        }

        private void InitializeGUI(AThemeConfig tc, bool realyInitialize = false)
        {
            Resize();
            themeConfig = tc;
            InstructionTextBox.Background = themeConfig.BackgroundInput;
            InstructionTextBox.Foreground = themeConfig.ForegroundInput;
            TestRichTextbox.Background = themeConfig.BackgroundOutput;
            TestRichTextbox.Foreground = themeConfig.ForegroundOutput;
            InstructionTextBox.Focus();

            // Hide when start up
            if (realyInitialize) Visibility = Visibility.Hidden; showOutput = false; Resize();
        }



        /// <summary>
        /// 执行指令框中的指令
        /// </summary>
        private void ExecuteTextBoxInstrution()
        {
            UpdateMaxWidth(InstructionTextBox.Text);

            // Execute instruction.
            string retnInfo = ExecuteInstruction(InstructionTextBox.Text);
            InitializeGUI(ATheme.GetThemeConfig());
            if (AInstruction.ADD_CONST_INSTRUCTION == retnInfo)
            {
                constInstructionInputMode = true;
                TestRichTextbox.IsReadOnly = false; InstructionTextBox.IsEnabled = false;
                //AppendRTBLine(TestRichTextbox, "确认编辑: Alt + S / 取消编辑: Alt + Esc", themeConfig.ForegroundOutputWarning, themeConfig.BackgroundOutputWarning);
                AppendRTBLine(TestRichTextbox, "Confirm: Alt + S / Cancel: Alt + Esc", themeConfig.ForegroundOutputWarning, themeConfig.BackgroundOutputWarning);
                TestRichTextbox.BorderThickness = new Thickness(1);

                // Content Start
                TestRichTextbox.CaretPosition = TestRichTextbox.Document.ContentEnd;
                constInstructionContentRange.ContentStart = TestRichTextbox.CaretPosition.Paragraph.ContentStart;
                TestRichTextbox.CaretPosition.Paragraph.IsEnabled = false;

                // If ci already exist
                string constInstruction = AInstruction_Const.GetConstInstructionFromMacroInstruction(InstructionTextBox.Text);
                if (AConstInstruction.Exist(constInstruction))
                {
                    ConstInstruction ci = new ConstInstruction();
                    if (AConstInstruction.GetConstInstructionFrame(constInstruction, ref ci))
                    {
                        OldConstInstructionFileName = AConstInstruction.GetFileNameFromConstInstruction(ci);
                        foreach (string insLine in ci.instructionLines)
                        {
                            AppendRTBLine(TestRichTextbox, insLine, themeConfig.ForegroundOutput, themeConfig.BackgroundOutput);
                            UpdateMaxWidth(insLine);
                        }
                    }
                }

                // Set CaretPosition.
                TestRichTextbox.CaretPosition = TestRichTextbox.Document.ContentEnd;

                //UpdateMaxWidth("确认编辑: Alt + S / 取消编辑: Alt + Esc");
                UpdateMaxWidth("Confirm: Alt + S / Cancel: Alt + Esc");
                Resize(true, constInstructionInputWidthBias);
                TestRichTextbox.Focus(); showOutput = true;
                return;
            }
            else if (AInstruction.UPDATE_INSTRUCTION == retnInfo)
            {
                UpdateMaxWidth(InstructionTextBox.Text);
                AppendRTBLine(TestRichTextbox, InstructionTextBox.Text, themeConfig.ForegroundOutput, themeConfig.BackgroundOutput);
                InstructionTextBox.Text = "";
                Resize();
                AHelper.AppendString appendString = delegate (string content, AInstruction.ReportType type)
                {
                    // InstructionTextBox 被主线程占用，利用 Dispatcher 进行操作
                    TestRichTextbox.Dispatcher.BeginInvoke((Action)(() => {
                        switch (type)
                        {
                            case AInstruction.ReportType.NONE: AppendRTBLine(TestRichTextbox, content, themeConfig.ForegroundOutput, themeConfig.BackgroundOutput); break;
                            case AInstruction.ReportType.WARNING: AppendRTBLine(TestRichTextbox, content, themeConfig.ForegroundOutputWarning, themeConfig.BackgroundOutputWarning); break;
                            case AInstruction.ReportType.OK: AppendRTBLine(TestRichTextbox, content, themeConfig.ForegroundOutputOk, themeConfig.BackgroundOutputOk); break;
                            case AInstruction.ReportType.ERROR: AppendRTBLine(TestRichTextbox, content, themeConfig.ForegroundOutputError, themeConfig.BackgroundOutputError); break;
                        }
                        InstructionTextBox.Text = "";
                        UpdateMaxWidth(content);
                        Resize();
                    }));
                };

                new Thread(AUpdate.UpdateAndRestart).Start(appendString);
                return;
            }

            // Append instruction line.
            AppendRTBLine(TestRichTextbox, InstructionTextBox.Text, themeConfig.ForegroundOutput, themeConfig.BackgroundOutput);

            // Print report.
            foreach (var reportInfo in AInstruction.ReportInfo)
            {
                AppendRTBLine(TestRichTextbox, reportInfo, themeConfig.ForegroundOutputWarning, themeConfig.BackgroundOutputWarning);
                UpdateMaxWidth(reportInfo);
                // If have reportInfo, then show outputBox.
                showOutput = true;
            }

            // Print return information.
            SolidColorBrush bgcolor;
            SolidColorBrush fgcolor;
            switch (AInstruction.reportType)
            {
                case AInstruction.ReportType.OK: bgcolor = themeConfig.BackgroundOutputOk; fgcolor = themeConfig.ForegroundOutputOk; if (AInstruction.ReportInfo.Count == 0) showOutput = false; break;
                case AInstruction.ReportType.WARNING: bgcolor = themeConfig.BackgroundOutputWarning; fgcolor = themeConfig.ForegroundOutputWarning; showOutput = true; break;
                case AInstruction.ReportType.ERROR: bgcolor = themeConfig.BackgroundOutputError; fgcolor = themeConfig.ForegroundOutputError; showOutput = true; break;
                default: bgcolor = themeConfig.BackgroundOutputWarning; fgcolor = themeConfig.ForegroundOutputWarning; break;
            }
            if (retnInfo != "") AppendRTBLine(TestRichTextbox, retnInfo.Trim(), fgcolor, bgcolor);

            if (AInstruction.GetType(InstructionTextBox.Text) == InstructionType.CMD) { InstructionTextBox.Text = "> "; showOutput = true; }
            else
            {
                InstructionTextBox.Text = "";
                if (AInstruction.ReportType.OK == AInstruction.reportType && 0 == AInstruction.ReportInfo.Count) Visibility = Visibility.Hidden;
            }
            InstructionTextBox.SelectionStart = InstructionTextBox.Text.Length;

            // Update width and resize.
            UpdateMaxWidth(retnInfo);
            Resize();
        }

        /// <summary>
        /// 更新最大行宽记录
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
            //if (!showOutput) TestRichTextbox.Width = 0;
            Width = (showOutput ? (insWidth > outputWidthMax ? insWidth : outputWidthMax) : insWidth) + widthBias;
            //if (showOutput) TestRichTextbox.Width = Width;
            TestRichTextbox.Visibility = showOutput ? Visibility.Visible : Visibility.Hidden;
            if (resizeHeight)
            {
                Height = (showOutput ? TestRichTextbox.ExtentHeight : 0) + InstructionTextBox.Height + heightBias;
                //Console.WriteLine(Height + " = " + TestRichTextbox.ExtentHeight + " + " + InstructionTextBox.Height + " + " + heightBias);
            }
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
            if (scrollToEnd) { TestRichTextbox.ScrollToEnd(); }
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
                if (!constInstructionInputMode)
                {
                    new Thread(() => { InstructionTextBox.Dispatcher.BeginInvoke((Action)(() => ExecuteTextBoxInstrution())); }).Start();
                }
                else { e.Handled = false; Resize(true, 12, 500); InstructionTextBox.Height = 500; InstructionTextBox.MaxLines = 500; }
                return;
            }
            if (e.Key == Key.Enter && "" == InstructionTextBox.Text) {
                Visibility = Visibility.Hidden; showOutput = false; Resize(); }
            if (e.Key == Key.Escape) { Visibility = Visibility.Hidden; showOutput = false; Resize(); }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Alt) { e.Handled = true; showOutput = !showOutput; Resize(); return; }
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                if (InstructionTextBox.Text[InstructionTextBox.Text.Length - 1] != ' ')
                { completing = true; InstructionTextBox.AppendText(" "); completing = false; }
                InstructionTextBox.CaretIndex = InstructionTextBox.Text.Length;
            }
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
            //TimeSpan startTime = new TimeSpan(System.DateTime.Now.Ticks);

            Resize(constInstructionInputMode);
            
            //TimeSpan endTime = new TimeSpan(System.DateTime.Now.Ticks);
            //TimeSpan countTime = startTime.Subtract(endTime).Duration();
            //Console.WriteLine(countTime.TotalMilliseconds.ToString());

            // Completion
            foreach (TextChange tc in e.Changes)
                if (tc.AddedLength > 0)
                { CompleteInstruction(); break; }

        }

        /// <summary>
        /// 指令补全
        /// </summary>
        private void CompleteInstruction()
        {
            if (!completing)
            {
                InputAttribution ia = ACompletion.GetInstructionCompletion(new InputAttribution { content = InstructionTextBox.Text, caretPosition = InstructionTextBox.CaretIndex, selectEmpty = true });
                if (!ia.selectEmpty)
                {
                    completing = true;
                    InstructionTextBox.Text = ia.content;
                    InstructionTextBox.CaretIndex = ia.caretPosition;
                    InstructionTextBox.SelectionStart = ia.selectStart;
                    InstructionTextBox.SelectionLength = ia.selectEnd - ia.selectStart + 1;
                }
            }
            completing = false;
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
                themeConfig.BackgroundInput,
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
