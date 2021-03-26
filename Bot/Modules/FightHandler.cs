using Discord;
using Discord.Commands;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Types.Melee;
using Bot.Types.Ranged;
using Bot.Types.Magic;
using Bot.Types;
using Bot.Services;
using Discord.WebSocket;
using Discord.Rest;
using System.Threading;

namespace Bot.Modules 
{
    class FightHandler : ModuleBase<SocketCommandContext>
    {
        public string ConvertType(string type)
        {
            switch (type)
            {
                case ("Alchemist"):
                    return "Алхимик";
                case ("Komtur"):
                    return "Комтур";
                case ("Thrower"):
                    return "Метатель";
                case ("Acolyte"):
                    return "Аколит";
                default:
                    return "null";
            }
        }

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

        static object GetFieldSQL(ulong id, string field, string table)
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
                            string getId = Convert.ToString(reader.GetValue(0));
                            if (getId.Equals(Convert.ToString(id)))
                                return reader[field];
                        }
                }
            }
            return null;
        }

        static int GetHealthSQL(ulong id, bool factor)
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

        public async Task StartMessage(SocketGuildUser user1, SocketGuildUser user2, ITextChannel textChannel1, ITextChannel textChannel2)
        {
            // Generate Characters' names for embed
			string user1name, user2name;
			user1name = user1.Username + "#" + user1.Discriminator;
			user2name = user2.Username + "#" + user2.Discriminator;

            // Get characters' types for embed
            string type1, type2;
            type1 = Convert.ToString(GetFieldSQL(user1.Id, "type", "users"));
            type2 = Convert.ToString(GetFieldSQL(user2.Id, "type", "users"));

            // Translate in russian
            type1 = ConvertType(type1);
            type2 = ConvertType(type2);

            // Get characters' levels for embed
            int level1, level2;
            level1 = Convert.ToInt32(GetFieldSQL(user1.Id, "level", "users"));
            level2 = Convert.ToInt32(GetFieldSQL(user2.Id, "level", "users"));

            // Generate embed
            var builder = new EmbedBuilder()
                .WithTitle($"Битва между {user1name} и {user2name}")
                .WithDescription($"{type1} {level1} уровня **VS** {type2} {level2} уровня")
                .WithColor(new Color(0xEB6613))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("RPG Awona");
                })
                .WithThumbnailUrl("https://cdn.discordapp.com/embed/avatars/0.png")
                .WithAuthor(author =>
                {
                    author
                        .WithName("RPG Awona");
                });
			var embed = builder.Build();
            // Build and send embeds for 2 channels
			await textChannel1.SendMessageAsync(
					null,
					embed: embed)
					.ConfigureAwait(false);

            await textChannel2.SendMessageAsync(
                    null,
                    embed: embed)
                    .ConfigureAwait(false);
        }

        public async void prikol(int time = 10)
        {
            while (time > 0)
            {
                //await msg1.ModifyAsync(m => m.Content = $"Осталось {time} секунд");
                Console.WriteLine(time);
                Thread.Sleep(1000);
                time--;
            }
        }

        public async Task FightLoop(SocketGuildUser user1, SocketGuildUser user2, Archetype player1, Archetype player2, ITextChannel textChannel1, ITextChannel textChannel2)
        {
            int health1, health2;
            health1 = 1;
            health2 = 1;
            bool surrender = false;
            await Task.Delay(3 * 1000);
            while (health1 > 0 & health2 > 0 & surrender != true)
            {
                health1 = GetHealthSQL(user1.Id, true);
                health2 = GetHealthSQL(user2.Id, false);

                byte time = 10;
                await FightMessage(user1, user2, player1, player2, textChannel1, textChannel2);
                var msg1 = await textChannel1.SendMessageAsync($"Осталось {time} секунд");
                
                prikol(); // :)

                //await msg1.Channel.DeleteMessageAsync(1);

                // Get player1move and player2move from SQL table `duel`
                string player1move, player2move;
                player1move = Convert.ToString(GetFieldSQL(user1.Id, "player1move", "duel")); 
                player2move = Convert.ToString(GetFieldSQL(user2.Id, "player2move", "duel"));

                int player1damage, player2damage;
                player1damage = player1.Action(player1move, player2);
                player2damage = player2.Action(player2move, player1);

                ExecuteSQL($"UPDATE duel SET player1health = {health1 - player2damage} WHERE player1id = {player1.Id}");
                ExecuteSQL($"UPDATE duel SET player2health = {health2 - player1damage} WHERE player2id = {player2.Id}");

            }

            ExecuteSQL($"DELETE FROM duel WHERE player1id = {player1.Id}");

        }

        public async Task<List<Archetype>> FightMessage(SocketGuildUser user1, SocketGuildUser user2, Archetype player1, Archetype player2,  ITextChannel textChannel1, ITextChannel textChannel2)
        {
            // Generate Characters' names for embed
            string user1name, user2name;
            user1name = user1.Username + "#" + user1.Discriminator;
            user2name = user2.Username + "#" + user2.Discriminator;

            // Get characters' levels for embed
            int level1, level2, health1, health2;
            level1 = Convert.ToInt32(GetFieldSQL(user1.Id, "level", "users"));
            level2 = Convert.ToInt32(GetFieldSQL(user2.Id, "level", "users"));

            health1 = GetHealthSQL(user1.Id, true);
            health2 = GetHealthSQL(user2.Id, false);
            Console.WriteLine($"{health1}, !@#$ {health2}");
            var builder = new EmbedBuilder()
                .WithTitle($"Битва между {user1name} и {user2name}")
                .WithDescription($"") // Last move
                .WithColor(new Color(0xEB6613))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("RPG Awona");
                })
                .WithThumbnailUrl("https://cdn.discordapp.com/embed/avatars/0.png")
                .WithAuthor(author =>
                {
                    author
                        .WithName("RPG Awona");
                })
                .AddField("Ваше здоровье :heart:", $"{health1}", inline: true) // Health (auth)
                .AddField("Здоровье противника :heart:", $"{health2}", inline: true) // Health (user)
                .AddField("Ваш урон :crossed_swords:", $"{player1.Damage}", inline: true) // Damage
                .AddField($"Ваша броня :shield:", $"{player1.Defence}", inline: true); // Armor (auth)

            var embed = builder.Build();
            await textChannel1.SendMessageAsync(
                    null,
                    embed: embed)
                    .ConfigureAwait(false);
            List<Archetype> characters = new List<Archetype>();
            characters.Add(player1);
            characters.Add(player2);
            return characters;
        }

        public async Task FinishMessage()
        {
            return;
        }

        public async Task EndFight()
        {
            return;
        }

    }
}
