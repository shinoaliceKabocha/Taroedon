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
using Notification = Mastonet.Entities.Notification;

namespace Taroedon
{
    public class StatusFragment2 : Android.Support.V4.App.Fragment
    {
        private static List<Mastonet.Entities.Notification> notifications;
        private static MastodonClient client;
        private static ListView listView;
        private NotificationAdapter statusAdapter;
        private static SwipeRefreshLayout swipelayout;
        private BackgroundWorker mWorker = null;


        public StatusFragment2() { }

        public static StatusFragment2 newInstance()
        {
            StatusFragment2 fragment = new StatusFragment2();
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.StatusListView_Fragment, container, false);
            notifications = new List<Mastonet.Entities.Notification>();

            listView = view.FindViewById<ListView>(Resource.Id.listViewStatus);
            statusAdapter = new NotificationAdapter(inflater, notifications);
            listView.Adapter = statusAdapter;

            //event
            listView.ItemClick += (sender, e) =>
            {
                View view2 = View.Inflate(this.Context, Resource.Layout.StatusListView_Fragment, null);

                var notify = notifications[e.Position];
                if (notify.Status != null)
                {
                    Status st = notify.Status;
                    UserAction.ListViewItemClick(st, view2);
                }
                else
                {
                    UserAction.Profile(notify.Account, view2.Context);
                }
            };
            //shortcut
            listView.ItemLongClick += (sender, e) =>
            {
                View view2 = View.Inflate(this.Context, Resource.Layout.StatusListView_Fragment, null);

                int select = e.Position;
                var notification  = notifications[select];
                if (notification.Status != null)
                {
                    //mention or mytweet
                    var status = notification.Status;
                    try
                    {
                        //if boosted toot, get orig toot
                        var re_status = status.Reblog;
                        if (re_status != null) status = re_status;
                    }
                    catch (Exception ex) { }
                    UserAction.Fav(status, view2);
                }
            };

            //swipe refersh
            swipelayout = view.FindViewById<Android.Support.V4.Widget.SwipeRefreshLayout>(Resource.Id.swipelayout);
            swipelayout.SetColorSchemeColors(Android.Graphics.Color.Red, Android.Graphics.Color.Blue,
                Android.Graphics.Color.Green, Android.Graphics.Color.Yellow, Android.Graphics.Color.Orange);
            swipelayout.Refresh += SwipelayoutPull;
            //swipe down
            listView.ScrollStateChanged += Listview_ScrollStateChanged;

            //Auth
            if (client == null) client = UserClient.getInstance().getClient();
            if (notifications.Count == 0) GetHNotifyTl();

            return view;
        }

        /***************************************************************
        *                   Time line
        / **************************************************************/
        private async void GetHNotifyTl()
        {
            var mstdnlist = new MastodonList<Notification>();
            mstdnlist = await client.GetNotifications();
            //0 follow
            if (mstdnlist.Count == 0) return;

            for (int i = mstdnlist.Count-1; i >= 0; i--)
            {
                notifications.Insert(0, mstdnlist[i]);
                statusAdapter.NotifyDataSetChanged();

            }
            statusAdapter.NotifyDataSetChanged();
        }

        /***************************************************************
         *                              update
         **************************************************************/
        private void SwipelayoutPull(object sender, EventArgs e)
        {
            if (mWorker != null)
            {
                swipelayout.Refreshing = false;
            }
            else
            {
                mWorker = new BackgroundWorker();
                mWorker.DoWork += Worker_DoWork;
                mWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                mWorker.RunWorkerAsync();
            }
        }


        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            statusAdapter.NotifyDataSetChanged();

            GetHNotifyTl();
            swipelayout.Refreshing = false;
            mWorker = null;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            notifications.Clear();
            statusAdapter.NotifyDataSetChanged();

        }

        /***************************************************************
         *                   scrool
         **************************************************************/
        private void Listview_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            try
            {
                if (listView.LastVisiblePosition == (listView.Count - 1))
                {
                    listView.ScrollStateChanged -= Listview_ScrollStateChanged;

                    long id = notifications[listView.Count - 1].Id;
                    GetTLdown(id);
                }
            }
            catch (Exception error)
            {
                /* do nothing */
                return;
            }
        }
        private async void GetTLdown(long under)
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
