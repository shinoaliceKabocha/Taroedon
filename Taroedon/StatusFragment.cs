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

namespace Taroedon
{
    public class StatusFragment : Android.Support.V4.App.Fragment
    {
        private static List<Status> statuses;
        private static MastodonClient client;
        private static TimelineStreaming streaming;
        private static bool streaming_flg = false;
        private static StatusAdapter statusAdapter;
        private static SwipeRefreshLayout swipelayout;
        private static ListView listView;
        private BackgroundWorker mWorker = null;

        public StatusFragment() { }

        public static StatusFragment newInstance(List<Status> ls)
        {
            StatusFragment fragment = new StatusFragment();
            statuses = ls;

            return fragment;
        }

        public override void OnDestroyView()
        {
            Android.Util.Log.Debug("Taroedon", "Home OnDestroyView");
            base.OnDestroyView();
            listView = null;
            swipelayout = null;
            streaming.Stop();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.StatusListView_Fragment, container, false);

            listView = view.FindViewById<ListView>(Resource.Id.listViewStatus);
            statusAdapter = new StatusAdapter(inflater, statuses);
            listView.Adapter = statusAdapter;

            listView.ItemClick += (sender, e) =>
            {
                View view2 = View.Inflate(this.Context, Resource.Layout.StatusListView_Fragment, null);
                Status st = statuses[e.Position];
                UserAction.ListViewItemClick(st, view2);
            };
            listView.ItemLongClick += (sender, e) =>
            {
                int select = e.Position;
                var status = statuses[select];
                try
                {
                    var re_status = status.Reblog;
                    if (re_status != null) status = re_status;
                }
                catch (Exception ex) { }

                View view2 = View.Inflate(this.Context, Resource.Layout.StatusListView_Fragment, null);
                UserAction.Fav(status, view2);
            };

            //swipe refersh
            swipelayout = view.FindViewById<Android.Support.V4.Widget.SwipeRefreshLayout>(Resource.Id.swipelayout);
            swipelayout.SetColorSchemeColors(Android.Graphics.Color.Red, Android.Graphics.Color.Blue,
                Android.Graphics.Color.Green, Android.Graphics.Color.Yellow, Android.Graphics.Color.Orange);
            swipelayout.Refresh += SwipelayoutPull;
            listView.ScrollStateChanged += Listview_ScrollStateChanged;

            statuses.Clear();
            //Auth
            if (client==null)
            {
                client = UserClient.getInstance().getClient();
                UserStreamRun();
            }
            GetHomeTl();
            streaming.Start();

            return view;
        }

        /***************************************************************
        *                   Time line
        / **************************************************************/
        private async void GetHomeTl()
        {
            if (statusAdapter == null) return;
            MastodonList<Status> mstdnlist = await client.GetHomeTimeline();

            //0 follow patch
            if (mstdnlist.Count == 0) return;

            for (int i = 15; i >= 0; i--)
            {
                Status s = mstdnlist[i];
                if(!statuses.Contains(s)) statuses.Insert(0, s);
            }
            statusAdapter.NotifyDataSetChanged();
        }

        private async void UserStreamRun()
        {
            if (streaming == null)
            {
                streaming = client.GetUserStreaming();
            }

            streaming.OnUpdate += (sender, e) =>
            {
                statuses.Insert(0, e.Status);
                if(listView.FirstVisiblePosition == 0)
                {
                    while (statuses.Count >= 20)
                    {
                        statuses.RemoveAt(statuses.Count - 1);
                    }
                }
                statusAdapter.NotifyDataSetChanged();

                if(e.Status.Content.Contains("@" + UserClient.currentAccountName) && e.Status.InReplyToAccountId != null)
                {
                    string notifyStr = e.Status.Account.DisplayName +"@"+ e.Status.Account.AccountName +"さんからとぅーと";
                    string s = OtherTool.HTML_removeTag(e.Status.Content);
                    notifyStr += "\n" + s;
                    View view = View.Inflate(this.Context, Resource.Layout.StatusListView_Fragment, null);
                    UserAction.ToastWithIcon_TopFIllHorizontal_Show(notifyStr, e.Status.Account.AvatarUrl, view, ColorDatabase.REPLY);
                }

            };
        }

        /***************************************************************
         *       1                       update
         **************************************************************/
        private void SwipelayoutPull(object sender, EventArgs e)
        {
            if(mWorker != null)
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

            if (streaming == null) streaming = client.GetUserStreaming();
            streaming.Start();
            streaming_flg = true;
            GetHomeTl();
            swipelayout.Refreshing = false;
            mWorker = null;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            streaming.Stop();
            streaming_flg = false;
            statuses.Clear();
            statusAdapter.NotifyDataSetChanged();
        }

        private void Listview_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            try
            {
                if (listView.LastVisiblePosition == (listView.Count - 1))
                {
                    listView.ScrollStateChanged -= Listview_ScrollStateChanged;

                    long id = statuses[listView.Count - 1].Id;
                    GetTLdown(id);
                }
            }
            catch(Exception error)
            {
                /* do nothing */
                return;
            }
        }
        private async void GetTLdown(long under)
        {
            MastodonList<Status> mstdnlist = await client.GetHomeTimeline(under);
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




