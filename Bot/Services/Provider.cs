using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Modules
{
    class Provider
    {
        public void ExecuteSQL(string cmd)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = cmd;
                command.ExecuteNonQuery();
                Console.WriteLine("Success!");
            }
        }

        public bool UserAlreadyCreated(string id)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                string sqlExpression = "SELECT discord_id FROM users";
                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId = Convert.ToString(reader.GetValue(0));
                            if (getId.Equals(id))
                                return true;

                        }
                }
            }
            return false;
        }

        public bool UserAlreadyInBattle(string id, bool factor)
        {
            string field; // "player1id"
            if (factor)
                field = "player1id";
            else
                field = "player2id";



            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                string sqlExpression = $"SELECT {field} FROM duel";
                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId = Convert.ToString(reader.GetValue(0));
                            if (getId.Equals(id))
                                return true;

                        }
                }
                sqlExpression = "SELECT player2id FROM duel";
                command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId = Convert.ToString(reader.GetValue(0));
                            if (getId.Equals(id))
                                return true;

                        }
                }
            }
            return false;
        }

        public object GetFieldAwonaByID(string field, string id, string idfield, string table)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                string sqlExpression = $"SELECT * FROM {table}";
                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId = Convert.ToString(reader[$"{idfield}"]);

                            if (getId.Equals(id))
                                return reader[$"{field}"];
                        }
                }
            }
            return null;
        }

        public object GetDuelIDAwona(string id, bool factor)
        {
            string field;
            if (factor)
                field = "player1id";
            else
                field = "player2id";

            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                string sqlExpression = $"SELECT * FROM duel";
                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId = Convert.ToString(reader[field]);
                            if (getId.Equals(id))
                                return reader[field];
                        }
                }
            }
            return null;
        }

        public object GetDuelChannelIDAwona(string id, bool factor)
        {
            string field;
            if (factor)
                field = "channel1id";
            else
                field = "channel2id";

            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                string sqlExpression = $"SELECT * FROM duel";
                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId1 = Convert.ToString(reader["player1id"]);
                            string getId2 = Convert.ToString(reader["player2id"]);
                            if (getId1.Equals(id) || getId2.Equals(id))
                                return reader[field];
                        }
                }
            }
            return null;
        }

        public int GetDuelHealthAwona(ulong id, bool factor)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                string sqlExpression = $"SELECT * FROM duel";
                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {

                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId;
                            if (factor)
                                getId = Convert.ToString(reader["player1id"]);
                            else
                                getId = Convert.ToString(reader["player2id"]);

                            if (getId.Equals(Convert.ToString(id)) && factor)
                                return Convert.ToInt32(reader["player1health"]);
                            else if (getId.Equals(Convert.ToString(id)) && !factor)
                                return Convert.ToInt32(reader["player2health"]);
                        }
                }
            }
            return 0;
        }
    }
}
