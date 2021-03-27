using Bot.Modules;
using Bot.Types;
using Bot.Types.Magic;
using Bot.Types.Melee;
using Bot.Types.Ranged;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Services
{
    class Subcommand
    {
        static Provider provider = new Provider();

        public bool ValidChecker(SocketGuildUser user, SocketGuildUser author, ref string answer, ulong channel_id, ulong context_id)
        {

            // Check is user parameter is invalid
            if (user == null)
            {
                answer = ":x: Вы не указали соперника";
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
                    Komtur komtur = new Komtur(guildUser.Username, guildUser.Id);
                    return komtur;
                case ("Thrower"):
                    Thrower thrower = new Thrower(guildUser.Username, guildUser.Id);
                    return thrower;
                case ("Alchemist"):
                    Alchemist alchemist = new Alchemist(guildUser.Username, guildUser.Id);
                    return alchemist;
                default:
                    return null;
            }
        }

        public string LastMove(string message)
        {
            Console.WriteLine(message);
            switch (message)
            {
                case ("Attack"):
                    return "Атака";
                case ("Defend"):
                    return "Защита";
                case ("Parry"):
                    return "Парирование";
                default:
                    return "Нет хода";
            }
        }
    }
}
