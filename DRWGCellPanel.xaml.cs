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

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;
                textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }

        private void ViewDirection_LostFocus(object sender, RoutedEventArgs e)
        {
            /*ComboBox comboBox = (ComboBox)sender;
            string[] itemsSource = comboBox.ItemsSource as string[];
            if (!itemsSource.Contains(comboBox.Text))
            {
                if (itemsSource.Length == 6)
                {
                    Array.Resize(ref itemsSource, 7);
                    itemsSource[6] = comboBox.Text;
                }
                else
                {
                    itemsSource[6] = comboBox.Text;
                }
                comboBox.ItemsSource = itemsSource;
                comboBox.SelectedValue = comboBox.Text;
  
            }      
            else
            {
                if (comboBox.SelectedIndex != 6)
                {
                    Array.Resize(ref itemsSource, 6);
                    comboBox.ItemsSource = itemsSource;
                }
            }    
            comboBox.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateSource();*/
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
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
    }
}
