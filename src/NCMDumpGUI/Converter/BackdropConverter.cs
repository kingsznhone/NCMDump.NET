
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace NCMDumpGUI.Converter
{
    public class BackdropConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowBackdropType.Auto)
            {
                return "Auto";
            }
            if (value is WindowBackdropType.Mica)
            {
                return "Mica";
            }
            if (value is WindowBackdropType.Acrylic)
            {
                return "Acrylic";
            }
            if (value is WindowBackdropType.None)
            {
                return "None";
            }
            if (value is WindowBackdropType.Tabbed)
            {
                return "Tabbed";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is "Auto")
            {
                return WindowBackdropType.Auto;
            }
            if (value is "Mica")
            {
                return WindowBackdropType.Mica;
            }
            if (value is "Acrylic")
            {
                return WindowBackdropType.Acrylic;
            }
            if (value is "None")
            {
                return WindowBackdropType.None;
            }
            if (value is "Tabbed")
            {
                return WindowBackdropType.Tabbed;
            }
            return null;
        }
    }
}
