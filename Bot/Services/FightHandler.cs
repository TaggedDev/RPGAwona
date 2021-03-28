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
    class FightHandler
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

        static Provider provider = new Provider();

        public async Task StartMessage(SocketGuildUser user1, SocketGuildUser user2, ITextChannel textChannel1, ITextChannel textChannel2)
        {
            // Generate Characters' names for embed
			string user1name, user2name;
			user1name = user1.Username + "#" + user1.Discriminator;
			user2name = user2.Username + "#" + user2.Discriminator;

            // Get characters' types for embed
            string type1, type2;
            type1 = Convert.ToString(provider.GetFieldAwonaByID("type", Convert.ToString(user1.Id), "discord_id","users"));
            type2 = Convert.ToString(provider.GetFieldAwonaByID("type", Convert.ToString(user2.Id), "discord_id", "users"));

            // Translate in russian
            type1 = ConvertType(type1);
            type2 = ConvertType(type2);

            // Get characters' levels for embed
            int level1, level2;
            level1 = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(user1.Id), "discord_id", "users"));
            level2 = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(user2.Id), "discord_id", "users"));

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

        public async Task FightLoop(SocketGuildUser user1, SocketGuildUser user2, Archetype player1, Archetype player2, ICategoryChannel category, ITextChannel textChannel1, ITextChannel textChannel2, IRole publicrole, IRole firstplayer, IRole secondplayer)
        {
            int health1, health2;
            health1 = 1;
            health2 = 1;
            bool surrender1, surrender2;
            surrender1 = Convert.ToBoolean(provider.GetFieldAwonaByID("player1surrender", Convert.ToString(user1.Id), "player1id", "duel"));
            surrender2 = Convert.ToBoolean(provider.GetFieldAwonaByID("player2surrender", Convert.ToString(user2.Id), "player2id", "duel"));
            await Task.Delay(3 * 1000);
            while (health1 > 0 && health2 > 0 && !surrender1 && !surrender2)
            {
                byte time = 10;
                await FightMessage(user1, user2, player1, player2, textChannel1, textChannel2);
                var msg1 = await textChannel1.SendMessageAsync($"У вас {time} секунд");

                while (time > 0)
                {
                    //await msg1.ModifyAsync(m => m.Content = $"Осталось {time} секунд");
                    Thread.Sleep(1000);
                    time--;
                }
                await msg1.DeleteAsync();

                // Get player1move and player2move from SQL table `duel`
                string player1move, player2move;
                player1move = Convert.ToString(provider.GetFieldAwonaByID("player1move", Convert.ToString(user1.Id), "player1id", "duel")); 
                player2move = Convert.ToString(provider.GetFieldAwonaByID("player2move", Convert.ToString(user2.Id), "player2id", "duel"));

                int player1damage, player2damage;
                player1damage = player1.Action(player1move, player2move, player2);
                player2damage = player2.Action(player2move, player1move, player1);

                health1 = provider.GetDuelHealthAwona(user1.Id, true);
                health2 = provider.GetDuelHealthAwona(user2.Id, false);
                if (health1 < 0) health1 = 0;
                if (health2 < 0) health2 = 0;

                string player1id = Convert.ToString(user1.Id), player2id = Convert.ToString(user2.Id);
                provider.ExecuteSQL($"UPDATE duel SET player1health = {health1 - player2damage} WHERE player1id = {player1id}");
                provider.ExecuteSQL($"UPDATE duel SET player2health = {health2 - player1damage} WHERE player2id = {player2id}");

                provider.ExecuteSQL($"UPDATE duel SET player1move = 'Sleep' WHERE player1id = {player1id}");
                provider.ExecuteSQL($"UPDATE duel SET player2move = 'Sleep' WHERE player2id = {player2id}");

                health1 = provider.GetDuelHealthAwona(user1.Id, true);
                health2 = provider.GetDuelHealthAwona(user2.Id, false);

                surrender1 = Convert.ToBoolean(provider.GetFieldAwonaByID("player1surrender", Convert.ToString(user1.Id), "player1id", "duel"));
                surrender2 = Convert.ToBoolean(provider.GetFieldAwonaByID("player2surrender", Convert.ToString(user2.Id), "player2id", "duel"));
            }

            provider.ExecuteSQL($"DELETE FROM duel WHERE player1id = {user1.Id}");
            textChannel1.DeleteAsync();
            textChannel2.DeleteAsync();
            category.DeleteAsync();
            publicrole.DeleteAsync();
            firstplayer.DeleteAsync();
            secondplayer.DeleteAsync();

        }

        public async Task<List<Archetype>> FightMessage(SocketGuildUser user1, SocketGuildUser user2, Archetype player1, Archetype player2,  ITextChannel textChannel1, ITextChannel textChannel2)
        {
            // Generate Characters' names for embed
            string user1name, user2name;
            user1name = user1.Username + "#" + user1.Discriminator;
            user2name = user2.Username + "#" + user2.Discriminator;

            // Get characters' levels for embed
            int level1, level2, health1, health2;
            level1 = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(user1.Id), "discord_id", "users"));
            level2 = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(user2.Id), "discord_id", "users"));

            health1 = provider.GetDuelHealthAwona(user1.Id, true);
            health2 = provider.GetDuelHealthAwona(user2.Id, false);

            var builder = new EmbedBuilder()
                .WithTitle($"Битва между {user1name} и {user2name}")
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
            await textChannel2.SendMessageAsync(
                    null,
                    embed: embed)
                    .ConfigureAwait(false);
            List<Archetype> characters = new List<Archetype>();
            characters.Add(player1);
            characters.Add(player2);
            return characters;
        }

        public void FinishMessage()
        {
            return;
        }
    }
}
