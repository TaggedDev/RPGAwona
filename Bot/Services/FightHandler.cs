using Discord;
using Discord.Commands;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Types.Melee;
using Bot.Types.Serenity;
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
                .WithDescription($"{type1} {level1} уровня **VS** {type2} {level2} уровня\n\n!attack - атака !defend - защита !ability - способность")
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
            bool surrender1, surrender2;

            surrender1 = Convert.ToBoolean(provider.GetFieldAwonaByID("player1surrender", Convert.ToString(user1.Id), "player1id", "duel"));
            surrender2 = Convert.ToBoolean(provider.GetFieldAwonaByID("player2surrender", Convert.ToString(user2.Id), "player2id", "duel"));
            
            await Task.Delay(3 * 1000);
            int counter = 1;

            while (player1.Health > 0 && player2.Health > 0 && !surrender1 && !surrender2)
            {
                byte time = 10;

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

                ResultMessage(player1damage, player2damage, counter, textChannel1, textChannel2);
                counter++;

                if (player2damage < 0) player2damage = 0;
                if (player1damage < 0) player1damage = 0;

                player1.Health -= player2damage;
                player2.Health -= player1damage;

                string player1id = Convert.ToString(user1.Id), player2id = Convert.ToString(user2.Id);
                provider.ExecuteSQL($"UPDATE duel SET player1move = 'Sleep' WHERE player1id = {player1id}");
                provider.ExecuteSQL($"UPDATE duel SET player2move = 'Sleep' WHERE player2id = {player2id}");

                surrender1 = Convert.ToBoolean(provider.GetFieldAwonaByID("player1surrender", Convert.ToString(user1.Id), "player1id", "duel"));
                surrender2 = Convert.ToBoolean(provider.GetFieldAwonaByID("player2surrender", Convert.ToString(user2.Id), "player2id", "duel"));
                
            }

            surrender1 = Convert.ToBoolean(provider.GetFieldAwonaByID("player1surrender", Convert.ToString(user1.Id), "player1id", "duel"));
            surrender2 = Convert.ToBoolean(provider.GetFieldAwonaByID("player2surrender", Convert.ToString(user2.Id), "player2id", "duel"));

            // DM results

            if (player2.Health < 0)
                await FinishMessage(user1, user2, user1.Username + "#" + user1.Discriminator, surrender1 || surrender2);
            else if (player1.Health < 0)
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

        private async void ResultMessage(int player1damage, int player2damage, int counter, ITextChannel tc1, ITextChannel tc2)
        {

            string msg_s = player1damage switch
            {
                (-1) => "Удар первого игрока был заблокирован",
                (-2) => "Игроки парировали друг друга",
                (-3) => "Первый игрок не пробил второго игрока",
                (-4) => "Второй игрок уклонился от удара",
                (-5) => "Ошибка",
                (-6) => "Первый игрок проспал",
                _ => $"Первый игрок нанёс :dagger: {player1damage} урона.",
            };

            msg_s += player2damage switch
            {
                (-1) => "\nУдар второго игрока был заблокирован",
                (-2) => "\nИгроки парировали друг друга",
                (-3) => "\nВторой игрок не пробил первого игрока",
                (-4) => "\nПервый игрок уклонился от удара",
                (-5) => "\nОшибка",
                (-6) => "\nВторой игрок проспал",
                _ => $"\nВторой игрок нанёс :dagger: {player2damage} урона.",
            };

            Embed embed;
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"Ход {counter}")
                .WithDescription(msg_s)
                .WithColor(new Color(0xEB6613))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("RPG Awona")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png");

                })
                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png")
                .WithAuthor(author =>
                {
                    author
                        .WithName("RPG Awona")
                        .WithIconUrl(@"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png");
                });
            embed = builder.Build();
            await tc1.SendMessageAsync(
                    null,
                    embed: embed)
                    .ConfigureAwait(false);
            await tc2.SendMessageAsync(
                    null,
                    embed: embed)
                    .ConfigureAwait(false);
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
                .AddField($"Ваша броня :shield:", $"{player1.Armor}"); // Armor (auth)

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
