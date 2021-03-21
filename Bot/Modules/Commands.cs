using Discord.Commands;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Commands> _logger;

        public Commands(ILogger<Commands> logger)
            => _logger = logger;

        static void ExecuteSQL(string cmd)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = cmd;
                command.ExecuteNonQuery();
            }
        }

        static bool alreadyCreated(string id)
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

        [Command("create")]
        public async Task createCharacter(string archetype)
        {
            //_logger.LogInformation($"{Context.User.Id} executed the create command");
            //Context.User.Id
            if (!alreadyCreated(Convert.ToString(Context.User.Id)))
            {
                int level;
                string type, discord_id;
                archetype = archetype.ToLower();

                if (!(archetype.Equals("faith") || archetype.Equals("ranged") || archetype.Equals("melee") || archetype.Equals("magic")))
                    await ReplyAsync(":x: Выберите один из четырёх археклассов (Faith, Ranged, Melee, Magic)");
                else
                {
                    switch (archetype)
                    {
                        case ("faith"):
                            type = "Acolyte";
                            break;
                        case ("ranged"):
                            type = "Thrower";
                            break;
                        case ("magic"):
                            type = "Alchemist";
                            break;
                        case ("melee"):
                            type = "Komtur";
                            break;
                        default:
                            type = "Komtur";
                            break;
                    }

                    discord_id = Convert.ToString(Context.User.Id);
                    level = 1;

                    ExecuteSQL($"INSERT INTO users (discord_id, level, archetype, type) VALUES ('{discord_id}', '{level}', '{archetype}', '{type}')");
                    await ReplyAsync(":white_check_mark: Персонаж успешно создан");
                }
            }
            else
                await ReplyAsync(":x: У вас уже есть персонаж. Если хотите создать нового - используйте `!delete`");
            
        }
    
        [Command("delete")] 
        public async Task deleteCharacter()
        {
            string id, command;
            id = Convert.ToString(Context.User.Id);
            command = $"DELETE FROM users WHERE discord_id = {id}";
            if (alreadyCreated(id))
            { 
                ExecuteSQL(command);
                await ReplyAsync(":white_check_mark: Персонаж успешно удалён");
            }
            else
                await ReplyAsync(":x: У вас ещё нет персонажа.");


        }
    }
}
