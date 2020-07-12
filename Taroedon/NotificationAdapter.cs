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
using Android.Text.Method;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mastonet;
using Mastonet.Entities;
using Notification = Mastonet.Entities.Notification;

namespace Taroedon
{
    class NotificationAdapter : BaseAdapter<Mastonet.Entities.Notification>
    {
        LayoutInflater inflater;
        List<Notification> notifications;
        List<string> imageUrls;

        public NotificationAdapter(LayoutInflater inflater, List<Mastonet.Entities.Notification> notifications)
        {
            this.notifications = notifications;
            this.inflater = inflater;
        }

        public override Notification this[int position]
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
            Notification notification = notifications[position];

            string accountname = notification.Account.AccountName;
            string displayname = notification.Account.DisplayName;
            string boostedbyName = displayname + "@" + accountname;

            var view = convertView;
            view = inflater.Inflate(Resource.Layout.Status, parent, false);

            //setting content
            ImageView avatar = view.FindViewById<ImageView>(Resource.Id.imageViewAvatar);
            TextView profile = view.FindViewById<TextView>(Resource.Id.textViewProfile);
            TextView content = view.FindViewById<TextView>(Resource.Id.textViewContent);
            content.Text = "";//init
            TextView createdat = view.FindViewById<TextView>(Resource.Id.textViewCreatedAt);

            imageUrls = new List<string>();
            ImageView[] imageViews = new ImageView[4];
            imageViews[0] = view.FindViewById<ImageView>(Resource.Id.imageViewImage0);
            imageViews[1] = view.FindViewById<ImageView>(Resource.Id.imageViewImage1);
            imageViews[2] = view.FindViewById<ImageView>(Resource.Id.imageViewImage2);
            imageViews[3] = view.FindViewById<ImageView>(Resource.Id.imageViewImage3);

            //avatar;
            ImageProvider imageProvider = new ImageProvider();
            imageProvider.ImageIconSetAsync(notification.Account.StaticAvatarUrl, avatar);
            avatar.Click += (sender, e) => 
            {
                UserAction.Profile(notification.Account, view.Context);
            };

            //follow notify
            string type = notification.Type;
            if (type.Equals("follow"))
            {
                //background color
                view.SetBackgroundColor(ColorDatabase.FF_BACK);
                //profile set
                profile.SetTextColor(ColorDatabase.TLTEXT);
                profile.Text = boostedbyName + "さんからフォロー";
                //content set
                content.SetTextColor(ColorDatabase.TLTEXT);
                content.SetText(Html.FromHtml(notification.Account.Note), TextView.BufferType.Spannable);
                string content_str = content.Text;
                content_str = displayname + "さんのプロフィール\n" + content_str;
                content.SetText(content_str, TextView.BufferType.Spannable);
                //created at time set
                createdat.SetTextColor(ColorDatabase.TIME);
                createdat.Text = notification.CreatedAt.ToLocalTime().ToString();
                //preview set -> not set
                for (int k = 0; k < 4; k++)
                {
                    imageViews[k].Visibility = ViewStates.Gone;
                }
                view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;
                return view;
            }

            /* notification.status != null */
            NotifyController notifyController = new NotifyController(notification);

            //background color
            notifyController.SetViewBackColor(view);

            //profile set
            notifyController.SetStatusToTextView_forProfile(profile, view.Context);
            
            //content set
            notifyController.SetStatusToTextView(content, ColorDatabase.TLTEXT, view.Context);
            
            //created at time set
            notifyController.SetCreateDate(createdat, boostedbyName);
            
            //Preview set
            notifyController.SetImagePreview(imageViews, view);

            return view;
        }

    }


}