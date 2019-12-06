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
        Default, Docks, Fairy
    }

    class AThemeConfig
    {
        SolidColorBrush _input_background = Brushes.Black;
        SolidColorBrush _input_foreground = Brushes.White;
        SolidColorBrush _output_background = Brushes.Black;
        SolidColorBrush _output_foreground = Brushes.MintCream;
        SolidColorBrush _output_ok_background = Brushes.DarkGreen;
        SolidColorBrush _output_ok_foreground = Brushes.MintCream;
        SolidColorBrush _output_warning_background = Brushes.Gold;
        SolidColorBrush _output_warning_foreground = Brushes.DarkBlue;
        SolidColorBrush _output_error_background = Brushes.Red;
        SolidColorBrush _output_error_foreground = Brushes.MintCream;
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
    class AThemeDocks : AThemeConfig { public AThemeDocks() => throw new NotImplementedException(); }
    class AThemeFairy : AThemeConfig { public AThemeFairy() => throw new NotImplementedException(); }

    static class ATheme
    {
        private static AlterfulTheme theme = AlterfulTheme.Default;
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
                default: return new AThemeConfig();
            }
        }
    }
}
