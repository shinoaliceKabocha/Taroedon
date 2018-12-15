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
