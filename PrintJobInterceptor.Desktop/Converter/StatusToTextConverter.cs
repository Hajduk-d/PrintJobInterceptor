using System.Globalization;
using System.Windows.Data;
using ReactiveUI;

namespace PrintJobInterceptor.Desktop.Converter;

public class StatusToTextConverter : IValueConverter, IBindingTypeConverter
{
    // I am aware that using a Converter for this is overkill/not the best way to solve this. 
    // the other solution would be to just use enums as they provide better type safety
    private string Convert(uint state) => state switch
    {
        1 => "Other",
        2 => "Unknown",
        3 => "Idle",
        4 => "Printing",
        5 => "Warmup",
        6 => "Stopped Printing",
        7 => "Offline",
        _ => "Unknown"
    };

    // For WPF XAML bindings
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is uint state)
        {
            return Convert(state);
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    // For ReactiveUI bindings
    public int GetAffinityForObjects(Type fromType, Type toType)
    {
        if (fromType == typeof(uint) && toType == typeof(string))
            return 100;
        return 0;
    }

    public bool TryConvert(object? from, Type toType, object? conversionHint, out object? result)
    {
        if (from is uint state)
        {
            result = Convert(state);
            return true;
        }
        result = "Unknown";
        return true;
    }

}