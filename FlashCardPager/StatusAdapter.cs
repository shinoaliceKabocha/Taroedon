using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mastonet;
using Mastonet.Entities;

namespace FlashCardPager
{
    class StatusAdapter : BaseAdapter<Status>
    {
        List<Status> statuslist;
        LayoutInflater inflater;

        public StatusAdapter(LayoutInflater inflater, List<Status> statuslist)
        {
            this.statuslist = statuslist;
            this.inflater = inflater;
        }

        public override Status this[int position]
        {
            get { return statuslist[position]; }
        }

        public override int Count
        {
            get { return statuslist == null ? 0 : statuslist.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            Status status = statuslist[position];
            string accountname = status.Account.AccountName;
            string displayname = status.Account.DisplayName;
            //rebolog check
            if (status.Reblog != null) status = status.Reblog;

            var view = convertView;
            view = inflater.Inflate(Resource.Layout.Status, parent, false);

            //表示内容の設定
            ImageView avatar = view.FindViewById<ImageView>(Resource.Id.imageViewAvatar);
            TextView profile = view.FindViewById<TextView>(Resource.Id.textViewProfile);
            TextView content = view.FindViewById<TextView>(Resource.Id.textViewContent);
            TextView createdat = view.FindViewById<TextView>(Resource.Id.textViewCreatedAt);

            ImageView[] imageViews = new ImageView[4];
            imageViews[0] = view.FindViewById<ImageView>(Resource.Id.imageViewImage0);
            imageViews[1] = view.FindViewById<ImageView>(Resource.Id.imageViewImage1);
            imageViews[2] = view.FindViewById<ImageView>(Resource.Id.imageViewImage2);
            imageViews[3] = view.FindViewById<ImageView>(Resource.Id.imageViewImage3);

            //avatar;
            ImageGetTask imageGetTask = new ImageGetTask(avatar);
            imageGetTask.Execute(status.Account.StaticAvatarUrl);
            avatar.Click += async (sender, e) =>
            {
                Log.Debug("post", "user name:" + status.Account.UserName + " status id:" + status.Id + "myfav:" + status.Favourited + "myBoosted:" + status.Reblogged);
            };


            //profile
            profile.SetTextColor(Color.DarkOliveGreen);
            ////ブーストされてるか判断する
            if (accountname != status.Account.AccountName)
            {
                profile.SetTextColor(Color.DarkRed);
            }
            profile.Text = status.Account.DisplayName + "@" + status.Account.AccountName;


            //content
            content.SetTextColor(Color.Black);
            string _content = OtherTool.HTML_removeTag(status.Content);
            content.Text = _content;

            ////画像URL取得 → contentに追加
            List<string> list = OtherTool.DLG_ITEM_getURL(status);
            foreach (var add in list)
            {
                if (!_content.Contains("@") && add != UserClient.instance)
                {
                    if (add.Contains("media"))
                    {
                        if (add.Length > 30) content.Text += "\r\nimg:" + add.Substring(0, 30) + "....";
                        else content.Text += "\r\nimg:" + add;
                    }
                    else
                    {
                        if (add.Length > 30) content.Text += "\r\n" + add.Substring(0, 30) + "....";
                        else content.Text += "\r\n" + add;
                    }
                }
            }

            //created at time 
            createdat.SetTextColor(Color.DarkGray);

            if (accountname != status.Account.AccountName) createdat.Text
                    = "Boosted by " + displayname + "@" + accountname + "\r\n" + status.CreatedAt.ToLocalTime();
            else
                createdat.Text = status.CreatedAt.ToLocalTime() + "";


            //image
            int i = 0;
            //サムネイル
            //foreach (var image_url in images_urls)
            //{
            //    ImageGetTask imageGetTask2 = new ImageGetTask(imageViews[i]);
            //    imageGetTask2.Execute(image_url.Url);
            //    i++;
            //}
            for (int j = i; j < 4; j++)
            {
                imageViews[j].Visibility = ViewStates.Gone;
            }
            view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;
            view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimagedown).Visibility = ViewStates.Gone;

            return view;
        }

    }


}