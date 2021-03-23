using Discord;
using Discord.Commands;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Types.Melee;
using Bot.Types.Faith;
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

        static object GetFieldSQL(ulong id, string field)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                string sqlExpression = "SELECT * FROM users";
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
            type1 = Convert.ToString(GetFieldSQL(user1.Id, "type"));
            type2 = Convert.ToString(GetFieldSQL(user2.Id, "type"));

            // Translate in russian
            type1 = ConvertType(type1);
            type2 = ConvertType(type2);

            // Get characters' levels for embed
            int level1, level2;
            level1 = Convert.ToInt32(GetFieldSQL(user1.Id, "level"));
            level2 = Convert.ToInt32(GetFieldSQL(user2.Id, "level"));

            // Generate embed
            var builder = new EmbedBuilder()
                .WithTitle($"Битва между {user1name} и {user2name}")
                .WithDescription($"**{type1} {level1} уровня** ***VS*** **{type2} {level2} уровня**")
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

        public async Task FightMessage(SocketGuildUser user1, SocketGuildUser user2, Archetype player1, Archetype player2,  ITextChannel textChannel1, ITextChannel textChannel2)
        {
            // Generate Characters' names for embed
            string user1name, user2name;
            user1name = user1.Username + "#" + user1.Discriminator;
            user2name = user2.Username + "#" + user2.Discriminator;

            

            // Get characters' types for embed
            string type1, type2;
            type1 = Convert.ToString(GetFieldSQL(user1.Id, "type"));
            type2 = Convert.ToString(GetFieldSQL(user2.Id, "type"));

            // Translate in russian
            type1 = ConvertType(type1);
            type2 = ConvertType(type2);

            // Get characters' levels for embed
            int level1, level2;
            level1 = Convert.ToInt32(GetFieldSQL(user1.Id, "level"));
            level2 = Convert.ToInt32(GetFieldSQL(user2.Id, "level"));

            var builder = new EmbedBuilder()
                .WithTitle($"Битва между {user1name} и {user2name}")
                .WithDescription($"**{type1} {level1} уровня** ***VS*** **{type2} {level2} уровня**")
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

            return;
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
