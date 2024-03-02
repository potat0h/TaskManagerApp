using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace TaskManagerApp
{
    public static class DataAccess
    {
        public async static void InitializeDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("TaskManager.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                String tableCommand = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TaskName NVARCHAR(2048) NOT NULL,
                    Deadline DATETIME,
                    IsComplete INTEGER NOT NULL DEFAULT 0
                )";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }

        public static void AddData(string taskName, DateTime? deadline, bool isComplete = false)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Format deadline to ISO 8601 format
                string formattedDeadline = deadline?.ToString("yyyy-MM-dd HH:mm:ss");

                insertCommand.CommandText = "INSERT INTO Tasks (TaskName, Deadline, IsComplete) VALUES (@TaskName, @Deadline, @IsComplete);";
                insertCommand.Parameters.AddWithValue("@TaskName", taskName);
                // Check if formattedDeadline is null before assigning a value
                insertCommand.Parameters.AddWithValue("@Deadline", formattedDeadline != null ? (object)formattedDeadline : DBNull.Value);
                insertCommand.Parameters.AddWithValue("@IsComplete", isComplete ? 1 : 0);

                insertCommand.ExecuteNonQuery();
            }
        }



        public static List<Task> GetData(bool completedTasks = false)
        {
            List<Task> tasks = new List<Task>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                string sqlCommand;
                if (completedTasks)
                {
                    // Query for completed tasks including the Deadline column
                    sqlCommand = "SELECT Id, TaskName, Deadline, IsComplete FROM Tasks WHERE IsComplete = 1";
                }
                else
                {
                    // Query for incomplete tasks
                    sqlCommand = "SELECT Id, TaskName, Deadline, IsComplete FROM Tasks WHERE IsComplete = 0";
                }

                SqliteCommand selectCommand = new SqliteCommand(sqlCommand, db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    Task task = new Task
                    {
                        Id = query.GetInt32(0),
                        TaskName = query.GetString(1),
                        Deadline = query.IsDBNull(2) ? DateTime.MinValue : query.GetDateTime(2), // Handle null values
                        IsComplete = query.GetInt32(3)
                    };
                    tasks.Add(task);
                }
            }

            return tasks;
        }


        public static void DeleteData(string taskName)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = db;

                deleteCommand.CommandText = "DELETE FROM Tasks WHERE TaskName = @TaskName;";
                deleteCommand.Parameters.AddWithValue("@TaskName", taskName);

                deleteCommand.ExecuteNonQuery();
            }
        }

        public static void UpdateTask(int taskId, string taskName, DateTime? deadline)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand updateCommand = new SqliteCommand();
                updateCommand.Connection = db;

                updateCommand.CommandText = "UPDATE Tasks SET TaskName = @TaskName, Deadline = @Deadline WHERE Id = @TaskId;";
                updateCommand.Parameters.AddWithValue("@TaskName", taskName);
                updateCommand.Parameters.AddWithValue("@Deadline", deadline.HasValue ? deadline.Value.Date : (object)DBNull.Value);
                updateCommand.Parameters.AddWithValue("@TaskId", taskId);

                updateCommand.ExecuteNonQuery();
            }
        }


        public static void MarkAsComplete(string taskName)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand updateCommand = new SqliteCommand();
                updateCommand.Connection = db;

                updateCommand.CommandText = "UPDATE Tasks SET IsComplete = 1 WHERE TaskName = @TaskName;";
                updateCommand.Parameters.AddWithValue("@TaskName", taskName);

                updateCommand.ExecuteNonQuery();
            }
        }

    }
}
