using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FlashCardPager
{
    public static class TLAction
    {
        ///***************************************************************
        //*                   スクロール
        //**************************************************************/
        //public void Listview_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        //{
        //    if (listView.LastVisiblePosition == (statuses.Count - 1))
        //    {
        //        listView.ScrollStateChanged -= Listview_ScrollStateChanged;

        //        long id = statuses[statuses.Count - 1].Id;
        //        GetTLdown(id);
        //    }

        //}
        //public static async Task GetTLdown(long under)
        //{
        //    MastodonList<Status> mstdnlist = await client.GetHomeTimeline(under);
        //    foreach (Status s in mstdnlist)
        //    {
        //        if (statuses.Contains(s) == true) { }
        //        else statuses.Add(s);
        //    }

        //    statusAdapter.NotifyDataSetChanged();
        //    listView.ScrollStateChanged += Listview_ScrollStateChanged;
        //}

    }
}