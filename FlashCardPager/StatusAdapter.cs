using System;
using System.Collections.Generic;
using System.Linq;
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
        List<string> imageUrls;

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
            view.SetBackgroundColor(Color.AliceBlue);

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
            ImageProvider imageProvider = new ImageProvider();
            imageProvider.ImageIconSetAsync(status.Account.AvatarUrl, avatar);
            avatar.Click += (sender, e) =>
            {
                UserAction.Profile(status.Account, view.Context);
            };

            //profile
            profile.SetTextColor(Color.DarkOliveGreen);
            ////ブーストされてるか判断する
            if (accountname != status.Account.AccountName)
            {
                profile.SetTextColor(new Color(45, 45, 45));
                view.SetBackgroundColor(new Color(211, 244, 203));
            }
            if(status.Visibility == Visibility.Private)
            {
                profile.Text = "🔒";
            }
            else
            {
                profile.Text = "";
            }
            profile.Text += status.Account.DisplayName + "@" + status.Account.AccountName;

            //replyを確認する
            if(status.Content.Contains("@"+UserClient.currentAccountName))
            {
                view.SetBackgroundColor(new Color(255, 226, 216));
            }

            //content
            string _content = OtherTool.HTML_removeTag(status.Content);

            ////画像URL取得 → contentに追加
            List<string> list = OtherTool.DLG_ITEM_getURL(status);
            foreach (var add in list)
            {
                if (!_content.Contains("@") && add != UserClient.instance)
                {
                    if (add.Contains("media") || add.Contains("jpg") || add.Contains("jpeg"))
                    {
                        if (!UserAction.bImagePre)
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
            foreach(EmojiPosition ep in emojiPositions)
            {
                Bitmap b = emojiGetTask.GetBitmap(ep.shortcode);
                if(b != null)
                {
                    var imageSpan = new ImageSpan(view.Context, b);
                    spannableString.SetSpan(imageSpan, ep.start, ep.end, SpanTypes.ExclusiveExclusive);
                }
            }
            spannableString.SetSpan(new ForegroundColorSpan(Color.Black), 0, _content.Length, SpanTypes.ExclusiveExclusive);
            content.TextFormatted = spannableString;


            //created at time 
            createdat.SetTextColor(Color.DarkGray);

            if (accountname != status.Account.AccountName) createdat.Text
                    = "Boosted by " + displayname + "@" + accountname + "\r\n" + status.CreatedAt.ToLocalTime();
            else
                createdat.Text = status.CreatedAt.ToLocalTime() + "";


            //プレビューを使う
            imageUrls = new List<string>();
            if (UserAction.bImagePre)
            {
                //サムネイルの表示
                int i = 0;
                imageUrls = OtherTool.ImageUrlPreviewfromStatus(status);
                for (i = 0; i < imageUrls.Count; i++)
                {
                    ImageProvider imageProvider2 = new ImageProvider();
                    imageProvider2.ImageThumnailSetAsync(imageUrls[i], imageViews[i]);
                }
                for (int j = i; j < 4; j++)
                {
                    imageViews[j].Visibility = ViewStates.Gone;
                }

                if (imageUrls.Count <= 2)
                {
                    view.FindViewById<LinearLayout>(Resource.Id.Linearlayoutimagedown).Visibility = ViewStates.Gone;
                    if (imageUrls.Count == 0)
                    {
                        view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;
                    }
                }

                //サムネイル クリックイベント
                //画質の設定
                List<string> thumbnail;
                if (UserAction.bImageQuality)
                {
                    thumbnail = OtherTool.ImageUrlRemotefromStatus(status);
                }
                else
                {
                    thumbnail = OtherTool.ImageUrlPreviewfromStatus(status);
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
            //プレビューを使わない
            else
            {
                for(int k = 0; k < 4; k++)
                {
                    imageViews[k].Visibility = ViewStates.Gone;
                }
                view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;
                view.FindViewById<LinearLayout>(Resource.Id.Linearlayoutimagedown).Visibility = ViewStates.Gone;
            }

            return view;
        }

    }


}