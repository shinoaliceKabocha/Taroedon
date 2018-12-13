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

namespace FlashCardPager
{
    class NotificationAdapter : BaseAdapter<Mastonet.Entities.Notification>
    {
        LayoutInflater inflater;
        List<Mastonet.Entities.Notification> notifications;
        List<string> imageUrls;

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


            //name
            string accountname = notification.Account.AccountName;
            string displayname = notification.Account.DisplayName;
            profile.SetTextColor(new Color(45,45,45));


            //対応
            string type = notification.Type;

            //follow
            if (type.Equals("follow"))
            {
                //profile.SetTextColor(Color.DarkBlue);
                view.SetBackgroundColor(new Color(191, 216, 255));
                profile.Text = displayname + accountname + "さんからフォロー";

                content.SetTextColor(Color.Black);
                content.Text = displayname + "さんのプロフィール\n";
                content.Text += OtherTool.HTML_removeTag(notification.Account.Note);

                createdat.SetTextColor(Color.DarkGray);
                createdat.Text = notification.CreatedAt.ToLocalTime().ToString();

                for (int k = 0; k < 4; k++)
                {
                    imageViews[k].Visibility = ViewStates.Gone;
                }
                view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;

                return view;
            }
            else
            {
                if (notification.Status.Visibility == Visibility.Direct)
                {
                    profile.Text = "📨";
                }
                else if (notification.Status.Visibility == Visibility.Private)
                {
                    profile.Text = "🔒";
                }
                else
                {
                    profile.Text = "";
                }


                //fav
                if (type.Equals("favourite"))
                {
                    //profile.SetTextColor(Color.DarkOrange);
                    view.SetBackgroundColor(new Color(255, 249, 216));
                    profile.Text += displayname + accountname + "さんからふぁぼられた";

                }
                //rebolog
                else if (type.Equals("reblog"))
                {
                    //profile.SetTextColor(Color.DarkGreen);
                    view.SetBackgroundColor(new Color(211,244,203));
                    profile.Text += displayname + accountname + "さんからぶーすとされた";
                }
                //mention
                else if (type.Equals("mention"))
                {
                    //profile.SetTextColor(Color.DarkMagenta);
                    view.SetBackgroundColor(new Color(255, 226, 216));
                    profile.Text += displayname + accountname + "さんからとぅーと!";
                }
                else
                {
                    profile.Text = "なんかの通知がきた";
                    content.Text = "内容はわからんちん";
                    createdat.Text = "時間を気にするやつは何してもだめ";
                    return view;
                }
            }


            //content
            string _content;
            content.SetText(Html.FromHtml(notification.Status.Content), TextView.BufferType.Spannable);
            _content = content.Text;
            try
            {
                _content = _content.Substring(0, _content.Length - 2);
            }
            catch (Exception e)
            {
                //_content = OtherTool.HTML_removeTag(notification.Status.Content);
            }



            ////画像URL取得 → contentに追加
            List<string> list = OtherTool.DLG_ITEM_getURL(notification.Status);
            foreach (var add in list)
            {
                if (!_content.Contains("@") && add != UserClient.instance)
                {
                    if (add.Contains("media") || add.Contains("jpg") || add.Contains("jpeg"))
                    {
                        if (!UserAction.bImagePre && !_content.Contains(add))
                        {
                            if (add.Length > 30) _content += "\r\nimg:" + add.Substring(0, 30) + "....";
                            else _content += "\r\nimg:" + add;
                        }
                    }
                }
            }
            //emoji content set!!
            EmojiGetTask emojiGetTask = new EmojiGetTask();
            var emojiPositions = emojiGetTask.EmojiPostions(_content);

            var spannableString = new SpannableString(_content);
            foreach (EmojiPosition ep in emojiPositions)
            {
                Bitmap b = emojiGetTask.GetBitmap(ep.shortcode);
                if (b != null)
                {
                    var imageSpan = new ImageSpan(view.Context, b);
                    spannableString.SetSpan(imageSpan, ep.start, ep.end, SpanTypes.ExclusiveExclusive);
                }
            }
            spannableString.SetSpan(new ForegroundColorSpan(UserAction.COLOR_TEXT), 0, _content.Length, SpanTypes.ExclusiveExclusive);
            content.TextFormatted = spannableString;


            createdat.SetTextColor(Color.DarkGray);
            createdat.Text = "Fav:" + notification.Status.FavouritesCount + "  Boost:" + notification.Status.ReblogCount + "  "
                + notification.Status.CreatedAt.ToLocalTime() + "";


            if(notification.Status != null && UserAction.bImagePre==true)
            {
                //サムネイルの表示
                int i = 0;
                imageUrls = OtherTool.ImageUrlPreviewfromStatus(notification.Status);
                for (i = 0; i < imageUrls.Count; i++)
                {
                    ImageProvider imageProvider2 = new ImageProvider();
                    imageProvider2.ImageThumnailSetAsync(imageUrls[i], imageViews[i]);

                    //ImageGetTask2 imageGetTask2 = new ImageGetTask2(imageViews[i]);
                    //imageGetTask2.Execute(imageUrls[i]);
                }
                for (int j = i; j < 4; j++)
                {
                    imageViews[j].Visibility = ViewStates.Gone;
                }

                if (imageUrls.Count == 0)
                {
                    view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;
                }

                //サムネイル クリックイベント
                //画質の設定
                List<string> thumbnail;
                if (UserAction.bImageQuality)
                {
                    thumbnail = OtherTool.ImageUrlRemotefromStatus(notification.Status);
                }
                else
                {
                    thumbnail = OtherTool.ImageUrlPreviewfromStatus(notification.Status);
                }
                //イベント発行
                imageViews[0].Click += (sender, e) =>
                {
                    if (thumbnail.Count > 0)
                    {
                        string u = thumbnail[0];
                        if (!u.Equals(null)) UserAction.UrlOpen(u, view);
                    }
                };
                imageViews[1].Click += (sender, e) =>
                {
                    if (thumbnail.Count > 1)
                    {
                        string u = thumbnail[1];
                        if (!u.Equals(null)) UserAction.UrlOpen(u, view);
                    }
                };
                imageViews[2].Click += (sender, e) =>
                {
                    if (thumbnail.Count > 2)
                    {
                        string u = thumbnail[2];
                        if (!u.Equals(null)) UserAction.UrlOpen(u, view);
                    }
                };
                imageViews[3].Click += (sender, e) =>
                {
                    if (thumbnail.Count > 3)
                    {
                        string u = thumbnail[3];
                        if (!u.Equals(null)) UserAction.UrlOpen(u, view);
                    }
                };

            }
            else
            {
                for (int k = 0; k < 4; k++)
                {
                    imageViews[k].Visibility = ViewStates.Gone;
                }
                view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;
            }

            return view;
        }

    }


}