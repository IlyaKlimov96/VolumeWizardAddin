using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VolumeWizardAddin
{
    /// <summary>
    /// Логика взаимодействия для SizeAndPositionPanel.xaml
    /// </summary>
    public partial class SizeAndPositionPanel : UserControl
    {
        public SizeAndPositionPanel()
        {
            InitializeComponent();
        }

        private void Rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SolidColorBrush solidColorBrush = null;
            BindingExpression binding = null;
            if (sender is Rectangle)
            {
                Rectangle rect = (Rectangle)sender;
                solidColorBrush = rect.Stroke as SolidColorBrush;
                binding = rect.GetBindingExpression(Rectangle.StrokeProperty);
     
            }
            else if (sender is TextBlock)
            {
                TextBlock textBlock = (TextBlock)sender;
                solidColorBrush = textBlock.Foreground as SolidColorBrush;
                binding = textBlock.GetBindingExpression(TextBlock.ForegroundProperty);
            }

            if (solidColorBrush != null)
            {
                if (solidColorBrush.Color == Colors.Black) solidColorBrush.Color = Colors.Red;
                binding?.UpdateSource();

            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                e.Handled = true;
            }
        }
    }

    public class SelectedSideConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Brush))
            {
                if ((Side)value == (Side)parameter) return new SolidColorBrush(Colors.Red); else return new SolidColorBrush(Colors.Black);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Side))
            {
                if (((SolidColorBrush)value).Color == Colors.Red) return (Side)parameter;
            }
            return null;
        }
    }
}
