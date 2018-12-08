using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Views;
using Android.Widget;

namespace FlashCardPager
{
    [Activity(Label = "ProfileActivity", Theme = "@style/PostTheme", ScreenOrientation = ScreenOrientation.Portrait )]
    public class ProfileActivity : Activity
    {
        readonly string FOLLOW = "follow";
        readonly string UNFOLLOW = "unfollow";
        readonly string CANCEL = "cancel";
        readonly string FOLLOW_SUCCESS = "フォローしました";
        readonly string FOLLOW_REQUEST = "フォローリクエストしました";
        readonly string FOLLOW_FAIL = "フォローに失敗しました";
        readonly string UNFOLLOW_SUCCESS = "アンフォローしました";
        readonly string UNFOLLOW_FAIL = "アンフォローに失敗しました";
        readonly Color COLOR_FOLLOW_SUCCESS = new Color(127, 176, 255);//Blue
        readonly Color COLOR_FOLLOW_REQUEST = new Color(127, 176, 255);
        readonly Color COLOR_FOLLOW_FAILED = UserAction.COLOR_FAILED;
        readonly Color COLOR_UNFOLLOW_SUCCESS = new Color(127, 176, 255);
        readonly Color COLOR_UNFOLLOW_FAILED = UserAction.COLOR_FAILED;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Profile);
            //いい感じの大きさにする
            IWindowManager windowManager = (IWindowManager)this.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetRealSize(point);
            Window.SetLayout((int)(point.X * 0.98), (int)(point.Y * 0.60));


            var imageViewAvatar = FindViewById<ImageView>(Resource.Id.imageViewAvatar);
            ImageProvider imageProvider = new ImageProvider();
            //imageProvider.ImageIconSetAsync(this.Intent.GetStringExtra("avatar"), imageViewAvatar);//Cache
            setImageViewAsync(this.Intent.GetStringExtra("avatar"), imageViewAvatar);

            var imageViewHeader = FindViewById<ImageView>(Resource.Id.imageViewHeader);
            setImageViewAsync(this.Intent.GetStringExtra("header"), imageViewHeader);


            string accountName = this.Intent.GetStringExtra("account_name");
            bool locked = this.Intent.GetBooleanExtra("lock", true);
            string _accountName;
            if (locked) _accountName = "🔒  " + accountName;
            else _accountName = accountName;
            FindViewById<TextView>(Resource.Id.textView_AccountName).Text = _accountName;

            string displayName = this.Intent.GetStringExtra("display_name");
            FindViewById<TextView>(Resource.Id.textView_DisplayName).Text = displayName;


            string note = this.Intent.GetStringExtra("note");
            var noteText = FindViewById<TextView>(Resource.Id.textViewNote);
            noteText.SetText(Html.FromHtml(note), TextView.BufferType.Spannable);
            noteText.MovementMethod = new LocalLinkMovementMethod();


            string ff = "Follow:"+this.Intent.GetStringExtra("follow") + "  Follower:" + this.Intent.GetStringExtra("follower");
            TextView textViewFF = FindViewById<TextView>(Resource.Id.textViewFF);
            textViewFF.Text = ff;

            //follow button
            long id = this.Intent.GetLongExtra("id", 0);
            Button buttonFF = FindViewById<Button>(Resource.Id.buttonFollow);
            ContentSet_From_RelationshipsAsync(accountName, id, buttonFF, textViewFF);

            buttonFF.LongClick += (sender, e) =>
            {
            };

