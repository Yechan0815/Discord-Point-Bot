using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Discord_Point_Bot
{
    public class BetUser
    {
        public string user;
        public int point;
    }

    public class Form
    {
        public string form;
        public List<BetUser> users;

        public Form()
        {
            users = new List<BetUser>();
        }
    }

    public class Event
    {
        public string title;
        public string author;
        public string date;
        public List<Form> forms;

        public Event()
        {
            forms = new List<Form>();
        }

        public string ToJson()
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);

            using (JsonWriter writer = new JsonTextWriter(stringWriter))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("title");
                writer.WriteValue(title);
                writer.WritePropertyName("date");
                writer.WriteValue(date);
                writer.WritePropertyName("author");
                writer.WriteValue(author);
                writer.WritePropertyName("events");
                writer.WriteStartArray();
                foreach (Form e in forms)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("form");
                    writer.WriteValue(e.form);
                    writer.WritePropertyName("users");
                    writer.WriteStartArray();
                    foreach (BetUser u in e.users)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("user");
                        writer.WriteValue(u.user);
                        writer.WritePropertyName("point");
                        writer.WriteValue(u.point);
                        writer.WriteEndObject();
                    }
                    writer.WriteEnd();
                    writer.WriteEndObject();
                }
                writer.WriteEnd();
                writer.WriteEndObject();
            }
            return stringBuilder.ToString();
        }
    }

    public class Betting
    {
        public static Betting instance = null;

        private SQLite sqlite;

        public static Betting Instance()
        {
            if (instance == null)
                instance = new Betting();
            return instance;
        }

        public static Event Parse(string title, string author, string events)
        {
            Event e = new Event();
            string[] eventArr = events.Split(',');

            e.title = title;
            e.author = author;
            e.date = DateTime.Now.ToString("MMddyyyy");
            foreach (string form in eventArr)
            {
                if (form != "")
                    e.forms.Add(new Form { form = form.Trim() });
            }
            return e;
        }

        public Betting()
        {
            sqlite = SQLite.Instance();
        }

        public void NewEvent(string title, string data)
        {
            sqlite.BetTableInsert(title, data);
        }

        public List<Event> AllEvents()
        {
            return (sqlite.GetBetList());
        }
    }
}
