using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Point_Bot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
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

        [Command("betlist")]
        public async Task BetList()
        {
            EmbedBuilder embed = new EmbedBuilder();
            Betting betting = Betting.Instance();
            SocketUser author = null;
            int count = 0;

            List<Event> events = betting.AllEvents();
            if (events.Count == 0)
                embed.WithDescription("목록이 비어있습니다!");
            for (int i = 0; i < events.Count; i++)
            {
                count = 0;
                author = User.FindUserAsId(Context.Guild.Users, events[i].author);
                foreach (var form in events[i].forms)
                    count += form.users.Count;
                embed.AddField(
                    $"`{(char)('A' + i)}.` __{events[i].title}__ ({(author == null ? "알 수 없음" : author.Username)})",
                    $"총 {count}명이 참여 중입니다!\n᲼"

                    );
            }
            embed.WithTitle(":exclamation: 베팅")
                .WithColor(169, 211, 219)
                .WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("betlist")]
        public async Task BetListDetail(char target)
        {
            EmbedBuilder embed = new EmbedBuilder();
            Betting betting = Betting.Instance();
            SocketUser author = null;
            int index = (int)(target - 'A');
            int point = 0;
            string userlist = "";

            List<Event> events = betting.AllEvents();
            if (index < 0 || events.Count <= index)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} 조회할 대상이 없습니다!");
                return;
            }

            embed.WithTitle(events[index].title);
            author = User.FindUserAsId(Context.Guild.Users, events[index].author);
            if (author != null)
                embed.WithAuthor(author); 
            for (int i = 0; i < events[index].forms.Count; i++)
            {
                userlist = "";
                point = 0;
                for (int j = 0; j < events[index].forms[i].users.Count; j++)
                {
                    author = User.FindUserAsId(Context.Guild.Users, events[index].forms[i].users[j].user);
                    userlist += $"{(author == null ? "Unknown" : author.Username)} => {events[index].forms[i].users[j].point} Point\n";
                    point += events[index].forms[i].users[j].point;
                }
                if (events[index].forms[i].users.Count == 0)
                    userlist = "참여자가 없습니다!";
                embed.AddField(
                    $"`{(char)('A' + i)}.` __{events[index].forms[i].form}__", $"{point} Point\n```{userlist}```"
                    );
            }
            embed.WithColor(169, 211, 219)
                .WithCurrentTimestamp();
            
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("newbet")]
        public async Task NewBet(string title, [Remainder]string events)
        {
            SQLite sqlite = SQLite.Instance();
            Event e = Betting.Parse(title, Context.User.Id.ToString(), events);

            if (e.forms.Count < 2)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} 항목이 적어도 두 개 이상 필요합니다!");
                return;
            }
            string description = "새 종목을 개설했습니다!\n\n```";
            for (int i = 0; i < e.forms.Count; i++)
            {
                description += $"{(char)('A' + i)}. {e.forms[i].form}\n";
            }
            description += "```";
            sqlite.BetTableInsert(e.title, e.ToJson());
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(Context.User)
                .WithTitle(e.title)
                .WithDescription(description)
                .WithColor(169, 211, 219)
                .WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("bet")]
        public async Task Bet(char CEindex, char CFindex, int point)
        {
            SQLite sqlite = SQLite.Instance();
            Betting betting = Betting.Instance();
            User user = null;
            int Eindex = (int)(CEindex - 'A');
            int Findex = (int)(CFindex - 'A');

            List<Event> events = betting.AllEvents();
            if (events.Count <= Eindex || events[Eindex].forms.Count <= Findex)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} 조회할 대상이 잘못되었습니다!");
                return;
            }
            user = sqlite.GetUser(Context.User.Id.ToString());
            if (!(0 < point && point <= user.Point))
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} {(point > user.Point ? "포인트가 부족합니다!" : "베팅할 포인트가 잘못되었습니다")}!");
                return;
            }
            if (Betting.IsAlreadyIn(events[Eindex], Context.User.Id.ToString()))
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} 이미 참여 중입니다!");
                return;
            }
            events[Eindex].forms[Findex].users.Add(new BetUser {
                user = Context.User.Id.ToString(),
                point = point
            });
            sqlite.BetUpdate(events[Eindex].date, events[Eindex].ToJson());
            sqlite.UserUpdate(Context.User.Id.ToString(), point: user.Point - point);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor(Context.User)
                .WithTitle(":bangbang: 베팅")
                .WithDescription($"**{events[Eindex].title}**\n{events[Eindex].forms[Findex].form}에 {point} Point를 걸었습니다!")
                .WithColor(169, 211, 219)
                .WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("finish")]
        public async Task finishBet(char CEindex, char CFindex)
        {
            Betting betting = Betting.Instance();
            int Eindex = (int)(CEindex - 'A');
            int Findex = (int)(CFindex - 'A');

            List<Event> events = betting.AllEvents();
            if (events.Count <= Eindex || events[Eindex].forms.Count <= Findex)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} 대상이 잘못되었습니다!");
                return;
            }
            if (events[Eindex].author != Context.User.Id.ToString())
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} 개설자만 종료할 수 있습니다!");
                return;
            }
            string result = events[Eindex].Finish(Context, Findex);
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(":white_check_mark: 지급")
                .WithDescription(result)
                .WithColor(169, 211, 219)
                .WithCurrentTimestamp();
            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Command("donation")]
        public async Task donationList(SocketUser user, int point)
        {
            
        }

        [Command("point")]
        public async Task _point_add(SocketUser user, int point)
        {
            SQLite sqlite = SQLite.Instance();
            if (Context.User.Id.ToString() == "691888177262100563")
            {
                User u = sqlite.GetUser(Context.User.Id.ToString());
                sqlite.UserUpdate(user.Id.ToString(), point: u.Point + point);
                await Context.Channel.SendMessageAsync($"Point {point}");
            }
        }


    }
}
