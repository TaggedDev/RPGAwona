using Bot.Modules;
using Bot.Types;
using Bot.Types.Magic;
using Bot.Types.Honor;
using Bot.Types.Serenity;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Services
{
    class Subcommand
    {
        static readonly Provider provider = new Provider();

        public bool ValidChecker(SocketGuildUser user, SocketGuildUser author, ref string answer, ulong channel_id, ulong context_id)
        {
            bool is1created, is2created;
            is1created = provider.UserAlreadyCreated(Convert.ToString(user.Id));
            is2created = provider.UserAlreadyCreated(Convert.ToString(author.Id));
            // Check is user parameter is invalid
            if (user == null)
            {
                answer = ":x: Вы не указали соперника";
                return false;
            }
            else if (!is1created && !is2created)
            {
                answer = ":x: У одного из игроков нет персонажа";
                return false;
            }
            else if (user.Id == author.Id)
            {
                answer = ":x: Вы не можете начать битву с собой";
                return false;
            }
            // If player is already in Battle
            bool authorbattle, userbattle;
            authorbattle = provider.UserAlreadyInBattle(Convert.ToString(author.Id), true);
            userbattle = provider.UserAlreadyInBattle(Convert.ToString(user.Id), false);
            if (authorbattle || userbattle)
            {
                answer = "Один из игроков уже находится в бою, подождите и попробуйте снова";
                return false;
            }
            else if (user.Id == 822717022374068224)
            {
                answer = ":x: Вы не можете начать битву с ботом";
                return false;
            }
            // Check if its a versus channel
            else if (context_id != channel_id)
            {
                answer = "Бросать вызов в другом месте! Вам нужно в трактир - <#823844887787077682>";
                return false;
            }
            return true;
        }

        public Archetype CreateClass(string type, SocketGuildUser guildUser)
        {
            switch (type)
            {
                case ("Acolyte"):
                    Acolyte acolyte = new Acolyte(guildUser.Username, guildUser.Id);
                    return acolyte;
                case ("Komtur"):
                    Honor komtur = new Honor(guildUser.Username, guildUser.Id);
                    return komtur;
                case ("Asigaru"):
                    Asigaru asigaru = new Asigaru(guildUser.Username, guildUser.Id);
                    return asigaru;
                case ("Alchemist"):
                    Alchemist alchemist = new Alchemist(guildUser.Username, guildUser.Id);
                    return alchemist;
                default:
                    return null;
            }
        }

        public string LastMove(string message)
        {
            return message switch
            {
                ("Attack") => "Атака",
                ("Defend") => "Защита",
                ("Ability") => "Способность",
                _ => "Нет хода",
            };
        }

        public EmbedBuilder CreateEmbed(string user1name, string user2name, bool winner, bool isSurrender)
        {
            uint color;
            string message, link;
            if (winner)
            {
                color = 0x21ff25;
                message = "Вы одержали победу!";
                link = @"https://cdn.discordapp.com/attachments/823526896582656031/825738803511033856/Win.png";
                if (isSurrender)
                    message = "Вы одержали победу, противник сдался!";
            }
            else
            {
                color = 0xff3b21;
                link = @"https://cdn.discordapp.com/attachments/823526896582656031/825738800814489603/Lose.png";
                message = "Вы проиграли!";
                if (isSurrender)
                    message = "Вы сдались!";
            }


            var builder = new EmbedBuilder()
                .WithTitle($"{user1name} :crossed_swords: {user2name}")
                .WithColor(new Color(color))
                .WithFooter(footer =>
                {
                    footer
                        .WithText("RPG Awona");
                })
                .WithThumbnailUrl(link)
                .WithAuthor(author =>
                {
                    author
                        .WithName("RPG Awona")
                        .WithIconUrl(@"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png");
                })

                .AddField("Результат:", $"{message}", inline: true); // Health (auth)

            return builder;
        }
    }
}
