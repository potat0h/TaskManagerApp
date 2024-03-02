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
        public ObservableCollection<Task> Tasks { get; set; }
        public ObservableCollection<Task> CompletedTasks { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;
            Tasks = new ObservableCollection<Task>();
            CompletedTasks = new ObservableCollection<Task>();
            Output.ItemsSource = Tasks;
            CompletedOutput.ItemsSource = CompletedTasks;
            DataAccess.InitializeDatabase();
            LoadData();
        }

        private void LoadData()
        {
            Tasks.Clear();
            CompletedTasks.Clear();
            var data = DataAccess.GetData();
            foreach (var task in data)
            {
                if (task.IsComplete == 0)
                    Tasks.Add(task);
                else
                    CompletedTasks.Add(task);
            }
        }

        private void AddData(object sender, RoutedEventArgs e)
        {
            DataAccess.AddData(Input_Box.Text);
            LoadData();
        }

        private void MarkAsComplete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Task task = button.DataContext as Task;

            if (task.IsComplete == 0)
            {
                task.IsComplete = 1;
                CompletedTasks.Add(task);
                Tasks.Remove(task);
                DataAccess.MarkAsComplete(task.TaskName);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Task task = button.DataContext as Task;

            if (task.IsComplete == 0)
                Tasks.Remove(task);
            else
                CompletedTasks.Remove(task);

            DataAccess.DeleteData(task.TaskName);
        }


    }
}

