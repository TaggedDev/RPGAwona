using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bot.Services;
using Microsoft.Data.Sqlite;

namespace Bot
{
    class Program
    {
        static void ExecuteSQL(string cmd)
        {
            using (var connection = new SqliteConnection("Data Source=awona.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand
                {
                    Connection = connection,
                    CommandText = cmd
                };
                command.ExecuteNonQuery();
            }
        }

        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug); // Defines what kind of information should be logged (e.g. Debug, Information, Warning, Critical) adjust this to your liking
                })
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose, // Defines what kind of information should be logged from the API (e.g. Verbose, Info, Warning, Critical) adjust this to your liking
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 10000,
                    };

                    config.Token = context.Configuration["token"];
                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Verbose;
                    config.DefaultRunMode = RunMode.Sync;
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<CommandHandler>();
                })
                .UseConsoleLifetime();

            //ExecuteSQL("DROP TABLE users");
            ExecuteSQL("DROP TABLE duel");
            //ExecuteSQL($"DELETE FROM duel WHERE player1id = {327391902167203841}");
            ExecuteSQL("CREATE TABLE IF NOT EXISTS users (discord_id INTEGER NOT NULL UNIQUE, level INTEGER NOT NULL, money INTEGER, exp INTEGER, archetype VARCHAR(100) NOT NULL, type VARCHAR(100) NOT NULL, weapon INTEGER, hat INTEGER, body INTEGER, legs INTEGER, boots INTEGER, gloves INTEGER)");
            ExecuteSQL("CREATE TABLE IF NOT EXISTS duel (categoryname VARCHAR(100), player1name VARCHAR(100), player2name VARCHAR(100), player1id INTEGER NOT NULL UNIQUE, player2id INTEGER NOT NULL UNIQUE, channel1id INTEGER NOT NULL, channel2id INTEGER NOT NULL, player1move VARCHAR(100), player2move VARHCAR(100), player1health INTEGER, player2health INTEGER, player1surrender BOOLEAN, player2surrender BOOLEAN)");
            //ExecuteSQL("CREATE TABLE IF NOT EXISTS inventory (discord_id INTEGER, level INTEGER, archetype VARCHAR(100), type VARCHAR(100), weapon INTEGER, hat INTEGER, body INTEGER, legs INTEGER, boots INTEGER, gloves INTEGER)");
            


            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}