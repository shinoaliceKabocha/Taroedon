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
using Android.Support.V4.App;
using System.Threading.Tasks;
using Mastonet;
using Mastonet.Entities;
using Android.Support.V4.Widget;
using System.ComponentModel;
using System.Threading;

namespace FlashCardPager
{
    public class StatusFragment3 : Android.Support.V4.App.Fragment
    {
        private static List<Status> statuses;
        private MastodonClient client;
        private static ListView listView;
        private TimelineStreaming streaming;
        private StatusAdapter statusAdapter;
        private static SwipeRefreshLayout swipelayout;

        public StatusFragment3() { }

        public static StatusFragment3 newInstance(List<Status> ls)
        {
            StatusFragment3 fragment = new StatusFragment3();
            statuses = ls;
            //customStatuses.Add(new CustomStatus("a", "aa", "2012/12/12"));

            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.StatusListView_Fragment, container, false);

            listView = view.FindViewById<ListView>(Resource.Id.listViewStatus);
            statusAdapter = new StatusAdapter(inflater, statuses);
            listView.Adapter = statusAdapter;
            statusAdapter.NotifyDataSetChanged();

            //イベント
            listView.ItemClick += (sender, e) =>
            {
                Status st = statuses[e.Position];
                UserAction.ListViewItemClick(st, view);
            };
            //ショートカット ふぁぼ
            listView.ItemLongClick += (sender, e) =>
            {
                int select = e.Position;
                var status = statuses[select];
                try
                {
                    //投稿がリツイートサれたものである場合は，もとの取得先を手に入れる
                    var re_status = status.Reblog;
                    if (re_status != null) status = re_status;
                }
                catch (Exception ex) { }

                UserAction.FavAsync(status, view);
            };

            //swipe refersh
            swipelayout = view.FindViewById<Android.Support.V4.Widget.SwipeRefreshLayout>(Resource.Id.swipelayout);
            swipelayout.SetColorSchemeColors(Android.Graphics.Color.Red, Android.Graphics.Color.Blue,
                Android.Graphics.Color.Green, Android.Graphics.Color.Yellow, Android.Graphics.Color.Orange);
            swipelayout.Refresh += (sender, e) =>
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync();
            };
            //swipe down
            listView.ScrollStateChanged += Listview_ScrollStateChanged;

            //Auth
            if (client == null) client = new UserClient().getClient();
            if (statuses.Count == 0)
            {
                GetPublicTl();
                PublicStreamRun();
            }

            return view;
        }

        /***************************************************************
        *                   Time line 取得関係
        / **************************************************************/
        private async Task GetPublicTl()
        {
            ////home get
            MastodonList<Status> mstdnlist = await client.GetPublicTimeline();

            for (int i = 15; i >= 0; i--)
            {
                Status s = mstdnlist[i];
                statuses.Insert(0, s);
            }
            //更新の反映
            statusAdapter.NotifyDataSetChanged();
        }

        private async Task PublicStreamRun()
        {
            if (streaming == null) streaming = client.GetPublicStreaming();
            streaming.Start();

            streaming.OnUpdate += (sender, e) =>
            {
                statuses.Insert(0, e.Status);
                statusAdapter.NotifyDataSetChanged();
            };
        }

        /***************************************************************
         *                              update
         **************************************************************/
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ////クリアを反映する
            statusAdapter.NotifyDataSetChanged();

            streaming.Start();
            GetPublicTl();
            swipelayout.Refreshing = false;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            streaming.Stop();
            statuses.Clear();

            Thread.Sleep(1000);
        }

        /***************************************************************
         *                   スクロール
         **************************************************************/
        private void Listview_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            if (listView.LastVisiblePosition == (statuses.Count - 1))
            {
                listView.ScrollStateChanged -= Listview_ScrollStateChanged;

                long id = statuses[statuses.Count - 1].Id;
                GetTLdown(id);
            }

        }
        private async Task GetTLdown(long under)
        {
            MastodonList<Status> mstdnlist = await client.GetPublicTimeline(under);
            foreach (Status s in mstdnlist)
            {
                if (statuses.Contains(s) == true) { }
                else statuses.Add(s);
            }

            statusAdapter.NotifyDataSetChanged();
            listView.ScrollStateChanged += Listview_ScrollStateChanged;
        }


    }
}
