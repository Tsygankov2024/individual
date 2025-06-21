using System;
using System.Data.SQLite;
using System.Configuration; // Добавлена эта строка

namespace individual
{
    public static class DatabaseHelper
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.AppSettings["ConnectionString"];
        }

        public static string GetMovieTitle(int movieId)
        {
            string movieTitle = string.Empty;
            using (SQLiteConnection connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT title FROM Movies WHERE id = @movieId";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@movieId", movieId);
                    movieTitle = command.ExecuteScalar()?.ToString() ?? "Неизвестно";
                }
            }
            return movieTitle;
        }

        public static string GetSessionTime(int sessionId)
        {
            string sessionTime = string.Empty;
            using (SQLiteConnection connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT time_film FROM Sessions WHERE id = @sessionId";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    sessionTime = command.ExecuteScalar()?.ToString() ?? "Неизвестно";
                }
            }
            return sessionTime;
        }

        public static string GetUserFullName(int userId)
        {
            string fullName = string.Empty;
            using (SQLiteConnection connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT name FROM Users WHERE id = @userId";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    fullName = command.ExecuteScalar()?.ToString() ?? "Неизвестно";
                }
            }
            return fullName;
        }


    }
}