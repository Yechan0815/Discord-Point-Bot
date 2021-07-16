using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord_Point_Bot
{
    class Program
    {
        private DiscordSocketClient _client;

        public static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            await _client.LoginAsync(TokenType.Bot, "");
        }
    }
}
