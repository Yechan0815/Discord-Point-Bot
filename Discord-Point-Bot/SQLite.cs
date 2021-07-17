using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Discord_Point_Bot
{
    public class SQLite
    {
        private static SQLite instance = null;

        private SQLiteConnection connection;

        public static SQLite Instance()
        {
            if (instance == null)
                instance = new SQLite();
            return instance;
        }

        public static void free()
        {
            if (instance != null)
                instance.close();
        }

        SQLite()
        {
            try
            {
                connection = new SQLiteConnection("Data Source=local.db");
                connection.Open();
                Console.WriteLine("SQLite is connected");
                TabaleInit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void close()
        {
            connection.Close();
            Console.WriteLine("SQLite is disconnected");
        }

        private void TabaleInit()
        {
            SQLiteCommand command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS User (user TEXT, donate TEXT, attendance TEXT, point INTEGER)"
                , connection
                );
            command.ExecuteNonQuery();
            command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS Betting (user TEXT, donate TEXT, attendance TEXT, point INTEGER)"
                , connection
                );
            command.ExecuteNonQuery();

        }

        public void TableInsert(string id, string attendance, int point)
        {
            Console.WriteLine($"new row in User {id}");
            SQLiteCommand command = new SQLiteCommand(
                $"INSERT INTO User (user, donate, attendance, point) values ('{id}', '', '{attendance}', {point})",
                connection
                );
            command.ExecuteNonQuery();
        }

        public void UserUpdate(string id, string donate = null, string attendance = null, int point = -1)
        {
            string target = "";
            if (donate != null)
                target = $"donate='{donate}'";
            else if (attendance != null)
                target = $"attendance='{attendance}'";
            else if (point != -1)
                target = $"point={point}";
            SQLiteCommand command = new SQLiteCommand(
                $"UPDATE User SET {target} WHERE user={id}",
                connection
                );
            command.ExecuteNonQuery();
        }

        public User GetUser(string id)
        {
            Console.WriteLine($"get User {id}");
            User result = new User();
            SQLiteCommand cmd = new SQLiteCommand(
                $"SELECT * FROM User WHERE user='{id}'",
                connection
                );
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                TableInsert(id, "null", 0);
                result.Donate = "";
                result.Attendance = "null";
                result.Point = 0;
                return result;
            }
            while (reader.Read())
            {
                result.Donate = reader["donate"].ToString();
                result.Attendance = reader["attendance"].ToString();
                result.Point = int.Parse(reader["point"].ToString());
            }
            reader.Close();
            return result;
        }
    }
}
