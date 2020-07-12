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
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Mastonet.Entities;
using Context = Android.Content.Context;
using Notification = Mastonet.Entities.Notification;

namespace Taroedon
{
    public class NotifyController : StatusController
    {
        private Notification notification;
        string type;
        public NotifyController(Notification _notification) : base(_notification.Status)
        {
            this.notification = _notification;
            type = notification.Type;
        }

        public override void SetViewBackColor(View view)
        {
            switch (type)
            {
                case "favourite":
                    {
                        view.SetBackgroundColor(ColorDatabase.FAV_BACK);
                        break;
                    }
                case "reblog":
                    {
                        view.SetBackgroundColor(ColorDatabase.BOOST_BACK);
                        break;
                    }
                case "mention":
                    {
                        view.SetBackgroundColor(ColorDatabase.REPLY_BACK);
                        break;
                    }
                default:
                    {
                        view.SetBackgroundColor(ColorDatabase.TL_BACK);
                        break;
                    }
            }
        }

        public override void SetStatusToTextView_forProfile(TextView profileTextView, Context context)
        {
            //🔒?
            if (status.Visibility == Mastonet.Visibility.Private)
            {
                profileTextView.Text = "🔒";
            }
            //📨？
            else if (status.Visibility == Mastonet.Visibility.Direct)
            {
                profileTextView.Text = "📨";
            }
            else
            {
                profileTextView.Text = "";
            }

            string displayname = notification.Account.DisplayName;
            string accountname = notification.Account.AccountName;

            switch (type)
            {
                case "favourite":
                    {
                        profileTextView.Text 
                            += displayname + "@" + accountname + "さんからfav";
                        break;
                    }
                case "reblog":
                    {
                        profileTextView.Text 
                            += displayname + "@" + accountname + "さんからboost";
                        break;
                    }
                case "mention":
                    {
                        profileTextView.Text += displayname + "@" + accountname + "さんからreply";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            //emoji convert
            SetStatusToTextView(profileTextView, ColorDatabase.PROFILE, context);
        }

    }

    public class StatusController
    {
        protected Status status;
        protected bool boostedFlg = false;

        public StatusController(Status _status)
        {
            var s = _status;
            if (_status.Reblog != null)
            {
                s = _status.Reblog;
                this.boostedFlg = true;
            }
            this.status = s;
        }

        // 1. parse HTML
        public void SetStatusToTextView(TextView textView, Color color, Context context)
        {
            //parce htmml
            string _content;
            if (string.IsNullOrEmpty(textView.Text))
            {
                textView.SetText(Html.FromHtml(status.Content), TextView.BufferType.Spannable);
                _content = textView.Text;
                try
                {
                    _content = _content.Substring(0, _content.Length - 2);
                }
                catch (Exception e)
                {
                    /* do nothing */
                }
            }
            //for profile
            else
            {
                _content = textView.Text;
            }
            //image url set(preview off case: not use)
            SetImageUrlContentFooter(ref _content);

            //emoji set
            EmojiGetTask emojiGetTask = new EmojiGetTask();
            emojiGetTask.SetStringConvertEmoji(textView, _content, color, context);

        }

        public virtual void SetStatusToTextView_forProfile(TextView profileTextView, Context context)
        {
            //🔒?
            if (status.Visibility == Mastonet.Visibility.Private)
            {
                profileTextView.Text = "🔒";
            }
            //📨？
            else if (status.Visibility == Mastonet.Visibility.Direct)
            {
                profileTextView.Text = "📨";
            }
            else
            {
                profileTextView.Text = "";
            }
            profileTextView.Text += status.Account.DisplayName + "@" + status.Account.AccountName;

            SetStatusToTextView(profileTextView, ColorDatabase.PROFILE, context);
        }

        public virtual void SetStatusToTextView_forProfile(TextView profileTextView, Color color, Context context)
        {
            //🔒?
            if (status.Visibility == Mastonet.Visibility.Private)
            {
                profileTextView.Text = "🔒";
            }
            //📨？
            else if (status.Visibility == Mastonet.Visibility.Direct)
            {
                profileTextView.Text = "📨";
            }
            else
            {
                profileTextView.Text = "";
            }
            profileTextView.Text += status.Account.DisplayName + "@" + status.Account.AccountName;

            SetStatusToTextView(profileTextView, color, context);
        }

        // ２．changed background color
        public virtual void SetViewBackColor(View view)
        {
            //boost green
            if (boostedFlg)
            {
                view.SetBackgroundColor(ColorDatabase.BOOST_BACK);
            }
            //reply red
            else if (status.Content.Contains("@" + UserClient.currentAccountName))
            {
                view.SetBackgroundColor(ColorDatabase.REPLY_BACK);
            }
            //other nomal
            else
            {
                view.SetBackgroundColor(ColorDatabase.TL_BACK);
            }
        }

        // ３．set time
        public void SetCreateDate(TextView createTextView, string boostedbyName)
        {
            createTextView.SetTextColor(ColorDatabase.TIME);
            createTextView.Text = "";
            if (boostedFlg)
            {
                createTextView.Text
                    += "Boosted by " + boostedbyName + "\r\n";
            }

            createTextView.Text += 
                "Fav:" + status.FavouritesCount + 
                "  Boost:" + status.ReblogCount + "  "
                + status.CreatedAt.ToLocalTime() + "";

        }


        // ４．Preview
        public void SetImagePreview(ImageView[] imageViews, View view)
        {
            if (UserAction.bImagePre)
            {
                // imageViews.setImage all
                List<string> imageUrls = new List<string>();
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

                //low quarity of images
                List<string> thumbnail;
                if (UserAction.bImageQuality)
                {
                    thumbnail = OtherTool.ImageUrlRemotefromStatus(status);
                }
                else
                {
                    thumbnail = OtherTool.ImageUrlPreviewfromStatus(status);
                }
                //event
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
            //not preview
            else
            {
                for (int k = 0; k < 4; k++)
                {
                    imageViews[k].Visibility = ViewStates.Gone;
                }
                view.FindViewById<LinearLayout>(Resource.Id.linearlayoutimageup).Visibility = ViewStates.Gone;
                view.FindViewById<LinearLayout>(Resource.Id.Linearlayoutimagedown).Visibility = ViewStates.Gone;
            }

        }

        // 4-2 Preview imageurl add content
        private void SetImageUrlContentFooter(ref string _content)
        {
            if (UserAction.bImagePre) return;

            List<string> thumbnail;
            if (UserAction.bImageQuality)
            {
                thumbnail = OtherTool.ImageUrlRemotefromStatus(status);
            }
            else
            {
                thumbnail = OtherTool.ImageUrlPreviewfromStatus(status);
            }
            foreach (var add in thumbnail)
            {
                if(add.Length > 30) _content += "\r\nimg:" + add.Substring(0, 30) + "...";
                else _content += "\r\nimg:" + add;
            }

        }

    }




}