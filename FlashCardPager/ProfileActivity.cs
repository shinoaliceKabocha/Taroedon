using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
using Mastonet.Entities;

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
        readonly Color COLOR_FOLLOW_SUCCESS = ColorDatabase.FOLLOW;
        readonly Color COLOR_FOLLOW_REQUEST = ColorDatabase.FOLLOW;
        readonly Color COLOR_FOLLOW_FAILED = ColorDatabase.FAILED;
        readonly Color COLOR_UNFOLLOW_SUCCESS = ColorDatabase.FOLLOW;
        readonly Color COLOR_UNFOLLOW_FAILED = ColorDatabase.FAILED;

        List<Mastonet.Entities.Status> statuses = new List<Mastonet.Entities.Status>();
        private ListView mListView;
        private StatusAdapter mStatusAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Profile);
            //いい感じの大きさにする
            IWindowManager windowManager = (IWindowManager)this.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetRealSize(point);
            Window.SetLayout((int)(point.X * 0.98), (int)(point.Y * 0.85));

            var imageViewAvatar = FindViewById<ImageView>(Resource.Id.imageViewAvatar);
            ImageProvider imageProvider = new ImageProvider();
            string avatar_url = this.Intent.GetStringExtra("avatar");
            imageProvider.ImageIconSetAsync(avatar_url, imageViewAvatar);//Cache

            imageViewAvatar.Click += (sender, e) =>
            {
                UserAction.UrlOpen(avatar_url, imageViewAvatar);
            };

            var imageViewHeader = FindViewById<ImageView>(Resource.Id.imageViewHeader);
            setImageViewAsync(this.Intent.GetStringExtra("header"), imageViewHeader);


            string accountName = this.Intent.GetStringExtra("account_name");
            bool locked = this.Intent.GetBooleanExtra("lock", true);
            string _accountName;
            if (locked) _accountName = "🔒  " + accountName;
            else _accountName = accountName;

            var accaountTextView = FindViewById<TextView>(Resource.Id.textView_AccountName);
            accaountTextView.Text = "";
            accaountTextView.Text = _accountName;

            string displayName = this.Intent.GetStringExtra("display_name");
            var displayNameTextView = FindViewById<TextView>(Resource.Id.textView_DisplayName);
            new EmojiGetTask().SetStringConvertEmoji(displayNameTextView, displayName, Color.White, this);

            string ff = "Follow:"+this.Intent.GetStringExtra("follow") + "  Follower:" + this.Intent.GetStringExtra("follower");
            TextView textViewFF = FindViewById<TextView>(Resource.Id.textViewFF);
            textViewFF.Text = ff;

            //follow button
            long id = this.Intent.GetLongExtra("id", 0);
            Button buttonFF = FindViewById<Button>(Resource.Id.buttonFollow);
            ContentSet_From_RelationshipsAsync(accountName, id, buttonFF, textViewFF);

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

            //tweet list
            mListView = FindViewById<ListView>(Resource.Id.listViewStatusforAccount);
            mListView.SetBackgroundColor(ColorDatabase.TL_BACK);

            string note = this.Intent.GetStringExtra("note");
            TextView noteText = new TextView(this);
            noteText.SetTextColor(ColorDatabase.TLTEXT);
            noteText.SetPadding(25, 10, 25, 0);
            noteText.SetText(Html.FromHtml(note), TextView.BufferType.Spannable);
            noteText.SetLinkTextColor(GetColorStateList(ColorDatabase.TLLINK));
            noteText.MovementMethod = new LocalLinkMovementMethod();
            mListView.AddHeaderView(noteText);

            //tweet list set
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            SetAccountStatusesAsync(id, mListView, mStatusAdapter, layoutInflater);

            //profile icon get あとでとれるように それまではCache
            setImageViewAsync(avatar_url, imageViewAvatar);

            //イベント
            mListView.ItemClick += (sender, e) =>
            {
                Status st = statuses[e.Position -1];//注意！！
                View view = this.FindViewById(Android.Resource.Id.Content);
                UserAction.ListViewItemClick(st, view);
            };
            //ショートカット
            mListView.ItemLongClick += (sender, e) =>
            {
                try
                {
                    int select = e.Position - 1;//注意！！
                    var status = statuses[select];
                    try
                    {
                        //投稿がリツイートサれたものである場合は，もとの取得先を手に入れる
                        var re_status = status.Reblog;
                        if (re_status != null) status = re_status;
                    }
                    catch (Exception ex) { }

                    UserAction.Fav(status, this.FindViewById(Android.Resource.Id.Content));
                }
                catch(Exception ex) { /*nothing*/}

            };
            //swipe down
            mListView.ScrollStateChanged += Listview_ScrollStateChanged;
        }

        private async void SetAccountStatusesAsync(long id, ListView listview, StatusAdapter adapter, LayoutInflater layoutInflater)
        {
            Mastonet.MastodonClient client = new UserClient().getClient();
            statuses = await client.GetAccountStatuses(id);
            mStatusAdapter = new StatusAdapter(layoutInflater, statuses);

            mListView.Adapter = mStatusAdapter;
            
            mStatusAdapter.NotifyDataSetChanged();

            
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

        /***************************************************************
         *                   スクロール
         **************************************************************/
        private void Listview_ScrollStateChanged(object sender, AbsListView.ScrollStateChangedEventArgs e)
        {
            if (mListView.LastVisiblePosition == (mListView.Count - 1))
            {
                mListView.ScrollStateChanged -= Listview_ScrollStateChanged;

                Status s = statuses[statuses.Count - 1];
                GetTLdown(s);
            }

        }
        private async Task GetTLdown(Status status)
        {
            var mstdnlist = await new UserClient().getClient().GetAccountStatuses(status.Account.Id, status.Id);
            if (mstdnlist == null)
            {
                UserAction.Toast_BottomFIllHorizontal_Show(UserAction.UNKNOWN, this.ApplicationContext, ColorDatabase.FAILED);
            }
            foreach (Status s in mstdnlist)
            {
                if (!statuses.Contains(s)) statuses.Add(s);
            }

            mStatusAdapter.NotifyDataSetChanged();
            mListView.ScrollStateChanged += Listview_ScrollStateChanged;
        }

    }

}

