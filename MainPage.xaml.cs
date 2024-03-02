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
        private bool deleteMode = false;

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


        private async void OpenAddTaskDialog(object sender, RoutedEventArgs e)
        {
            // Create a content dialog for adding a new task
            ContentDialog addTaskDialog = new ContentDialog
            {
                Title = "Add New Task",
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel"
            };

            // Create controls for task name, date, and time
            TextBox taskNameTextBox = new TextBox
            {
                PlaceholderText = "Enter task name"
            };

            DatePicker deadlineDatePicker = new DatePicker
            {
                Date = DateTimeOffset.Now.Date, // Set initial date to today
                Header = "Select deadline" // Set header text as a hint for the user
            };

            TimePicker deadlineTimePicker = new TimePicker
            {
                Header = "Select time", // Set header text as a hint for the user
                ClockIdentifier = "24HourClock"
            };

            // Add controls to the content dialog
            StackPanel panel = new StackPanel();
            panel.Children.Add(taskNameTextBox);
            panel.Children.Add(deadlineDatePicker);
            panel.Children.Add(deadlineTimePicker);
            addTaskDialog.Content = panel;

            // Handle dialog buttons
            addTaskDialog.PrimaryButtonClick += async (s, args) =>
            {
                // Validate task name
                if (string.IsNullOrWhiteSpace(taskNameTextBox.Text))
                {
                    // Show error message if task name is empty
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Please enter a task name.",
                        CloseButtonText = "OK"
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                // Combine date and time into a single DateTime
                DateTime deadline = deadlineDatePicker.Date.Date + deadlineTimePicker.Time;

                // Add the task to the database
                DataAccess.AddData(taskNameTextBox.Text, deadline);

                // Refresh task list
                LoadData();
            };

            
            // Show the dialog
            await addTaskDialog.ShowAsync();
        }


        private async void OpenEditTaskDialog(object sender, RoutedEventArgs e)
        {
            // Get the task to edit from the sender (assuming sender is a Button)
            Button editButton = sender as Button;
            Task taskToEdit = editButton.DataContext as Task;

            // Check if a task is selected
            if (taskToEdit == null)
            {
                // Show error message if no task is selected
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please select a task to edit.",
                    CloseButtonText = "OK"
                };
                await errorDialog.ShowAsync();
                return;
            }

            // Create a content dialog for editing the selected task
            ContentDialog editTaskDialog = new ContentDialog
            {
                Title = "Edit Task",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel"
            };

            // Create controls for editing task name and deadline
            TextBox taskNameTextBox = new TextBox
            {
                Text = taskToEdit.TaskName,
                PlaceholderText = "Enter task name"
            };

            DatePicker deadlineDatePicker = new DatePicker
            {
                Date = taskToEdit.Deadline != DateTime.MinValue ? taskToEdit.Deadline : DateTimeOffset.Now.Date,
                Header = "Select deadline" // Set header text as a hint for the user
            };

            // Add controls to the content dialog
            StackPanel panel = new StackPanel();
            panel.Children.Add(taskNameTextBox);
            panel.Children.Add(deadlineDatePicker);
            editTaskDialog.Content = panel;

            // Handle dialog buttons
            editTaskDialog.PrimaryButtonClick += async (s, args) =>
            {
                // Validate task name
                if (string.IsNullOrWhiteSpace(taskNameTextBox.Text))
                {
                    // Show error message if task name is empty
                    var errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Please enter a task name.",
                        CloseButtonText = "OK"
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                // Convert DateTimeOffset to DateTime
                DateTime deadline = deadlineDatePicker.Date.DateTime;

                // Update the selected task in the database
                DataAccess.UpdateTask(taskToEdit.Id, taskNameTextBox.Text, deadline);

                // Refresh task list
                LoadData();
            };

            // Show the dialog
            await editTaskDialog.ShowAsync();
        }

        private async void ViewDeadline_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            DateTime? deadline = button.Tag as DateTime?;
            if (deadline.HasValue)
            {
                ContentDialog deadlineDialog = new ContentDialog
                {
                    Title = "Deadline",
                    Content = $"The deadline for this task is: {deadline.Value.ToString("MM/dd/yyyy HH:mm:ss")}",
                    CloseButtonText = "Close"
                };



                await deadlineDialog.ShowAsync();
            }
            else
            {
                // Handle the case where deadline is not set
                // You can show a message or take other appropriate action
            }
        }

        private void ToggleDeleteMode(object sender, RoutedEventArgs e)
        {
            deleteMode = !deleteMode; // Toggle deletion mode

            if (deleteMode)
            {
                // Show checkboxes and delete button
                DeleteButton.Visibility = Visibility.Visible;
            }
            else
            {
                // Hide checkboxes and delete button
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteSelectedTasks(object sender, RoutedEventArgs e)
        {
            // Remove completed tasks from the Tasks collection
            for (int i = Tasks.Count - 1; i >= 0; i--)
            {
                if (Tasks[i].IsComplete == 1)
                {
                    DataAccess.DeleteData(Tasks[i].TaskName); // Delete task from the database
                    Tasks.RemoveAt(i); // Remove task from the Tasks collection
                }
            }

            // Remove completed tasks from the CompletedTasks collection
            for (int i = CompletedTasks.Count - 1; i >= 0; i--)
            {
                if (CompletedTasks[i].IsComplete == 1)
                {
                    DataAccess.DeleteData(CompletedTasks[i].TaskName); // Delete task from the database
                    CompletedTasks.RemoveAt(i); // Remove task from the CompletedTasks collection
                }
            }
        }



    }
}