            buttonFF.Click += (sender, e) =>
            {
                //client
                var client = new UserClient().getClient();

                //follow
                if (buttonFF.Text == FOLLOW)
                {
                    var dlg = new AlertDialog.Builder(this);
                    dlg.SetTitle("フォローしますか");
                    dlg.SetPositiveButton(
                        FOLLOW, async (s, a) =>
                        {
                            try
                            {
                                var result = await client.Follow(id);
                                //フォローできた
                                if (result.Following)
                                {
                                    buttonFF.Text = UNFOLLOW;
                                    UserAction.Toast_BottomFIllHorizontal_Show(FOLLOW_SUCCESS, this, COLOR_FOLLOW_SUCCESS);
                                }
                                //フォローリクエスト
                                else
                                {
                                    UserAction.Toast_BottomFIllHorizontal_Show(FOLLOW_REQUEST, this, COLOR_FOLLOW_REQUEST);
                                }
                            }
                            catch (Exception exception)
                            {
                                UserAction.Toast_BottomFIllHorizontal_Show(FOLLOW_FAIL, this, COLOR_FOLLOW_FAILED);
                            }
                        });
                    dlg.SetNegativeButton(
                        CANCEL,(s, a) =>
                        {
                            //do nothing.
                        });

                    dlg.Create().Show();
                }
                //unfollow
                else
                {
                    var dlg = new AlertDialog.Builder(this);
                    dlg.SetTitle("アンフォローしますか");
                    dlg.SetPositiveButton(
                        UNFOLLOW, async (s, a) =>
                        {
                            try
                            {
                                var result = await client.Unfollow(id);
                                //アンフォローできた
                                if (!result.Following)
                                {
                                    buttonFF.Text = FOLLOW;
                                    UserAction.Toast_BottomFIllHorizontal_Show(UNFOLLOW_SUCCESS, this, COLOR_UNFOLLOW_SUCCESS);
                                }
                                //アンフォロー失敗
                                else
                                {
                                    UserAction.Toast_BottomFIllHorizontal_Show(UNFOLLOW_FAIL, this, COLOR_UNFOLLOW_FAILED);
                                }
                            }
                            //アンフォロー失敗
                            catch (Exception exception)
                            {
                                UserAction.Toast_BottomFIllHorizontal_Show(UNFOLLOW_FAIL, this, COLOR_UNFOLLOW_FAILED);
                            }

                        });
                    dlg.SetNegativeButton(
                        CANCEL, (s, a) =>
                        {
                            //do nothing.
                        });

                    dlg.Create().Show();
                }
            };

        }

        private async void setImageViewAsync(string url, ImageView imageView)
        {
            using (var webClient = new WebClient())
            {
                Uri uri = new Uri(url);
                var imageByte = await webClient.DownloadDataTaskAsync(uri);
                if(imageByte != null && imageByte.Length > 0)
                {
                    var bitmap = BitmapFactory.DecodeByteArray(imageByte, 0, imageByte.Length);
                    imageView.SetImageBitmap(bitmap);
                }
            }
        }

        private async void ContentSet_From_RelationshipsAsync(string accountName, long id, Button button, TextView textView)
        {
            if(id == 0)
            {
                button.Text = "????";
                button.Enabled = false;
                return;
            }
            else
            {
                var client = new UserClient().getClient();
                if(UserClient.currentAccountName == accountName)
                {
                    button.Text = "My Account";
                    button.Enabled = false;
                    return;
                }

                var relation = await client.GetAccountRelationships(id);
                foreach (var rl in relation)
                {
                    //followed
                    if(rl.FollowedBy)
                    {
                        textView.Text += "\nフォローされています";
                    }

                    if (rl.Following)
                    {
                        button.Text = UNFOLLOW;
                    }
                    else
                    {
                        button.Text = FOLLOW;
                    }
                }
            }

        }
    }

    public class LocalLinkMovementMethod : Android.Text.Method.LinkMovementMethod
    {
        public override bool OnTouchEvent(TextView widget, ISpannable buffer, MotionEvent e)
        {
            var action = e.Action;
            if(action == MotionEventActions.Down || action == MotionEventActions.Up)
            {
                int x = (int)e.GetX();
                int y = (int)e.GetY();

                x -= widget.TotalPaddingLeft;
                y -= widget.TotalPaddingTop;

                x += widget.ScrollX;
                y += widget.ScrollX;

                var layout = widget.Layout;
                int line = layout.GetLineForVertical(y);
                int off = layout.GetOffsetForHorizontal(line, x);

                Type type = typeof(ClickableSpan);
                var link = buffer.GetSpans(off, off,  Java.Lang.Class.FromType(type) );

                if(link.Length != 0)
                {
                    if(action == MotionEventActions.Up)
                    {
                        if(link[0] is URLSpan)
                        {
                            string url = ((URLSpan)link[0]).URL;
                            View view = (View)widget;
                            UserAction.UrlOpen(url, view);
                        }
                    }
                    else  if(action == MotionEventActions.Down)
                    {
                        Selection.SetSelection(buffer, buffer.GetSpanStart(link[0]), buffer.GetSpanEnd(link[0]));
                    }
                    return true;
                }
                else
                {
                    Selection.RemoveSelection(buffer);
                }
               
            }
            return base.OnTouchEvent(widget, buffer, e);


        }
    }


}

