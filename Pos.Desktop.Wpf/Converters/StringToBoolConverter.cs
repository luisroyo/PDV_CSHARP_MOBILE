using System.Globalization;
using System.Windows.Data;

namespace Pos.Desktop.Wpf.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                bool hasValue = !string.IsNullOrWhiteSpace(str);
                
                // Se o parâmetro for "Collapsed", retorna Collapsed quando tem valor
                if (parameter?.ToString() == "Collapsed")
                    return hasValue ? Visibility.Collapsed : Visibility.Visible;
                
                // Se o parâmetro for "Visible", retorna Visible quando tem valor
                if (parameter?.ToString() == "Visible")
                    return hasValue ? Visibility.Visible : Visibility.Collapsed;
                
                // Padrão: retorna bool
                return hasValue;
            }
            
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
