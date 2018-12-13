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
    public class StatusFragment : Android.Support.V4.App.Fragment
    {
        private static List<Status> statuses;
        private static MastodonClient client;
        private static TimelineStreaming streaming;
        private static StatusAdapter statusAdapter;
        private static SwipeRefreshLayout swipelayout;
        private static ListView listView;

        public StatusFragment() { }

        public static StatusFragment newInstance(List<Status> ls)
        {
            StatusFragment fragment = new StatusFragment();
            statuses = ls;

            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.StatusListView_Fragment, container, false);

            listView = view.FindViewById<ListView>(Resource.Id.listViewStatus);
            statusAdapter = new StatusAdapter(inflater, statuses);
            listView.Adapter = statusAdapter;

            //イベント
            listView.ItemClick += (sender, e) =>
            {
                View view2 = View.Inflate(this.Context, Resource.Layout.StatusListView_Fragment, null);
                Status st = statuses[e.Position];
                UserAction.ListViewItemClick(st, view2);
            };
            //ショートカット
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

                View view2 = View.Inflate(this.Context, Resource.Layout.StatusListView_Fragment, null);
                UserAction.Fav(status, view2);
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
            if (client==null) client = new UserClient().getClient();
            if (statuses.Count == 0)
            {
                GetHomeTl();
                UserStreamRun();
            }

            return view;
        }

        /***************************************************************
        *                   Time line 取得関係
        / **************************************************************/
        private async Task GetHomeTl()
        {
            ////home get
            MastodonList<Status> mstdnlist = await client.GetHomeTimeline();

            for (int i = 15; i >= 0; i--)
            {
                Status s = mstdnlist[i];
                if(!statuses.Contains(s)) statuses.Insert(0, s);
            }
            //更新の反映
            statusAdapter.NotifyDataSetChanged();
        }

        private async Task UserStreamRun()
        {
            if(streaming==null) streaming = client.GetUserStreaming();
            streaming.Start();

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

                //通知
                if(e.Status.Content.Contains("@" + UserClient.currentAccountName) && e.Status.InReplyToAccountId != null)
                {
                    string notifyStr = e.Status.Account.DisplayName +"@"+ e.Status.Account.AccountName +"さんからとぅーと";
                    string s = OtherTool.HTML_removeTag(e.Status.Content);
                    notifyStr += "\n" + s;
                    //UserAction.Toast_TopFIllHorizontal_Show(notifyStr, this.Context, new Android.Graphics.Color(255, 131, 109));
                    View view = View.Inflate(this.Context, Resource.Layout.StatusListView_Fragment, null);
                    UserAction.ToastWithIcon_TopFIllHorizontal_Show(notifyStr, e.Status.Account.AvatarUrl, view, new Android.Graphics.Color(255, 131, 109));
                }

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
            GetHomeTl();
            swipelayout.Refreshing = false;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            streaming.Stop();
            statuses.Clear();
            statusAdapter.NotifyDataSetChanged();
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




