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
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Commands> _logger;
        
        public Commands(ILogger<Commands> logger)
            => _logger = logger;

        public static void ExecuteSQL(string cmd)
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

        static bool AlreadyCreated(string id)
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

        static object GetFieldSQL(string field, ulong id, string sqlExpression)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                //string sqlExpression = "SELECT discord_id FROM users";
                SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                        while (reader.Read())   // построчно считываем данные
                        {
                            string getId = Convert.ToString(reader.GetValue(0));
                            if (getId.Equals(Convert.ToString(id)))
                            {
                                if (field.Equals("level"))
                                    return reader["level"];
                                else if (field.Equals("archetype"))
                                    return reader["archetype"];
                                else if (field.Equals("type"))
                                    return reader["type"];
                            }
                        }
                }
            }
            return null;
        }

        [Command("create")]
        [Alias("create_character")]
        public async Task CreateCharacter(string archetype)
        {
            if (!AlreadyCreated(Convert.ToString(Context.User.Id)))
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
        [Alias("delete_character")]
        public async Task DeleteCharacter()
        {
            string id, command;
            id = Convert.ToString(Context.User.Id);
            command = $"DELETE FROM users WHERE discord_id = {id}";
            if (AlreadyCreated(id))
            { 
                ExecuteSQL(command);
                await ReplyAsync(":white_check_mark: Персонаж успешно удалён");
            }
            else
                await ReplyAsync(":x: У вас ещё нет персонажа.");


        }

        [Command("character")]
        [Alias("char", "c")]
        public async Task CharacterWindow()
        {
            string type, archetype;
            int level;

            type = Convert.ToString(GetFieldSQL("type", Context.User.Id, "SELECT discord_id, type FROM users"));
            archetype = Convert.ToString(GetFieldSQL("archetype", Context.User.Id, "SELECT discord_id, archetype FROM users"));
            level = Convert.ToInt32(GetFieldSQL("level", Context.User.Id, "SELECT discord_id, level FROM users"));
            char firstLetter = archetype[0];
            archetype = char.ToUpper(firstLetter) + archetype.Substring(1);

            uint color = archetype switch
            {
                ("Faith") => 0xE7EB2D,
                ("Ranged") => 0x2DEB8C,
                ("Magic") => 0xD52DEB,
                ("Melee") => 0xCF3232,
                _ => 0xE0D41B,
            };

            int damage, health, armor;
            float luck, agility;

            string name;
            ulong discord_id;
            name = Context.User.Username + "#" + Context.User.Discriminator;
            discord_id = Context.User.Id;

            Console.WriteLine(type);

            switch (type)
            {
                case ("Acolyte"):
                    Acolyte acolyte = new Acolyte(name, discord_id);
                    damage = acolyte.Damage;
                    health = acolyte.Health;
                    armor = acolyte.Defence;
                    luck = acolyte.Luck;
                    agility = acolyte.Dodge;
                    break;
                case ("Komtur"):
                    Komtur komtur = new Komtur(name, discord_id);
                    damage = komtur.Damage;
                    health = komtur.Health;
                    armor = komtur.Defence;
                    luck = komtur.Luck;
                    agility = komtur.Dodge;
                    break;
                case ("Thrower"):
                    Thrower thrower = new Thrower(name, discord_id);
                    damage = thrower.Damage;
                    health = thrower.Health;
                    armor = thrower.Defence;
                    luck = thrower.Luck;
                    agility = thrower.Dodge;
                    break;
                case ("Alchemist"):
                    Alchemist alchemist = new Alchemist(name, discord_id);
                    damage = alchemist.Damage;
                    health = alchemist.Health;
                    armor = alchemist.Defence;
                    luck = alchemist.Luck;
                    agility = alchemist.Dodge;
                    break;
                default:
                    damage = 0;
                    health = 0;
                    armor = 0;
                    luck = 0;
                    agility = 0;
                    break;
            }

            var builder = new EmbedBuilder()
                .WithTitle($"{name}")
                .WithDescription($"**Окно персонажа**\n\nКласс: {type}\nАрхетип: {archetype}\nУровень: {level}\n")
                //.WithUrl("https://discordapp.com")
                .WithColor(new Color(color))
                .WithTimestamp(DateTimeOffset.FromUnixTimeMilliseconds(1616356046810))
                .WithFooter(footer => {
                    footer
                        .WithText("footer text")
                        .WithIconUrl("https://cdn.discordapp.com/embed/avatars/0.png");
                })
                .WithThumbnailUrl("https://cdn.discordapp.com/embed/avatars/0.png")
                .WithAuthor(author => {
                    author
                        .WithName("Awona")
                        .WithUrl("https://discordapp.com")
                        .WithIconUrl("https://cdn.discordapp.com/embed/avatars/0.png");
                })
                .AddField("Урон", $"{damage}")
                .AddField("Здоровье", $"{health}")
                .AddField("Защита", $"{armor}")
                .AddField("Удача", $"{luck}")
                .AddField("Ловкость", $"{agility}");
                        var embed = builder.Build();
                        await Context.Channel.SendMessageAsync(
                            null,
                            embed: embed)
                            .ConfigureAwait(false);


        }
    
        [Command("challenge")]
        [Alias("versus", "fight", "vs", "destroy")]
        public async Task Challenge(SocketGuildUser user = null)
        {
            SocketUser messageAuthor = Context.Message.Author;
            SocketGuildUser author = (messageAuthor as SocketGuildUser);

            // Check is user parameter is invalid
            if (user == null)
            {
                await ReplyAsync(":x: Вы не указали соперника");
                return;
            } 
            else if (user != Context.Message.Author)
            {
                await ReplyAsync(":x: Вы не можете начать битву с собой");
                return;
            } 
            else if (user.Id == 822717022374068224)
            {
                await ReplyAsync(":x: Вы не можете начать битву с ботом");
                return;
            }

            // Check if its a versus channel
            if (Context.Channel.Id != 823844887787077682)
            {
                await ReplyAsync("Бросать вызов в другом месте! Вам нужно в трактир - <#823844887787077682>");
                return;
            }
                

            string authorname, username;
            authorname = Context.Message.Author.Username;
            username = user.Username;
            
            //
            // Get everyone role, create new role, create permissions, set roles
            //

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

            FightHandler fightHandler = new FightHandler();
            await fightHandler.StartMessage(author, user, userchannel, authorchannel);

            
            Acolyte acolyte = new Acolyte(authorname, author.Id);
            Thrower thrower = new Thrower(username, user.Id);

            while (true)
            {
                
                await fightHandler.FightMessage(author, user, acolyte, thrower, authorchannel, userchannel);
            }


        }
    
        
    }
}
