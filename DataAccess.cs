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
                    IsComplete INTEGER NOT NULL DEFAULT 0
                )";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }

        public static void AddData(string inputText)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                insertCommand.CommandText = "INSERT INTO Tasks (TaskName, IsComplete) VALUES (@TaskName, @IsComplete);";
                insertCommand.Parameters.AddWithValue("@TaskName", inputText);
                insertCommand.Parameters.AddWithValue("@IsComplete", 0);


                insertCommand.ExecuteNonQuery();

            }

        }

        public static List<Task> GetData()
        {
            List<Task> tasks = new List<Task>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TaskManager.db");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand("SELECT * FROM Tasks", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    tasks.Add(new Task
                    {
                        Id = query.GetInt32(0),
                        TaskName = query.GetString(1),
                        IsComplete = query.GetInt32(2)
                    });
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
