using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;


namespace Bot.Modules
{
    public class PublicCommands : ModuleBase<SocketCommandContext>
    {

        [Command("help")]
        [Alias("hilfe", "faq", "howtostart", "howto", "start", "hts")]
        public async Task HelpCommand()
        {
            var builder = new EmbedBuilder()
                .WithTitle($"**Помощь:** о сервере и боте, как начать играть?")
                .WithColor(0xffbb37)
                .WithFooter(footer =>
                {
                    footer
                        .WithText("RPG Awona | Help command");
                })
                .WithThumbnailUrl("https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png")
                .WithAuthor(author =>
                {
                    author
                        .WithName("RPG Awona")
                        .WithIconUrl(@"https://cdn.discordapp.com/attachments/823526896582656031/825726892946227250/wowo.png");
                })
                .AddField("О сервере и боте:", $"Awona - это новая MMORPG в Discord, основной идеей которой являются дуэли между двумя игроками и получение репутации и получение рупия (валюта)") 
                .AddField("Как начать игру?", $"Для начала вам нужно создать персонажа, сделать это можно с помощью команды !create (подробнее ниже), а чтобы начать бой используйте !challenge (подробнее тоже снизу)")
                .AddField("Создание персонажа", $"Чтобы создать персонажа нужно написать команду !create и выбрать один из четырёх классов (Magic - Магия и алхимия, Faith - Вера, Serenity - Безмятежность, Army - Орден тамплиеров). **Пример:**\n!create Magic")
                .AddField("Изменить персонажа", $"Иметь больше одного персонажа нельзя, для создания нового персонажа нужно удалить предыдущего командой !delete.")
                .AddField("Начать бой", $"Чтобы начать бой нужно написать !challenge и упомянуть игрока. **Пример**:\n!challenge <@327391902167203841>\nАлиасы команды: !vs !versus !fight !destroy !allez !los")
                .AddField("О бое", $"Дуэль состоит из ходов, на каждый из которых у обоих игроков отводят __10 секунд.__ За это время игрок может прописать одну из 4х (пока что) команд:\n\n!attack - Атака, которая наносит урон противнику, зависит от брони противника\n!defend - Вы загораживаетесь щитом, который закрывает вас от удара, однако некоторая часть урона проходит по вам\n!Ability - спецспособность класса\n!surrender - Сдайтесь и бегите прочь, вы проиграли битву!\n")
                .AddField("Экономика", $"*В разработке*")
                .AddField("О предметах", $"*В разработке*");

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(":white_check_mark: Информация у вас в личных сообщениях");
            await Context.User.SendMessageAsync(null, false, embed, null, null);
        }

    }
}
