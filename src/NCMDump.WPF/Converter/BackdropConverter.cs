using System;
using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace NCMDump.WPF.Converter
{
    public class BackdropConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                WindowBackdropType.Auto => "Auto",
                WindowBackdropType.Mica => "Mica",
                WindowBackdropType.Acrylic => "Acrylic",
                WindowBackdropType.None => "None",
                WindowBackdropType.Tabbed => "Tabbed",
                _ => string.Empty
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                "Auto" => WindowBackdropType.Auto,
                "Mica" => WindowBackdropType.Mica,
                "Acrylic" => WindowBackdropType.Acrylic,
                "None" => WindowBackdropType.None,
                "Tabbed" => WindowBackdropType.Tabbed,
                _ => WindowBackdropType.Auto
            };
        }
    }
}
