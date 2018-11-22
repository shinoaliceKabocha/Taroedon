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
    public class StatusFragment2 : Android.Support.V4.App.Fragment
    {
        private static List<Mastonet.Entities.Notification> notifications;
        private static MastodonClient client;
        private static ListView listView;
        private NotificationAdapter statusAdapter;
        private static SwipeRefreshLayout swipelayout;


        public StatusFragment2() { }

        public static StatusFragment2 newInstance()
        {
            StatusFragment2 fragment = new StatusFragment2();
            //customStatuses = ls;

            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.StatusListView_Fragment, container, false);
            notifications = new List<Mastonet.Entities.Notification>();

            listView = view.FindViewById<ListView>(Resource.Id.listViewStatus);
            statusAdapter = new NotificationAdapter(inflater, notifications);
            listView.Adapter = statusAdapter;

            //イベント
            listView.ItemClick += (sender, e) =>
            {
                var notify = notifications[e.Position];
                if (notify.Status != null)
                {
                    Status st = notify.Status;
                    UserAction.ListViewItemClick(st, view);
                }
            };
            //ショートカット機能
            listView.ItemLongClick += (sender, e) =>
            {
                int select = e.Position;
                var notification  = notifications[select];
                if (notification.Status != null)
                {
                    //mention or mytweet
                    var status = notification.Status;
                    try
                    {
                        //投稿がリツイートサれたものである場合は，もとの取得先を手に入れる
                        var re_status = status.Reblog;
                        if (re_status != null) status = re_status;
                    }
                    catch (Exception ex) { }
                    UserAction.FavAsync(status, view);
                }
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
            if (notifications.Count == 0) GetHNotifyTl();

            return view;
        }

        /***************************************************************
        *                   Time line 取得関係
        / **************************************************************/
        private async Task GetHNotifyTl()
        {
            ////home get
            var mstdnlist = await client.GetNotifications();

            for (int i = mstdnlist.Count-1; i >= 0; i--)
            {
                notifications.Insert(0, mstdnlist[i]);

                //更新の反映 mention系はなぜか必要
                statusAdapter.NotifyDataSetChanged();

            }
            //更新の反映
            statusAdapter.NotifyDataSetChanged();
        }

        /***************************************************************
         *                              update
         **************************************************************/
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ////クリアを反映する
            statusAdapter.NotifyDataSetChanged();

            GetHNotifyTl();
            swipelayout.Refreshing = false;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            notifications.Clear();
            statusAdapter.NotifyDataSetChanged();

            //Thread.Sleep(1000);
        }

        /***************************************************************
         *                   スクロール
         **************************************************************/
        private void Listview_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            if (listView.LastVisiblePosition == (notifications.Count - 1))
            {
                listView.ScrollStateChanged -= Listview_ScrollStateChanged;

                long id = notifications[notifications.Count - 1].Id;
                GetTLdown(id);
            }

        }
        private async Task GetTLdown(long under)
        {
            MastodonList<Mastonet.Entities.Notification> mstdnlist = await client.GetNotifications(under);
            foreach (var n in mstdnlist)
            {
                if (!notifications.Contains(n)) notifications.Add(n);
            }

            statusAdapter.NotifyDataSetChanged();
            listView.ScrollStateChanged += Listview_ScrollStateChanged;
        }

    }
}
