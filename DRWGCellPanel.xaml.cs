using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для DRWGCellPanel.xaml
    /// </summary>
    /// 

    public partial class DRWGCellPanel : UserControl
    {
        public DRWGCellPanel()
        {
            InitializeComponent();       
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

        public void DRWGSelected_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            if (dataGrid.SelectedItems.Count > 0) e.CanExecute = true; else e.CanExecute = false;
        }

        private void DeleteElement(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedUICommand applicationCommands = ApplicationCommands.Delete;
            DataGrid dataGrid = (DataGrid)sender;
            string message = $"Вы действительно хотите удалить эти элементы ({dataGrid.SelectedItems.Count} шт.)?";
            switch (dataGrid.SelectedItem.GetType().Name)
            {
                case "Section":
                    {                       
                        if (MessageBox.Show (message, "Удалить", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            List<Section> element = dataGrid.SelectedItems.Cast<Section>().ToList();
                            foreach (Section item in element) item.Delete();
                        }
                        break;
                    }
                case "View":
                    {
                        if (MessageBox.Show(message, "Удалить", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            List<View> element = dataGrid.SelectedItems.Cast<View>().ToList();
                            foreach (View item in element) item.Delete();
                        }
                        break;
                    }
            }           
        }

        private void ViewDirection_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
