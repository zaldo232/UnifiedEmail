using System.Globalization;
using System.Windows.Data;

namespace UnifiedEmail.Helpers
{
    // bool 값을 반전하는 컨버터
    public class InverseBoolConverter : IValueConverter
    {
        // bool이면 NOT 반환, 아니면 true 반환
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(value is bool b) || !b;

        // 역변환 미지원, 예외 발생
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
