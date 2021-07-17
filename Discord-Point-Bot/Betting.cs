using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Discord_Point_Bot
{
    public class UserBet
    {
        public SocketUser user;
        public int point;
    }

    public class Form
    {
        public string name;
        public List<UserBet> member;

        public Form()
        {
            member = new List<UserBet>();
        }

        public bool HasUser(SocketUser user)
        {
            if (member.Single(x => x.user.Id == user.Id) != null)
                return true;
            return false;
        }

        public void AddUser(SocketUser user, int point)
        {
            member.Add(new UserBet { user = user, point = point });
        }

        public void UpdateUser(SocketUser user, int point)
        {
            member.Single(x => x.user.Id == user.Id).point = point;
        }

        public void DeleteUser(SocketUser user)
        {
            member.Remove(member.Single(x => x.user.Id == user.Id));
        }
    }

    public class Event
    {
        private string title;
        private string date;
        private List<Form> forms;

        public Event()
        {
            forms = new List<Form>();
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Date 
        {
            get { return date; }
            set { date = value; }
        }

        public void AddForm(string name)
        {
            forms.Add(new Form { name = name });
        }

        public IEnumerable<Form> AllForms()
        {
            return forms.AsEnumerable();
        }

    
    }

    public class Betting
    {
        public static Betting instance = null;

        private List<Event> events;

        public Betting()
        {
            events = new List<Event>();
        }

        public static Betting Instance()
        {
            if (instance == null)
                instance = new Betting();
            return instance;
        }

        public void NewEvent(string title)
        {
            Event @event = new Event();
            @event.Title = title;
            @event.Date = DateTime.Now.ToString("MMddyyyy");
            events.Add(@event);
        }

        public IEnumerable<Event> AllEvents()
        {
            return events.AsEnumerable();
        }
    }
}
