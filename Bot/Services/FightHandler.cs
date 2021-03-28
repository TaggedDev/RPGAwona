using Discord;
using Discord.Commands;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Types.Melee;
using Bot.Types.Japan;
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
            return type switch
            {
                ("Alchemist") => "Алхимик",
                ("Komtur") => "Комтур",
                ("Asigaru") => "Асигару",
                ("Acolyte") => "Аколит",
                _ => "null",
            };
        }

        static readonly Provider provider = new Provider();

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
                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png")
                .WithAuthor(author =>
                {
                    author
                        .WithName("RPG Awona")
                        .WithIconUrl(@"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png");
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
                //await FightMessage(user1, user2, player1, textChannel1, textChannel2);

                Embed embed1, embed2;
                
                var first = FightMessage(user1, user2, player1, player2);
                var second = FightMessage(user2, user1, player2, player1);
                
                embed1 = first.Build();
                embed2 = second.Build();
                
                await textChannel1.SendMessageAsync(embed: embed1);
                await textChannel2.SendMessageAsync(embed: embed2);

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

                player1.Health = health1 - player2damage;
                player2.Health = health2 - player1damage;

                provider.ExecuteSQL($"UPDATE duel SET player1move = 'Sleep' WHERE player1id = {player1id}");
                provider.ExecuteSQL($"UPDATE duel SET player2move = 'Sleep' WHERE player2id = {player2id}");

                health1 = provider.GetDuelHealthAwona(user1.Id, true);
                health2 = provider.GetDuelHealthAwona(user2.Id, false);

                surrender1 = Convert.ToBoolean(provider.GetFieldAwonaByID("player1surrender", Convert.ToString(user1.Id), "player1id", "duel"));
                surrender2 = Convert.ToBoolean(provider.GetFieldAwonaByID("player2surrender", Convert.ToString(user2.Id), "player2id", "duel"));
            }

            health1 = provider.GetDuelHealthAwona(user1.Id, true);
            health2 = provider.GetDuelHealthAwona(user2.Id, false);

            surrender1 = Convert.ToBoolean(provider.GetFieldAwonaByID("player1surrender", Convert.ToString(user1.Id), "player1id", "duel"));
            surrender2 = Convert.ToBoolean(provider.GetFieldAwonaByID("player2surrender", Convert.ToString(user2.Id), "player2id", "duel"));

            // DM results

            if (health2 < 0)
                await FinishMessage(user1, user2, user1.Username + "#" + user1.Discriminator, surrender1 || surrender2);
            else if (health1 < 0)
                await FinishMessage(user1, user2, user2.Username + "#" + user2.Discriminator, surrender1 || surrender2);
            else if (surrender1)
                await FinishMessage(user1, user2, user2.Username + "#" + user2.Discriminator, surrender1 || surrender2);
            else if (surrender2)
                await FinishMessage(user1, user2, user1.Username + "#" + user1.Discriminator, surrender1 || surrender2);
            else
                Console.WriteLine("Error");

            provider.ExecuteSQL($"DELETE FROM duel WHERE player1id = {user1.Id}");

            publicrole.DeleteAsync();
            firstplayer.DeleteAsync();
            secondplayer.DeleteAsync();
            textChannel1.DeleteAsync();
            textChannel2.DeleteAsync();
            category.DeleteAsync();
            

        }

        public EmbedBuilder FightMessage(SocketGuildUser user1, SocketGuildUser user2, Archetype player1, Archetype player2)
        {
            // Generate Characters' names for embed
            string user1name, user2name;
            user1name = user1.Username + "#" + user1.Discriminator;
            user2name = user2.Username + "#" + user2.Discriminator;

            // Get characters' levels for embed
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"Битва между {user1name} и {user2name}")
                .WithColor(new Color(0xEB6613))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("RPG Awona");
                })
                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png")
                .WithAuthor(author =>
                {
                    author
                        .WithName("RPG Awona")
                        .WithIconUrl(@"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png");
                })
                .AddField("Ваше здоровье :heart:", $"{player1.Health}", inline: true) // Health (auth)
                .AddField("Здоровье противника :heart:", $"{player2.Health}", inline: true) // Health (user)
                .AddField("Ваш урон :crossed_swords:", $"{player1.Damage}") // Damage
                .AddField($"Ваша броня :shield:", $"{player1.Defence}"); // Armor (auth)

            return builder;
        }

        public async Task FinishMessage(SocketGuildUser user1, SocketGuildUser user2, string winnerName, bool isSurrender)
        {

            Subcommand sbc = new Subcommand();

            // Generate Characters' names for embed
            string user1name, user2name;
            user1name = user1.Username + "#" + user1.Discriminator;
            user2name = user2.Username + "#" + user2.Discriminator;

            Embed embedW, embedL;
            var winner = sbc.CreateEmbed(user1name, user2name, true, isSurrender);
            var loser = sbc.CreateEmbed(user1name, user2name, false, isSurrender);
            embedW = winner.Build();
            embedL = loser.Build();

            if (winnerName.Equals(user1.Username + "#" + user1.Discriminator))
            {
                // If the winner is the first player
                // Send him winner embed
                await user1.SendMessageAsync(
                    null,
                    embed: embedW)
                    .ConfigureAwait(false);
                // And send loser embed for second player
                await user2.SendMessageAsync(
                    null,
                    embed: embedL)
                    .ConfigureAwait(false);
            } 
            else
            {
                // If the winner is the second player
                // Send the second player winner embed
                await user2.SendMessageAsync(
                    null,
                    embed: embedW)
                    .ConfigureAwait(false);
                // And loser embed for the first player
                await user1.SendMessageAsync(
                    null,
                    embed: embedL)
                    .ConfigureAwait(false);
            }

            return;
        }
    }
}
