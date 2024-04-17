using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace NCMDumpGUI_WinUI.Converter
{
    public class BackdropConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MicaBackdrop micaBackdrop)
            {
                // Customize the conversion as needed
                return "Mica";
            }
            if (value is DesktopAcrylicBackdrop desktopAcrylicBackdrop)
            {
                return "Acrylic";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is "Mica")
            {
                return new MicaBackdrop();
            }
            if (value is "Acrylic")
            {
                return new DesktopAcrylicBackdrop();
            }
            return null;
        }
    }
}