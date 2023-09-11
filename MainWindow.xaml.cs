using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Aveva.Core.Database;

namespace VolumeWizardAddin
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateListOfDRWG(object sender, ExecutedRoutedEventArgs e)
        {
            ((ListOfDRWG)this.DataContext).Update();
        }

        private void SelectElement_Command(object sender, ExecutedRoutedEventArgs e)
        {
            if (DbElement.Parse(((TextBlock)sender).Text, out DbElement element, out _)) CurrentElement.Element = element;
        }

        private void CreateElement_Command(object sender, ExecutedRoutedEventArgs e)
        {
            DRWG dRWG = ((DRWG)this.DRWGListView.SelectedItem);
            switch ((string)e.Parameter)
            {
                case "DRWG":
                    {
                        ((ListOfDRWG)this.DataContext).Create();
                        break;
                    }
                case "MainVolume":
                    {
                        dRWG.CreateMain();
                        break;
                    }
                case "Section":
                    {
                        dRWG.CreateSection();
                        break;
                    }
                case "View":
                    {
                        dRWG.CreateView();
                        break;
                    }
            }
        }

        private void DRWGPanel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DRWGListView.SelectedIndex != -1) e.CanExecute = true; else e.CanExecute = false;
        }

        private void DRWGListView_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (((ListOfDRWG)this.DataContext).Name != null) e.CanExecute = true; else e.CanExecute = false;
        }
    }

}
