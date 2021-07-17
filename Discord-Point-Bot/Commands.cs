using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Point_Bot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task Test()
        {
            await Context.Channel.SendMessageAsync("test");
        }

        [Command("user")]
        public async Task UserData(SocketUser socketUser = null)
        {
            SQLite sqlite = SQLite.Instance();
            User user;

            if (socketUser == null)
                user = sqlite.GetUser(Context.User.Id.ToString());
            else
                user = sqlite.GetUser(socketUser.Id.ToString());
            string attend = "아직 기록이 없습니다";
            if (user.Attendance != "null")
                attend = $"{user.Attendance.Substring(0, 2)}/{user.Attendance.Substring(2, 2)}";

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("기록")
                .AddField("미션", $"{user.Donate} <<") 
                .AddField("Point", $"{user.Point}")
                .AddField("마지막 출석일", $"{attend}")
                .WithAuthor(socketUser == null ? Context.User : socketUser)
                .WithColor(169, 211, 219)
                .WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("daily")]
        public async Task DailyCheck()
        {
            SQLite sqlite = SQLite.Instance();
            User user = sqlite.GetUser(Context.User.Id.ToString());
            if (user.Attendance == DateTime.Now.ToString("MMddyyyy"))
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} 오늘은 이미 출석 했습니다!");
                return;
            }
            sqlite.UserUpdate(Context.User.Id.ToString(), point: user.Point + 10);
            sqlite.UserUpdate(Context.User.Id.ToString(), attendance: DateTime.Now.ToString("MMddyyyy"));
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} 출석 체크 되었습니다!");
        }

        [Command("bet")]
        public async Task Bet()
        {
            Betting betting = Betting.Instance();

            betting.NewEvent("new event A");
            betting.NewEvent("new event C");
            betting.NewEvent("new event B");
            string a = "";
            foreach (Event @event in betting.AllEvents())
            {
                a += $"{@event.Title} {@event.Date}\n";
            }
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(":exclamation: 베팅")
                .WithDescription(a)
                .WithColor(169, 211, 219)
                .WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
