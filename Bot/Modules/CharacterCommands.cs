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
using System.Linq;

namespace Bot.Modules
{
    public class CharacterCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<CharacterCommands> _logger;
        
        public CharacterCommands(ILogger<CharacterCommands> logger)
            => _logger = logger;

        static Provider provider = new Provider();
        static Subcommand subcommand = new Subcommand();


        [Command("create")]
        [Alias("create_character")]
        public async Task CreateCharacter(string archetype)
        {
            if (!provider.UserAlreadyCreated(Convert.ToString(Context.User.Id)))
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

                    provider.ExecuteSQL($"INSERT INTO users (discord_id, level, archetype, type) VALUES ('{discord_id}', '{level}', '{archetype}', '{type}')");
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
            if (provider.UserAlreadyCreated(id))
            { 
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

            Archetype character = subcommand.CreateClass(type, Context.User as SocketGuildUser);
            damage = character.Damage;
            health = character.Health;
            armor = character.Defence;
            luck = character.Luck;
            agility = character.Dodge;


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
    
    }
}
