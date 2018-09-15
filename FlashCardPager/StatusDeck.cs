using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mastonet;
using Mastonet.Entities;

namespace FlashCardPager
{
    //public class CustomStatus 
    //{
    //    public string username, content, createdAt;
    //    public string UserName { get { return username; } }
    //    public string Content { get { return content; } }
    //    public string CreatedAt { get { return createdAt; } }

    //    public CustomStatus(string u, string con, string cre)
    //    {
    //        this.username = u;
    //        this.content = con;
    //        this.createdAt = cre;
    //    }
    //}

    public class StatusDeck
    {
        static List<Status>[] statuses_vs = new List<Status>[]
        {
            new List<Status>(){  },
            new List<Status>(){ },
            new List<Status>(){  }
        };

        private List<Status>[] allTimeLineStatuses;

        public StatusDeck() { allTimeLineStatuses = statuses_vs; }

        public List<Status> this[int i] { get { return allTimeLineStatuses[i]; } }

        public int NumTimeLines { get { return allTimeLineStatuses.Length; } }

    }
}
