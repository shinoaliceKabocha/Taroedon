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

            //�C�x���g
            listView.ItemClick += (sender, e) =>
            {
                Status st = statuses[e.Position];
                UserAction.ListViewItemClick(st, view);
            };
            //�V���[�g�J�b�g
            listView.ItemLongClick += (sender, e) =>
            {
                int select = e.Position;
                var status = statuses[select];
                try
                {
                    //���e�����c�C�[�g�T�ꂽ���̂ł���ꍇ�́C���Ƃ̎擾�����ɓ����
                    var re_status = status.Reblog;
                    if (re_status != null) status = re_status;
                }
                catch (Exception ex) { }

                UserAction.Fav( status, view );
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
        *                   Time line �擾�֌W
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
            //�X�V�̔��f
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

                //�ʒm
                if(e.Status.Content.Contains("@" + UserClient.currentAccountName) && e.Status.InReplyToAccountId != null)
                {
                    string notifyStr = e.Status.Account.DisplayName +"@"+ e.Status.Account.AccountName +"���񂩂�Ƃ��[��";
                    UserAction.Toast_TopFIllHorizontal_Show(notifyStr, this.Context, UserAction.COLOR_INFO);
                }

            };
        }

        /***************************************************************
         *                              update
         **************************************************************/
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ////�N���A�𔽉f����
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
         *                   �X�N���[��
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






///*
//�p�~���� �I���W�i���̃N���X�Ɗ֐�
//public class StatusFragment : Android.Support.V4.App.Fragment
//{
//private static string USER_ = "user";
//private static string CONTENT_ = "content";
//private static string CREATEDAT_ = "createdat";

//public StatusFragment() { }

//public static StatusFragment newInstance(List<CustomStatus> ls)
//{
//// Instantiate the fragment class:
//StatusFragment fragment = new StatusFragment();

//// Pass the question and answer to the fragment:
//Bundle args = new Bundle();

///*
//displayname(���{��),   status.AccountName�i���̂�j
//relog?
//createdat
//�T���l�C��URL
//�R���e���g�C���f�B�A�iList<String>�`���ɂȂ�̂őΉ����K�v�j
//Toot ID
//*/

//List<string> userlist = new List<string>();
//List<string> contentlist = new List<string>();
//List<string> createdatlist = new List<string>();

//foreach(CustomStatus cs in ls)
//{
//userlist.Add(cs.username);
//contentlist.Add(cs.content);
//createdatlist.Add(cs.createdAt);
//}
//args.PutStringArrayList(USER_, userlist);
//args.PutStringArrayList(CONTENT_, contentlist);
//args.PutStringArrayList(CREATEDAT_, createdatlist);

//fragment.Arguments = args;
//return fragment;
//}

//public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
//{
//List<CustomStatus> _customStatuses = new List<CustomStatus>();
//var userlist = Arguments.GetStringArrayList(USER_);
//var contentlist = Arguments.GetStringArrayList(CONTENT_);
//var createdatlist = Arguments.GetStringArrayList(CREATEDAT_);

//try
//{
//for (int i = 0; i < userlist.Count; i++)
//{
//CustomStatus customStatus = new CustomStatus(userlist[i], contentlist[i], createdatlist[i]);
//_customStatuses.Add(customStatus);
//}
//}
//catch (Exception) { }

//// Inflate this fragment from the "flashcard_layout":
//View view = inflater.Inflate(Resource.Layout.StatusListView_Fragment, container, false);
//var layout = inflater.Inflate(Resource.Layout.)

//ListView listView = view.FindViewById<ListView>(Resource.Id.listViewStatus);
//StatusAdapter statusAdapter = new StatusAdapter(inflater, customStatuses);
//listView.Adapter = statusAdapter;


//return view;
//}


