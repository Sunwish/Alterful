using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Alterful.Functions
{
    public enum AlterfulTheme
    {
        Default, Docks, Fairy, Mild, Bright, Classic
    }

    class AThemeConfig
    {
        protected SolidColorBrush _input_background = Brushes.Black;
        protected SolidColorBrush _input_foreground = Brushes.White;
        protected SolidColorBrush _output_background = Brushes.Black;
        protected SolidColorBrush _output_foreground = Brushes.MintCream;
        protected SolidColorBrush _output_ok_background = Brushes.DarkGreen;
        protected SolidColorBrush _output_ok_foreground = Brushes.MintCream;
        protected SolidColorBrush _output_warning_background = Brushes.Gold;
        protected SolidColorBrush _output_warning_foreground = Brushes.DarkBlue;
        protected SolidColorBrush _output_error_background = Brushes.Red;
        protected SolidColorBrush _output_error_foreground = Brushes.MintCream;
        public SolidColorBrush BackgroundInput { get => _input_background; }
        public SolidColorBrush ForegroundInput { get => _input_foreground; }
        public SolidColorBrush BackgroundOutput { get => _output_background; }
        public SolidColorBrush ForegroundOutput { get => _output_foreground; }
        public SolidColorBrush BackgroundOutputWarning { get => _output_warning_background; }
        public SolidColorBrush ForegroundOutputWarning { get => _output_warning_foreground; }
        public SolidColorBrush BackgroundOutputOk { get => _output_ok_background; }
        public SolidColorBrush ForegroundOutputOk { get => _output_ok_foreground; }
        public SolidColorBrush BackgroundOutputError { get => _output_error_background; }
        public SolidColorBrush ForegroundOutputError { get => _output_error_foreground; }
    }

    class AThemeDefault : AThemeConfig { public AThemeDefault() { } }
    class AThemeMild : AThemeConfig {
        public AThemeMild()
        {
            _input_background = new SolidColorBrush(Color.FromArgb(255, 1, 47, 80));
            _input_foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            _output_background = _input_background;
            _output_foreground = _input_foreground;
            _output_ok_background = new SolidColorBrush(Color.FromArgb(255, 166, 226, 51));
            _output_ok_foreground = _output_background;
            _output_warning_background = new SolidColorBrush(Color.FromArgb(255, 223, 245, 90));
            _output_warning_foreground = new SolidColorBrush(Color.FromArgb(255, 13, 24, 43));
            _output_error_background = new SolidColorBrush(Color.FromArgb(255, 249, 39, 114));
            _output_error_foreground = Brushes.MintCream;
        }
    }
    class AThemeBright : AThemeConfig
    {
        public AThemeBright()
        {
            _input_background = Brushes.White;
            _input_foreground = Brushes.Black;
            _output_background = _input_background;
            _output_foreground = _input_foreground;
            _output_ok_background = new SolidColorBrush(Color.FromArgb(255, 112, 204, 84));
            _output_ok_foreground = Brushes.Black;
            _output_warning_background = Brushes.Gold;
            _output_warning_foreground = Brushes.DarkBlue;
            _output_error_background = Brushes.Red;
            _output_error_foreground = Brushes.MintCream;
        }
    }

    class AThemeFairy : AThemeConfig
    {
        public AThemeFairy()
        {
            _input_background = new SolidColorBrush(Color.FromArgb(255, 239, 231, 212));
            _input_foreground = new SolidColorBrush(Color.FromArgb(255, 211, 78, 1));
            _output_background = _input_background;
            _output_foreground = _input_foreground;
            _output_ok_background = new SolidColorBrush(Color.FromArgb(255, 20, 88, 4));
            _output_ok_foreground = Brushes.MintCream;
            _output_warning_background = new SolidColorBrush(Color.FromArgb(255, 186, 136, 0));
            _output_warning_foreground = new SolidColorBrush(Color.FromArgb(255, 239, 231, 212));
            _output_error_background = new SolidColorBrush(Color.FromArgb(255, 208, 84, 0));
            _output_error_foreground = Brushes.MintCream;
        }
    }

    class AThemeDocks : AThemeConfig
    {
        public AThemeDocks()
        {
            _input_background = new SolidColorBrush(Color.FromArgb(255, 43, 6, 42));
            _input_foreground = new SolidColorBrush(Color.FromArgb(255, 136, 126, 135));
            _output_background = _input_background;
            _output_foreground = _input_foreground;
            _output_ok_background = new SolidColorBrush(Color.FromArgb(255, 20, 88, 4));
            _output_ok_foreground = Brushes.MintCream;
            _output_warning_background = new SolidColorBrush(Color.FromArgb(255, 1, 160, 160));
            _output_warning_foreground = _input_background;
            _output_error_background = new SolidColorBrush(Color.FromArgb(255, 255, 122, 33));
            _output_error_foreground = new SolidColorBrush(Color.FromArgb(255, 13, 21, 43));
        }
    }

    class AThemeClassic : AThemeConfig
    {
        public AThemeClassic()
        {
            _input_background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 170));
            _input_foreground = new SolidColorBrush(Color.FromArgb(255, 85, 255, 255));
            _output_background = _input_background;
            _output_foreground = _input_foreground;
            _output_ok_background = new SolidColorBrush(Color.FromArgb(255, 0, 170, 170));
            _output_ok_foreground = _input_background;
            _output_warning_background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 85));
            _output_warning_foreground = _input_background;
            _output_error_background = Brushes.Red;
            _output_error_foreground = Brushes.MintCream;
        }
    }

    static class ATheme
    {
        private static AlterfulTheme theme = AlterfulTheme.Mild;
        static string THEME_CONFIG_FILE_PATH = AFile.BASE_PATH + @"\Config\ThemeConfig";

        public static AlterfulTheme GetThemeByString(string themeString)
        {
            AlterfulTheme retn = AlterfulTheme.Default;
            foreach (AlterfulTheme theme in Enum.GetValues(typeof(AlterfulTheme)))
            {
                if (theme.ToString().ToLower().Trim() == themeString.ToLower().Trim())
                {
                    retn = theme;
                    break;
                }
            }
            return retn;
        }

        public static AlterfulTheme Theme
        {
            get => theme;
            set
            {
                if (value == theme) return;
                WriteAllConfig(value.ToString());
                theme = value;
            }
        }

        public static AThemeConfig GetThemeConfig()
        {
            switch (theme)
            {
                case AlterfulTheme.Default: return new AThemeDefault();
                case AlterfulTheme.Docks: return new AThemeDocks();
                case AlterfulTheme.Fairy: return new AThemeFairy();
                case AlterfulTheme.Mild: return new AThemeMild();
                case AlterfulTheme.Bright: return new AThemeBright();
                case AlterfulTheme.Classic: return new AThemeClassic();
                default: return new AThemeConfig();
            }
        }

        public static bool IsThemeConfigFileExists()
        {
            return File.Exists(THEME_CONFIG_FILE_PATH);
        }

        /// <summary>
        /// 创建常引用文件，若原本已存在常引用文件则不进行操作
        /// </summary>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static void CreateThemeConfigFile()
        {
            if (!IsThemeConfigFileExists())
                WriteAllConfig(AlterfulTheme.Default.ToString());
        }

        /// <summary>
        /// 写出主题配置
        /// </summary>
        /// <param name="config">主题名</param>
        public static void WriteAllConfig(string config)
        {
            using (StreamWriter streamWriter = new StreamWriter(THEME_CONFIG_FILE_PATH, false)) { streamWriter.WriteLine(config); }
        }

        /// <summary>
        /// 读取主题配置并立即生效
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        public static void ReadAllConfig()
        {
            try
            {
                AlterfulTheme configTheme = AlterfulTheme.Default;
                using (StreamReader streamReader = new StreamReader(THEME_CONFIG_FILE_PATH))
                {
                    configTheme = GetThemeByString(streamReader.ReadToEnd());
                }
                ATheme.Theme = configTheme;
            }
            catch (Exception) { throw; }
        }
    }
}
