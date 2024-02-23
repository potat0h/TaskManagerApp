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
using System.Data.SQLite;
using Windows.Storage;


namespace TaskManagerApp
{
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Task> tasks;
        private string connectionString = $"Data Source={Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db")};Version=3;";


        public MainPage()
        {
            this.InitializeComponent();
            tasks = new ObservableCollection<Task>();
            TaskListView.ItemsSource = tasks;
            InitializeDatabase();
            LoadTasks();
        }

        private void InitializeDatabase()
        {
            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            SQLiteConnection.CreateFile(dbPath); //to do - skuziti zas nece napraviti novi db file, nego samo ovako locally nesto (ne kuzim)
            
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TaskName NVARCHAR(2048) NOT NULL,
                    IsComplete BOOLEAN NOT NULL
                )";
                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = createTableQuery;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }


        private void LoadTasks()
        {
            tasks.Clear();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT Id, TaskName FROM Tasks";
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new Task { Id = Convert.ToInt32(reader["Id"]), TaskName = reader["Task"].ToString() });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TaskWindow));
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem != null)
            {
                Task task = TaskListView.SelectedItem as Task;
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string deleteQuery = $"DELETE FROM Tasks WHERE Id = {task.Id}";
                        using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    tasks.Remove(task);
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }
            }
        }

        private void MarkAsComplete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Task task = button.DataContext as Task;
            task.IsComplete = true;
            // Update database or perform any other necessary action
        }
    }
}

