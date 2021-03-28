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
            if (archetype == null)
                await ReplyAsync(":x: Выберите один из четырёх археклассов (Faith, Japan, Melee, Magic)");
            if (!provider.UserAlreadyCreated(Convert.ToString(Context.User.Id)))
            {
                int level;
                string type, discord_id;
                archetype = archetype.ToLower();

                if (!(archetype.Equals("faith") || archetype.Equals("japan") || archetype.Equals("melee") || archetype.Equals("magic")))
                    await ReplyAsync(":x: Выберите один из четырёх археклассов (Faith, Japan, Melee, Magic)");
                else
                {
                    type = archetype switch
                    {
                        ("faith") => "Acolyte",
                        ("japan") => "Asigaru",
                        ("magic") => "Alchemist",
                        ("melee") => "Komtur",
                        _ => "Komtur",
                    };
                    discord_id = Convert.ToString(Context.User.Id);
                    level = 1;
                    provider.ExecuteSQL($"INSERT INTO users (discord_id, level, money, exp, archetype, type) VALUES ('{discord_id}', '{level}', 0, 0, '{archetype}', '{type}')");
                    IRole role = archetype switch
                    {
                        ("faith") => Context.Guild.GetRole(825802241277165598),
                        ("japan") => Context.Guild.GetRole(825802240609484880),
                        ("magic") => Context.Guild.GetRole(825802241616510977),
                        ("melee") => Context.Guild.GetRole(825802244825284688),
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
                    ("japan") => Context.Guild.GetRole(825802240609484880),
                    ("magic") => Context.Guild.GetRole(825802241616510977),
                    ("melee") => Context.Guild.GetRole(825802244825284688),
                    _ => Context.Guild.GetRole(825802244825284688),
                };
                await (Context.User as IGuildUser).RemoveRoleAsync(role);

                command = $"DELETE FROM users WHERE discord_id = {id}";
                provider.ExecuteSQL(command);
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

            type = Convert.ToString(provider.GetFieldAwonaByID("type", Convert.ToString(Context.User.Id), "discord_id", "users"));
            archetype = Convert.ToString(provider.GetFieldAwonaByID("archetype", Convert.ToString(Context.User.Id), "discord_id", "users"));
            level = Convert.ToInt32(provider.GetFieldAwonaByID("level", Convert.ToString(Context.User.Id), "discord_id", "users"));
            char firstLetter = archetype[0];
            archetype = char.ToUpper(firstLetter) + archetype[1..];

            uint color = archetype switch
            {
                ("Faith") => 0xE7EB2D,
                ("Japan") => 0x2DEB8C,
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

            Archetype character = subcommand.CreateClass(type, Context.User as SocketGuildUser);
            damage = character.Damage;
            health = character.Health;
            armor = character.Defence;
            luck = character.Luck;
            agility = character.Dodge;

            string race = Convert.ToString(provider.GetFieldAwonaByID("type", Convert.ToString(character.Id), "discord_id", "users"));
            string awonalink = @"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png";
            string classlink = race switch
            {
                ("Acolyte") => @"https://cdn.discordapp.com/attachments/823526896582656031/825726887296106517/Acolyte.png",
                ("Asigaru") => @"https://cdn.discordapp.com/attachments/823526896582656031/825726889272148039/Asigaru.png",
                ("Alchemist") => @"https://cdn.discordapp.com/attachments/823526896582656031/825727004921692180/Alchemist.png",
                ("Komtur") => @"https://cdn.discordapp.com/attachments/823526896582656031/825726890609999902/Komtur.png",
                _ => awonalink,
            };
            var builder = new EmbedBuilder()
                .WithTitle($"{name}")
                .WithDescription($"**Окно персонажа**\n\nКласс: {type}\nАрхетип: {archetype}\nУровень: {level}\n")
                //.WithUrl("https://discordapp.com")
                .WithColor(new Color(color))
                .WithTimestamp(DateTimeOffset.FromUnixTimeMilliseconds(1616356046810))
                .WithFooter(footer => {
                    footer
                        .WithText("Awona RPG | Discord | Character window")
                        .WithIconUrl($"{awonalink}");
                })
                .WithThumbnailUrl($"{classlink}")
                .WithAuthor(author => {
                    author
                        .WithName("Awona")
                        .WithIconUrl($"{awonalink}");
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
