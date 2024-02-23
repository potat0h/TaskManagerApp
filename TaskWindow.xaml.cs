using System.Data.SQLite;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238


namespace TaskManagerApp
{
    public sealed partial class TaskWindow : Page
    {
        private string connectionString = "Data Source=TaskManager.db;Version=3;";

        public TaskWindow()
        {
            this.InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TaskTextBox.Text))
            {
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO Tasks (TaskName) VALUES (@TaskName)";
                        using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@TaskName", TaskTextBox.Text);
                            command.ExecuteNonQuery();
                        }

                    }
                    Frame.GoBack(); // Navigate back to the previous page (MainPage)
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack(); // Just go back to the previous page without adding the task
        }
    }
}

