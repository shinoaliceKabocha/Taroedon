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
using Mastonet.Entities;
using Context = Android.Content.Context;

namespace Taroedon
{
    [Activity(Label = "ConversationActivity", Theme = "@style/PostTheme")]
    public class ConversationActivity : Activity
    {
        Mastonet.MastodonClient client = UserClient.getInstance().getClient();
        List<Status> statuses = new List<Status>();
        private ListView mListView;
        private StatusAdapter mStatusAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Conversation);

            //Adjust size
            IWindowManager windowManager = (IWindowManager)this.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetRealSize(point);
            Window.SetLayout((int)(point.X * 0.98), (int)(point.Y * 0.85));

            //param
            long statusId = this.Intent.GetLongExtra("statusId", -1);
            SetConversationAsync(statusId);

            //regist
            mListView = this.FindViewById<ListView>(Resource.Id.listViewConversation);
            mListView.SetBackgroundColor(ColorDatabase.TL_BACK);
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            mStatusAdapter = new StatusAdapter(layoutInflater, statuses);
            mListView.Adapter = mStatusAdapter;
            mStatusAdapter.NotifyDataSetChanged();

            //event
            mListView.ItemClick += (sender, e) =>
            {
                Status st = statuses[e.Position];
                View view = this.FindViewById(Android.Resource.Id.Content);
                UserAction.ListViewItemClick(st, view);
            };
            //shortcut
            mListView.ItemLongClick += (sender, e) =>
            {
                int select = e.Position;
                var status = statuses[select];
                try
                {
                    //if boosted toot, get orig toot
                    var re_status = status.Reblog;
                    if (re_status != null) status = re_status;
                }
                catch (Exception ex) { }

                UserAction.Fav(status, this.FindViewById(Android.Resource.Id.Content));
            };


        }


        private async void SetConversationAsync(long id)
        {
            long _id = id;

            while (true)
            {
                var status = await client.GetStatus(_id);
                statuses.Add(status);

                _id = status.InReplyToId.GetValueOrDefault(-1);
                if (_id < 0) break;
            }

            mStatusAdapter.NotifyDataSetChanged();

        }
    }
}