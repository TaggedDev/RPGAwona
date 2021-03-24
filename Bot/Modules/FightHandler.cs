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

        public async Task FightLoop(SocketGuildUser user1, SocketGuildUser user2, Archetype player1, Archetype player2, ITextChannel textChannel1, ITextChannel textChannel2)
        {
            int health1, health2;
            health1 = player1.Health;
            health2 = player2.Health;
            bool surrender = false;
            while (health1 >= 0 || health2 >= 0 || surrender != true)
            {
                health1 = Convert.ToInt32(GetFieldSQL(user1.Id, "player1health", "duel"));
                health2 = Convert.ToInt32(GetFieldSQL(user2.Id, "player2health", "duel"));
                byte time = 10;
                await FightMessage(user1, user2, player1, player2, textChannel1, textChannel2);
                var msg1 = await textChannel1.SendMessageAsync($"Осталось {time} секунд");
                while (time > 0)
                {
                    await msg1.ModifyAsync(m => m.Content = $"Осталось {time} секунд");
                    await Task.Delay(1 * 1000);
                    time--;
                }



                await msg1.Channel.DeleteMessageAsync(1);
                
            }
        }

        public async Task<List<Archetype>> FightMessage(SocketGuildUser user1, SocketGuildUser user2, Archetype player1, Archetype player2,  ITextChannel textChannel1, ITextChannel textChannel2)
        {
            // Generate Characters' names for embed
            string user1name, user2name;
            user1name = user1.Username + "#" + user1.Discriminator;
            user2name = user2.Username + "#" + user2.Discriminator;

            // Get characters' levels for embed
            int level1, level2;
            level1 = Convert.ToInt32(GetFieldSQL(user1.Id, "level", "users"));
            level2 = Convert.ToInt32(GetFieldSQL(user2.Id, "level", "users"));

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
                .AddField("Ваше здоровье :heart:", $"{player1.Health}", inline: true) // Health (auth)
                .AddField("Здоровье противника :heart:", $"{player2.Health}", inline: true) // Health (user)
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
