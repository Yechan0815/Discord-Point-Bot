using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord_Point_Bot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;

        public static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            SQLite.Instance();
            Betting.Instance();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Interrupt);

            await _client.LoginAsync(TokenType.Bot, JObject.Parse(File.ReadAllText(@"config.json"))["Token"].ToString());
            await _client.StartAsync();

            _client.Ready += () =>
            {
                Console.WriteLine($"Logged in as {_client.CurrentUser}");
                return Task.CompletedTask;
            };

            _client.MessageReceived += HandleCommandAsync;
            
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            int argPos = 0;

            if (message == null)
                return;
            if (!(message.HasStringPrefix("y ", ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, null);
        }

        private async void Interrupt(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Interrupt");
            SQLite.free();
            await _client.StopAsync();
            Environment.Exit(0);
        }
    }
}
