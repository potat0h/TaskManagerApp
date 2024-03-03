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
using Windows.UI;


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
                ClockIdentifier = "24HourClock" // Use 24-hour format
            };

            // Create a text block for displaying error messages
            TextBlock errorTextBlock = new TextBlock
            {
                Text = string.Empty, // Initially empty
                Foreground = new SolidColorBrush(Colors.Red), // Set error message color to red
                TextWrapping = TextWrapping.Wrap // Enable text wrapping
            };

            // Add controls to the content dialog
            StackPanel panel = new StackPanel();
            panel.Children.Add(taskNameTextBox);
            panel.Children.Add(deadlineDatePicker);
            panel.Children.Add(deadlineTimePicker);
            panel.Children.Add(errorTextBlock); // Add error text block

            addTaskDialog.Content = panel;

            // Handle dialog buttons
            addTaskDialog.PrimaryButtonClick += async (s, args) =>
            {
                // Validate task name
                if (string.IsNullOrWhiteSpace(taskNameTextBox.Text))
                {
                    // Set error message below the text box
                    errorTextBlock.Text = "Please enter a task name.";
                    args.Cancel = true; // Prevent dialog from closing
                    return;
                }

                // Combine date and time into a single DateTime
                DateTime deadline = deadlineDatePicker.Date.Date;
                if (deadlineTimePicker.SelectedTime.HasValue)
                {
                    deadline = deadline.Date + deadlineTimePicker.SelectedTime.Value;
                }


                // Add the task to the database
                DataAccess.AddData(taskNameTextBox.Text, deadline);

                // Refresh task list
                LoadData();
            };

            // Show the dialog
            await addTaskDialog.ShowAsync();
        }




        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            // Get the task to edit from the sender (assuming sender is a Button)
            Button editButton = sender as Button;
            Task taskToEdit = editButton.DataContext as Task;

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

            // Create controls for editing task name, date, and time
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

            TimePicker deadlineTimePicker = new TimePicker
            {
                Header = "Select time", // Set header text as a hint for the user
                ClockIdentifier = "24HourClock" // Use 24-hour format
            };

            // Create a text block for displaying error messages
            TextBlock errorTextBlock = new TextBlock
            {
                Text = string.Empty, // Initially empty
                Foreground = new SolidColorBrush(Colors.Red), // Set error message color to red
                TextWrapping = TextWrapping.Wrap // Enable text wrapping
            };

            // Add controls to the content dialog
            StackPanel panel = new StackPanel();
            panel.Children.Add(taskNameTextBox);
            panel.Children.Add(deadlineDatePicker);
            panel.Children.Add(deadlineTimePicker);
            panel.Children.Add(errorTextBlock); // Add error text block
            editTaskDialog.Content = panel;

            // Handle dialog buttons
            editTaskDialog.PrimaryButtonClick += async (s, args) =>
            {
                // Validate task name
                if (string.IsNullOrWhiteSpace(taskNameTextBox.Text))
                {
                    // Set error message below the text box
                    errorTextBlock.Text = "Please enter a task name.";
                    args.Cancel = true; // Prevent dialog from closing
                    return;
                }

                // Combine date and time into a single DateTime
                DateTime deadline = deadlineDatePicker.Date.DateTime;
                if (deadlineTimePicker.SelectedTime.HasValue)
                {
                    deadline = deadline.Date + deadlineTimePicker.SelectedTime.Value;
                }

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

        private void MarkAsComplete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            // Update the task's completion status (e.g., by appending a marker to indicate completion)
            Task task = button.DataContext as Task;

            if (task.IsComplete == 0)
            {
                task.IsComplete = 1;
                CompletedTasks.Add(task);
                Tasks.Remove(task);
                DataAccess.MarkAsComplete(task.TaskName);
            }
        }












    }
}

