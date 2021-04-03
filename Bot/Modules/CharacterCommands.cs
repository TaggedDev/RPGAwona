using Discord;
using Discord.Commands;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bot.Types.Honor;
using Bot.Types.Serenity;
using Bot.Types.Magic;
using Bot.Types;
using Bot.Services;
using Discord.WebSocket;
using Discord.Rest;
using System.Linq;

namespace Bot.Modules
{
    public class CharacterCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<CharacterCommands> _logger;
        
        public CharacterCommands(ILogger<CharacterCommands> logger)
            => _logger = logger;

        static readonly Provider provider = new Provider();
        static readonly Subcommand subcommand = new Subcommand();


        [Command("create")]
        [Alias("create_character")]
        public async Task CreateCharacter(string archetype = null)
        {
            if (archetype == null) {
                await ReplyAsync(":x: Выберите один из четырёх археклассов (Faith, Serenity, Honor, Magic)");
                return;
            }
                
            if (!provider.UserAlreadyCreated(Convert.ToString(Context.User.Id)))
            {
                int level;
                string type, discord_id;
                archetype = archetype.ToLower();

                if (!(archetype.Equals("faith") || archetype.Equals("serenity") || archetype.Equals("honor") || archetype.Equals("magic")))
                    await ReplyAsync(":x: Выберите один из четырёх археклассов (Faith, Serenity, Honor, Magic)");
                else
                {
                    type = archetype switch
                    {
                        ("faith") => "Acolyte",
                        ("serenity") => "Asigaru",
                        ("magic") => "Alchemist",
                        ("honor") => "Komtur",
                        _ => "Komtur",
                    };
                    discord_id = Convert.ToString(Context.User.Id);
                    level = 1;
                    provider.ExecuteSQL($"INSERT INTO stats (discord_id, wins, loses, fights) VALUES ('{discord_id}', '0', '0', '0')");
                    provider.ExecuteSQL($"INSERT INTO users (discord_id, level, money, exp, archetype, type) VALUES ('{discord_id}', '{level}', 0, 0, '{archetype}', '{type}')");
                    IRole role = archetype switch
                    {
                        ("faith") => Context.Guild.GetRole(825802241277165598),
                        ("serenity") => Context.Guild.GetRole(825802240609484880),
                        ("magic") => Context.Guild.GetRole(825802241616510977),
                        ("honor") => Context.Guild.GetRole(825802244825284688),
                        _ => Context.Guild.GetRole(825802244825284688),
                    };
                    
                    await (Context.User as IGuildUser).AddRoleAsync(role);
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
            
            if (provider.UserAlreadyCreated(id))
            {
                string archetype = Convert.ToString(provider.GetFieldAwonaByID("archetype", id, "discord_id", "users"));
                archetype = archetype.ToLower();

                IRole role = archetype switch
                {
                    ("faith") => Context.Guild.GetRole(825802241277165598),
                    ("serenity") => Context.Guild.GetRole(825802240609484880),
                    ("magic") => Context.Guild.GetRole(825802241616510977),
                    ("honor") => Context.Guild.GetRole(825802244825284688),
                    _ => Context.Guild.GetRole(825802244825284688),
                };
                await (Context.User as IGuildUser).RemoveRoleAsync(role);

                provider.ExecuteSQL($"DELETE FROM users WHERE discord_id = {id}");
                provider.ExecuteSQL($"DELETE FROM stats WHERE discord_id = {id}");

                await ReplyAsync(":white_check_mark: Персонаж успешно удалён");
            }
            else
                await ReplyAsync(":x: У вас ещё нет персонажа.");
        }

        [Command("character")]
        [Alias("char", "c", "cw", "me", "mychar", "my_char", "c_window", "w")]
        public async Task CharacterWindow()
        {
            string type, archetype, name, wins, fights;
            int level, damage, health, armor, currentExp, needExp, myExp;
            float luck, agility;
            ulong discord_id;

            int[] expForLevel = { 0, 250, 516, 844, 1312, 1974, 2966, 4427, 6601, 9850, 13066, 16510, 20356, 24770, 29913, 34642, 39244, 43913, 48774, 57866 };

            type = Convert.ToString(provider.GetFieldAwonaByID("type", Convert.ToString(Context.User.Id), "discord_id", "users"));
            archetype = Convert.ToString(provider.GetFieldAwonaByID("archetype", Convert.ToString(Context.User.Id), "discord_id", "users"));
            level = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(Context.User.Id), "discord_id", "users"));
            char firstLetter = archetype[0];
            archetype = char.ToUpper(firstLetter) + archetype[1..];

            uint color = archetype switch
            {
                ("Faith") => 0xE7EB2D,
                ("Serenity") => 0x2DEB8C,
                ("Magic") => 0xD52DEB,
                ("Honor") => 0xCF3232,
                _ => 0xE0D41B,
            };

            name = Context.User.Username + "#" + Context.User.Discriminator;
            discord_id = Context.User.Id;

            wins = Convert.ToString(provider.GetFieldAwonaByID("wins", Convert.ToString(discord_id), "discord_id", "stats"));
            fights = Convert.ToString(provider.GetFieldAwonaByID("fights", Convert.ToString(discord_id), "discord_id", "stats"));

            if (fights.Equals("0")) fights = "N/A";
            
            Archetype character = subcommand.CreateClass(type, Context.User as SocketGuildUser);
            damage = character.Damage;
            health = character.Health;
            armor = character.Armor;
            luck = character.Luck;
            agility = character.Dodge;

            if (level != 20) { 
                myExp = Convert.ToInt32(provider.GetFieldAwonaByID("exp", $"{discord_id}", "discord_id", "users"));
                if (level == 1)
                    currentExp = myExp;
                else 
                    currentExp = myExp - expForLevel[level - 1];
                needExp = expForLevel[level];
            } 
            else
            {
                currentExp = expForLevel[19];
                needExp = expForLevel[19];
            }

            string classlink = type switch
            {
                ("Acolyte") => @"https://cdn.discordapp.com/attachments/823526896582656031/825726887296106517/Acolyte.png",
                ("Asigaru") => @"https://cdn.discordapp.com/attachments/823526896582656031/825726889272148039/Asigaru.png",
                ("Alchemist") => @"https://cdn.discordapp.com/attachments/823526896582656031/825727004921692180/Alchemist.png",
                ("Komtur") => @"https://cdn.discordapp.com/attachments/823526896582656031/825726890609999902/Komtur.png",
                _ => @"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png",
            };

            archetype = archetype switch
            {
                ("Faith") => "Вера",
                ("Serenity") => "Безмятежность",
                ("Magic") => "Магия",
                ("Honor") => "Честь",
                _ => "Ошибка",
            };

            type = type switch
            {
                ("Acolyte") => "Аколит",
                ("Asigaru") => "Асигару",
                ("Alchemist") => "Алхимик",
                ("Komtur") => "Комтур",
                _ => "Ошибка"
            };


            var builder = new EmbedBuilder()
                .WithTitle($"{name}")
                .WithDescription($"**Окно персонажа**\n\n<:awona:825766545279549500> Класс: **{type}**\n:sewing_needle: Архетип: **{archetype}**\n<:lvlup:825766544586833981> Уровень: **{level} ({currentExp}\\{needExp})**\n:crossed_swords: Всего боёв: **{fights}**\n:100: Процент побед: **{wins}%**")
                //.WithUrl("https://discordapp.com")
                .WithColor(new Color(color))
                .WithTimestamp(DateTimeOffset.FromUnixTimeMilliseconds(1616356046810))
                .WithFooter(footer => {
                    footer
                        .WithText("Awona RPG | Discord | Character window")
                        .WithIconUrl(@"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png");
                })
                .WithThumbnailUrl($"{classlink}")
                .WithAuthor(author => {
                    author
                        .WithName("Awona")
                        .WithIconUrl(@"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png");
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
    
    }
}
