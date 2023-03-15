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

        private void CreateMainVol(object sender, ExecutedRoutedEventArgs e)
        {         
           ((DRWG)this.ListOfDRWG_LV.SelectedItem).CreateMain();
        }

        private void ListOfDRWG_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.ListOfDRWG_LV.SelectedIndex != -1) e.CanExecute = true ; else e.CanExecute = false;
        }

        private void CreateDRWG(object sender, ExecutedRoutedEventArgs e)
        {
            ((ListOfDRWG)this.DataContext).Create();
        }

        private void ListOfDrwg_Created(object sender, CanExecuteRoutedEventArgs e)
        {
            if (((ListOfDRWG)this.DataContext).Element != null) e.CanExecute = true; else e.CanExecute = false;
        }

        private void CreateView(object sender, ExecutedRoutedEventArgs e)
        {
            ((DRWG)this.ListOfDRWG_LV.SelectedItem).CreateView();
        }

        private void CreateSection(object sender, ExecutedRoutedEventArgs e)
        {
            ((DRWG)this.ListOfDRWG_LV.SelectedItem).CreateSection();
        }

        private void SelectElement(object sender, ExecutedRoutedEventArgs e)
        {
            if (DbElement.Parse(((TextBlock)sender).Text, out DbElement element, out _)) CurrentElement.Element = element;
        }
    }




}
