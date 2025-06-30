using System.Globalization;
using System.Windows.Data;

namespace UnifiedEmail.Helpers
{
    // null 여부를 bool로 변환하는 컨버터
    public class NullToBoolConverter : IValueConverter
    {
        // null이면 false, 아니면 true 반환
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null;

        // 역변환 미지원, 예외 발생
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
