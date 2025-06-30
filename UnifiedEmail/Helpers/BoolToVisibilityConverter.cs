using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UnifiedEmail.Helpers
{
    // bool -> Visibility 변환 컨버터
    public class BoolToVisibilityConverter : IValueConverter
    {
        // true면 Visible, false면 Collapsed 반환
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;

        // 역변환 미지원, 예외 발생
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
