using System;
using System.Globalization;
using System.Windows.Data;
using MatrixMultiplicationApp.Models;

namespace MatrixMultiplicationApp
{
    /// <summary>
    /// Конвертер для відображення назв методів множення матриць
    /// </summary>
    public class MethodNameConverter : IValueConverter
    {
        public static readonly MethodNameConverter Instance = new MethodNameConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MultiplicationMethod method)
            {
                switch (method)
                {
                    case MultiplicationMethod.Traditional:
                        return "Традиційний метод";
                    case MultiplicationMethod.Strassen:
                        return "Метод Штрассена";
                    case MultiplicationMethod.WinogradStrassen:
                        return "Метод Винограда-Штрассена";
                    default:
                        return method.ToString();
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}