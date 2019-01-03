using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
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
            StatusController statusController = new StatusController(status);

            string accountname = status.Account.AccountName;
            string displayname = status.Account.DisplayName;
            string boostedbyName = displayname + "@" + accountname;

            var view = convertView;
            view = inflater.Inflate(Resource.Layout.Status, parent, false);
   
            //表示内容の設定
            ImageView avatar = view.FindViewById<ImageView>(Resource.Id.imageViewAvatar);
            TextView profile = view.FindViewById<TextView>(Resource.Id.textViewProfile);
            TextView content = view.FindViewById<TextView>(Resource.Id.textViewContent);
            content.Text = "";//init
            TextView createdat = view.FindViewById<TextView>(Resource.Id.textViewCreatedAt);

            ImageView[] imageViews = new ImageView[4];
            imageViews[0] = view.FindViewById<ImageView>(Resource.Id.imageViewImage0);
            imageViews[1] = view.FindViewById<ImageView>(Resource.Id.imageViewImage1);
            imageViews[2] = view.FindViewById<ImageView>(Resource.Id.imageViewImage2);
            imageViews[3] = view.FindViewById<ImageView>(Resource.Id.imageViewImage3);

            //background color
            statusController.SetViewBackColor(view);

            //avatar;
            ImageProvider imageProvider = new ImageProvider();
            var s = status;
            if (status.Reblog != null) s = status.Reblog;
            imageProvider.ImageIconSetAsync(s.Account.StaticAvatarUrl, avatar);
            avatar.Click += (sender, e) =>
            {
                UserAction.Profile(s.Account, view.Context);
            };

            //profile set
            statusController.SetStatusToTextView_forProfile(profile, view.Context);

            //content set
            statusController.SetStatusToTextView(content, ColorDatabase.TLTEXT, view.Context);

            //created at time set 
            statusController.SetCreateDate(createdat, boostedbyName);

            //Preview set
            statusController.SetImagePreview(imageViews, view);

            ////animation add
            //if (position == 0)
            //{
            //    //var anim = Android.Views.Animations.AnimationUtils.LoadAnimation(view.Context, Resource.Animation.listViewScroll);
            //    var anim = Android.Views.Animations.AnimationUtils.LoadAnimation(view.Context, Android.Resource.Animation.FadeIn);
            //    view.StartAnimation(anim);
            //}

            return view;
        }

    }


}