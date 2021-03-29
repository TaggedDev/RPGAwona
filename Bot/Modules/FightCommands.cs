using Bot.Services;
using Bot.Types;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Modules
{
    public class FightCommands : ModuleBase<SocketCommandContext>
    {
        static readonly Provider provider = new Provider();
        static readonly Subcommand subcommand = new Subcommand();

        [Command("challenge")]
        [Alias("versus", "fight", "vs", "destroy", "allez", "alles", "los")]
        public async Task Challenge(SocketGuildUser user = null)
        {
            // Create SocketGuildUser objects
            SocketGuildUser author = (Context.Message.Author as SocketGuildUser);

            ulong context_id = Context.Channel.Id;
            ulong channel_id = 823844887787077682;
            string answer = "";

            if (!subcommand.ValidChecker(user, author, ref answer, channel_id, context_id))
            {
                await ReplyAsync(answer);
                return;
            }
            //
            // Get everyone role, create new role, create permissions, set roles
            //

            // Create user name and author name
            string authorname, username;
            authorname = Context.Message.Author.Username;
            username = user.Username;
            // Create Roles
            IRole everyone = Context.Guild.EveryoneRole;
            IRole publicrole = await Context.Guild.CreateRoleAsync($"{authorname}-vs-{username}", null, new Color(0xf5fffa), false, null);
            IRole firstplayer = await Context.Guild.CreateRoleAsync($"{authorname}#{authorname.Length}", null, new Color(0xf5fffa), false, null);
            IRole secondplayer = await Context.Guild.CreateRoleAsync($"{authorname}#{authorname.Length}", null, new Color(0xf5fffa), false, null);
            // Create Permissions
            OverwritePermissions noView = new OverwritePermissions(viewChannel: PermValue.Deny);
            OverwritePermissions yesView = new OverwritePermissions(viewChannel: PermValue.Allow);
            // Add roles to first and second player
            await (Context.User as IGuildUser).AddRoleAsync(publicrole);
            await (Context.User as IGuildUser).AddRoleAsync(firstplayer);
            await (user as IGuildUser).AddRoleAsync(publicrole);
            await (user as IGuildUser).AddRoleAsync(secondplayer);

            //
            // Create category, set permissions, create two channels
            //

            // Create category and set permissions
            ICategoryChannel category = await Context.Guild.CreateCategoryChannelAsync($"{authorname}-vs-{username}");
            await category.AddPermissionOverwriteAsync(everyone, noView);
            await category.AddPermissionOverwriteAsync(firstplayer, yesView);
            await category.AddPermissionOverwriteAsync(secondplayer, yesView);
            // Create channel 1
            ITextChannel authorchannel = await Context.Guild.CreateTextChannelAsync($"{authorname}-challenge");
            await authorchannel.AddPermissionOverwriteAsync(everyone, noView);
            await authorchannel.AddPermissionOverwriteAsync(firstplayer, yesView);
            await authorchannel.ModifyAsync(x => x.CategoryId = category.Id);
            // Create channel 2
            ITextChannel userchannel = await Context.Guild.CreateTextChannelAsync($"{username}-challenge");
            await userchannel.AddPermissionOverwriteAsync(everyone, noView);
            await userchannel.AddPermissionOverwriteAsync(secondplayer, noView);
            await userchannel.ModifyAsync(x => x.CategoryId = category.Id);

            // 
            // Fight category
            //

            // Start message
            FightHandler fightHandler = new FightHandler();
            await fightHandler.StartMessage(author, user, userchannel, authorchannel);

            // Creating objects
            string type1, type2;
            type1 = Convert.ToString(provider.GetFieldAwonaByID("type", Convert.ToString(author.Id), "discord_id", "users"));
            type2 = Convert.ToString(provider.GetFieldAwonaByID("type", Convert.ToString(user.Id), "discord_id", "users"));
            Archetype player1 = subcommand.CreateClass(type1, author);
            Archetype player2 = subcommand.CreateClass(type2, user);
            provider.ExecuteSQL($"INSERT INTO duel VALUES (\"{authorname}-vs-{username}\", \"{authorname}\", \"{username}\", {author.Id}, {user.Id}, {authorchannel.Id}, {userchannel.Id}, \"Sleep\", \"Sleep\", {player1.Health}, {player2.Health}, false, false)");
            // If created successfully
            if (player1 == null)
            {
                await ReplyAsync("Ошибка при создании первого игрока");
                return;
            }
            else if (player2 == null)
            {
                await ReplyAsync("Ошибка при создании второго игрока");
                return;
            }
            await ReplyAsync(":white_check_mark: Вызов отправлен");
            fightHandler.FightLoop(author, user, player1, player2, category, authorchannel, userchannel, publicrole, firstplayer, secondplayer);


        }

        [Command("attack")]
        [Alias("hit", "att", "damage", "smash", "kill", "dam", "dmg", "atck", "atack", "attck", "attac", "attak")]
        public async Task Attack()
        {
            string userid;
            userid = Convert.ToString(Context.User.Id);
            //string channe1lid = Convert.ToString(provider.GetFieldAwonaByID("channel1id", Convert.ToString(userid), "player1id", "duel"));
            //string channe2lid = Convert.ToString(provider.GetFieldAwonaByID("channel2id", Convert.ToString(userid), "player2id", "duel"));
            
            // If player is already in a battle
            if (!(provider.UserAlreadyInBattle(userid, true) || provider.UserAlreadyInBattle(userid, false)))
                return;
            
            // If user is not typing in his channel (in progress)
            /*if (!(channe1lid.Equals(Context.Channel.Id) || channe2lid.Equals(Context.Channel.Id)))
                return;*/

            string p1id, p2id;
            p1id = Convert.ToString(provider.GetDuelIDAwona(userid, true)); // get 1st player id
            p2id = Convert.ToString(provider.GetDuelIDAwona(userid, false)); // get 2nd player id

            if (p1id.Equals(userid))
            {
                provider.ExecuteSQL($"UPDATE duel SET player1move = 'Attack' WHERE player1id = {userid}");
                await ReplyAsync("Ваш ход был засчитан как **атака**");
            }
            else if (p2id.Equals(userid))
            {
                provider.ExecuteSQL($"UPDATE duel SET player2move = 'Attack' WHERE player2id = {userid}");
                await ReplyAsync("Ваш ход был засчитан как **атака**");
            }
            else
                await ReplyAsync("Ошибка");
        }

        [Command("defend")]
        [Alias("shield", "defence", "s", "def", "de", "defenc", "sh")]
        public async Task Defend()
        {
            string userid;
            userid = Convert.ToString(Context.User.Id);

            // If player is already in a battle

            if (!(provider.UserAlreadyInBattle(userid, true) || provider.UserAlreadyInBattle(userid, false)))
                return;
            // If user is not typing in his channel

            string p1id, p2id;
            p1id = Convert.ToString(provider.GetDuelIDAwona(userid, true)); // get 1st player id
            p2id = Convert.ToString(provider.GetDuelIDAwona(userid, false)); // get 2nd player id

            if (p1id.Equals(userid))
            {
                provider.ExecuteSQL($"UPDATE duel SET player1move = 'Defend' WHERE player1id = {userid}");
                await ReplyAsync("Ваш ход был засчитан как **защита**");
            }
            else if (p2id.Equals(userid))
            {
                provider.ExecuteSQL($"UPDATE duel SET player2move = 'Defend' WHERE player2id = {userid}");
                await ReplyAsync("Ваш ход был засчитан как **защита**");
            }
            else
                await ReplyAsync("Ошибка");
        }

        [Command("parry")]
        [Alias("parri", "par", "parr", "pary", "pari", "parre", "parade")]
        public async Task Parry()
        {
            string userid;
            userid = Convert.ToString(Context.User.Id);
            // If player is already in a battle

            if (!(provider.UserAlreadyInBattle(userid, true) || provider.UserAlreadyInBattle(userid, false)))
                return;
            // If user is not typing in his channel (in progress)
            /*if (!((channel1id.Equals(Context.Channel.Id) || channel2id.Equals(Context.Channel.Id))))
                return;*/

            string p1id, p2id;
            p1id = Convert.ToString(provider.GetDuelIDAwona(userid, true)); // get 1st player id
            p2id = Convert.ToString(provider.GetDuelIDAwona(userid, false)); // get 2nd player id

            if (p1id.Equals(userid))
            {
                provider.ExecuteSQL($"UPDATE duel SET player1move = 'Parry' WHERE player1id = {userid}");
                await ReplyAsync("Ваш ход был засчитан как **парирование**");
            }
            else if (p2id.Equals(userid))
            {
                provider.ExecuteSQL($"UPDATE duel SET player2move = 'Parry' WHERE player2id = {userid}");
                await ReplyAsync("Ваш ход был засчитан как **парирование**");
            }
            else
                await ReplyAsync("Ошибка");
        }

        [Command("surrender")]
        [Alias("surr", "sur", "srnd", "srrnd", "giveup", "give_up", "gu", "gvp", "capituler")]
        public async Task Surrender()
        {
            string userid;
            userid = Convert.ToString(Context.User.Id);
            string player1id, player2id;
            player1id = Convert.ToString(provider.GetFieldAwonaByID("player1id", userid, "player1id", "duel"));
            player2id = Convert.ToString(provider.GetFieldAwonaByID("player2id", userid, "player2id", "duel"));
            if (userid.Equals(player1id))
                provider.ExecuteSQL($"UPDATE duel SET player1surrender = 1 WHERE player1id = {player1id}");
            else
                provider.ExecuteSQL($"UPDATE duel SET player2surrender = 1 WHERE player2id = {player2id}");
            await ReplyAsync("Вы сдались.");
        }

    }
}
