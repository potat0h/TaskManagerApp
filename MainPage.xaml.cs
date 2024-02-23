using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;


namespace TaskManagerApp
{
    public sealed partial class MainPage : Page
    {

        public ObservableCollection<string> Tasks { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            Tasks = new ObservableCollection<string>();
            Output.ItemsSource = Tasks;
            DataAccess.InitializeDatabase();
            LoadData();
        }

        private void LoadData()
        {
            Tasks.Clear();
            var data = DataAccess.GetData();
            foreach (var task in data)
            {
                Tasks.Add(task);
            }
        }

        private void AddData(object sender, RoutedEventArgs e)
        {
            DataAccess.AddData(Input_Box.Text);
            LoadData();
        }
    }
}

