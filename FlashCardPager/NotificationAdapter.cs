﻿using System;
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
    class NotificationAdapter : BaseAdapter<Mastonet.Entities.Notification>
    {
        LayoutInflater inflater;
        List<Mastonet.Entities.Notification> notifications;

        public NotificationAdapter(LayoutInflater inflater, List<Mastonet.Entities.Notification> notifications)
        {
            this.notifications = notifications;
            this.inflater = inflater;
        }

        public override Mastonet.Entities.Notification this[int position]
        {
            get { return notifications[position]; }
        }

        public override int Count
        {
            get { return notifications == null ? 0 : notifications.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            Mastonet.Entities.Notification notification = notifications[position];

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
            imageGetTask.Execute(notification.Account.StaticAvatarUrl);

            //name
            string accountname = notification.Account.AccountName;
            string displayname = notification.Account.DisplayName;

            //サムネイル
            for (int j = 0; j < 4; j++)
            {
                imageViews[j].Visibility = ViewStates.Gone;
            }
            view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;
            view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimagedown).Visibility = ViewStates.Gone;


            //対応
            string type = notification.Type;

            //follow
            if (type.Equals("follow"))
            {
                profile.SetTextColor(Color.DarkBlue);
                profile.Text = displayname + accountname + "さんからフォロー";

                content.SetTextColor(Color.DarkGray);
                content.Text = displayname + "さんのプロフィール\n";
                content.Text += OtherTool.HTML_removeTag(notification.Account.Note);

                createdat.SetTextColor(Color.DarkGray);
                createdat.Text = notification.CreatedAt.ToLocalTime().ToString();

                return view;
            }
            //fav
            else if (type.Equals("favourite"))
            {
                profile.SetTextColor(Color.DarkOrange);
                profile.Text = displayname + accountname + "さんからふぁぼられた";

            }
            //rebolog
            else if (type.Equals("reblog"))
            {
                profile.SetTextColor(Color.DarkGreen);
                profile.Text = displayname + accountname + "さんからぶーすとされた";
            }
            //mention
            else if (type.Equals("mention"))
            {
                profile.SetTextColor(Color.DarkMagenta);
                profile.Text = displayname + accountname + "さんからとぅーと!";
            }
            else
            {
                profile.Text = "なんかの通知がきた";
                content.Text = "内容はわからんちん";
                createdat.Text = "時間を気にするやつは何してもだめ";
                return view;
            }

            content.SetTextColor(Color.Black);
            //普通の文章を追加
            string _content = OtherTool.HTML_removeTag(notification.Status.Content);
            content.Text = _content;
            ////画像URL取得 → contentに追加
            List<string> list = OtherTool.DLG_ITEM_getURL(notification.Status);
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
            

            createdat.SetTextColor(Color.DarkGray);
            createdat.Text = notification.Status.CreatedAt.ToLocalTime().ToString();

            return view;
        }

    }


}