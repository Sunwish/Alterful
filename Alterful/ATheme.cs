using System;
using System.Collections.Generic;
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
        public static AlterfulTheme Theme
        {
            get => theme;
            set
            {
                if (value == theme) return;
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
    }
}
