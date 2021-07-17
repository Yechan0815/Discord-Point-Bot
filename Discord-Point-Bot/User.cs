using System;
using System.Collections.Generic;
using System.Text;

namespace Discord_Point_Bot
{
    public class User
    {
        private string donate;
        private string attendance;
        private int point;

        public string Donate
        {
            get { return donate;  }
            set { donate = value; }
        }

        public string Attendance
        {
            get { return attendance;  }
            set { attendance = value; }
        }

        public int Point
        {
            get { return point;  }
            set { point = value; }
        }
    }
}
