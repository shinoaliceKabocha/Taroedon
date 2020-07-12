using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Mastonet.Entities;

namespace Taroedon
{
    [Activity(Label = "DialogActivity", Theme = "@style/PostTheme")]
    public class DialogActivity : Activity
    {
        private static Bitmap bitmap_reply, bitmap_fav, bitmap_boost, bitmap_user, bitmap_link, bitmap_conversation;
        string SHOW_THE_CONVERSATION = "会話を表示";


        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (bitmap_reply == null) bitmap_reply = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.reply);
            if (bitmap_fav == null) bitmap_fav = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.fav);
            if (bitmap_boost == null) bitmap_boost = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.boost);
            if (bitmap_user == null) bitmap_user = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.person);
            if (bitmap_link == null) bitmap_link = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.link);
            if (bitmap_conversation == null) bitmap_conversation = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.comment);


            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Dialog);
            //Adjust size
            IWindowManager windowManager = (IWindowManager)this.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetRealSize(point);
            Window.SetLayout((int)(point.X * 0.85), (int)(point.Y * 0.6));
            Window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(ColorDatabase.TL_BACK));

            //data intent_get
            var status = IntentExtensionStatus.GetExtra<Status>(this.Intent, "status");
            try
            {
                var re_status = status.Reblog;
                if (re_status != null) status = re_status;
            }
            catch (Exception ex) { }



            var avater_url = status.Account.StaticAvatarUrl;
            var username_text = status.Account.UserName;
            var content_text = status.Content;
            var commnds = Intent.GetStringArrayListExtra(UserAction.KEY_COMMAND);
            List<string> itemlist = commnds.ToList<string>();

            var avatar = FindViewById<ImageView>(Resource.Id.dlg_imageViewAvatar);
            var username = FindViewById<TextView>(Resource.Id.dlg_textView_DisplayName);
            var content = FindViewById<TextView>(Resource.Id.dlg_textView_content);
            var listview = FindViewById<ListView>(Resource.Id.dlg_listView_command);

            ImageProvider imageProvider = new ImageProvider();

            //avatar
            imageProvider.ImageIconSetAsync(avater_url, avatar);

            var statusController = new StatusController(status);
            //username
            username.Text = "";
            statusController.SetStatusToTextView_forProfile(username, Color.White, this);

            //content
            content.Text = "";
            statusController.SetStatusToTextView(content, Color.White, this);
            //content.SetText(Html.FromHtml(content_text), TextView.BufferType.Normal);

            //time
            //var time = FindViewById<TextView>(Resource.Id.dlg_textView_CreteTime);
            //time.Text = time_text;

            //string[] command
            listview.SetBackgroundColor(ColorDatabase.TL_BACK);
            List<EmojiItem> commands_withIcon_list = new List<EmojiItem>();
            for(int i=0; i<itemlist.Count; i++)
            {
                //Android.Util.Log.Info("HOGE", itemlist[i]);

                Bitmap bitmap = null;
                switch (i)
                {
                    case 0:
                        bitmap = bitmap_reply;
                        break;
                    case 1:
                        bitmap = bitmap_fav;
                        break;
                    case 2:
                        bitmap = bitmap_boost;
                        break;
                    case 3:
                        bitmap = bitmap_user;
                        break;
                    default:
                        if (!itemlist[i].Contains("http")) bitmap = bitmap_conversation;
                        else bitmap = bitmap_link;
                        break;
                }
                EmojiItem item = new EmojiItem(itemlist[i], bitmap);
                commands_withIcon_list.Add(item);
            }

            CustomListAdapter customListAdapter = new CustomListAdapter(this, commands_withIcon_list);
            listview.Adapter = customListAdapter;
            customListAdapter.NotifyDataSetChanged();

            listview.ItemClick += (sender, e) =>
            {
                int select = e.Position;
                View view = (View)sender;

                switch (select)
                {
                    case 0://reply
                        UserAction.Reply(status, view);
                        this.FinishAndRemoveTask();
                        break;

                    case 1://fav
                        UserAction.Fav(status, view);
                        this.FinishAndRemoveTask();
                        break;

                    case 2://boost
                        UserAction.Boost(status, view);
                        this.FinishAndRemoveTask();
                        break;

                    case 3://USER
                        UserAction.Profile(status.Account, view.Context);
                        this.FinishAndRemoveTask();
                        break;

                    default://urls
                        string urlOrAccountName = itemlist[select];

                        //display conversation
                        if (urlOrAccountName.Equals(SHOW_THE_CONVERSATION))
                        {
                            Intent intent = new Intent(view.Context, typeof(ConversationActivity));
                            intent.PutExtra("statusId", status.Id);
                            this.FinishAndRemoveTask();
                            view.Context.StartActivity(intent);
                            break;
                        }

                        //urls
                        UserAction.UrlOpen(urlOrAccountName, view);
                        this.FinishAndRemoveTask();
                        break;
                }
            };

        }

    }
}