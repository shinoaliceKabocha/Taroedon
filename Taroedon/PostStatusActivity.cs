using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Mastonet.Entities;
using Newtonsoft.Json;
using CoreTweet;
using Android.Graphics;

namespace Taroedon
{
    [Activity(Label = "", Theme = "@style/PostTheme", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateVisible)]
    public class PostStatusActivity : Activity, Android.Text.ITextWatcher
    {
        Mastonet.MastodonClient client = UserClient.getInstance().getClient();
        long status_id;
        List<long> media_id_list;
        static int spin_position = -1;//default
        private string KEY_RANGE = "range";

        //twitter
        private string SETTINGS = "SETTING";
        private string TWEET_RANGE = "twitterOnOff";
        private static CoreTweet.Tokens sTwiiter_tokens;
        private static bool tweet_range;

        //dialog
        delegate void YesNoDlg(string title, Android.Content.Context context, Attachment[] attachments, ImageView imageView, int i);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PostStatus);
            //Range init
            if (spin_position == -1)
            {
                //Toot range
                try
                {
                    var pref = GetSharedPreferences(KEY_RANGE, FileCreationMode.Private);
                    var p = pref.GetInt(KEY_RANGE, 0);
                    spin_position = p;
                }
                catch (IOException ioerror)
                {
                    Android.Util.Log.Error(this.ToString(), ioerror.Message);
                    spin_position = 0;
                }
                //tweet range
                try
                {
                    tweet_range = UserAction.bTwitterOnOff;
                }
                catch (IOException ioerror)
                {
                    Android.Util.Log.Error(this.ToString(), ioerror.Message);
                    tweet_range = false;
                }
            }

            //Adjust size
            IWindowManager windowManager = (IWindowManager)this.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetRealSize(point);
            Window.SetLayout((int)(point.X * 0.98), (int)(point.Y * 0.53));
            Window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Transparent));

            //Textwatcher
            var edittext = FindViewById<EditText>(Resource.Id.editTextTweet2);
            edittext.SetTextColor(ColorDatabase.TLTEXT);
            edittext.SetBackgroundColor(ColorDatabase.TL_BACK);
            edittext.AddTextChangedListener(this);


            //POST button
            var button_post = FindViewById<Button>(Resource.Id.buttonPOST);
            button_post.Text = "POST";
            //POST button event
            button_post.Click += Button_post_ClickAsync;

            //Emoji Dictionary
            var imageEmojiDictionary = FindViewById<ImageView>(Resource.Id.imageEmojiDictionary);
            imageEmojiDictionary.Click += (sender, e) =>
             {
                 Intent intent = new Intent(this, typeof(EmojiDictionaryActivity));
                 StartActivityForResult(intent, 10);
             };

            //Reply ret
            try
            {
                var _status_id = this.Intent.GetLongExtra("status_Id", 0);
                this.status_id = _status_id;

                var status_AccountName = this.Intent.GetStringExtra("status_AcountName");
                if (_status_id != 0)
                {
                    edittext.Text = "@" + status_AccountName + " ";
                    button_post.Text = "Reply";
                }

            }
            catch (System.Exception ee) { status_id = 0; }
            finally
            {
                edittext.SetSelection(edittext.Text.Length);
            }

            //twitter setting
            //token set
            var pref_twitter = GetSharedPreferences("TWITTER", FileCreationMode.Private);

            if (tweet_range && sTwiiter_tokens == null)
            {
                sTwiiter_tokens = UserAction.GetTokens(pref_twitter);
            }
            //button
            var tweetRange = FindViewById<ImageView>(Resource.Id.TweetRange);
            //resouce set
            if (tweet_range && sTwiiter_tokens != null)
            {
                tweetRange.SetImageResource(Resource.Drawable.twitter_yes);
            }
            else
            {
                tweet_range = false;
                tweetRange.SetImageResource(Resource.Drawable.twitter_no);
            }
            //event

            tweetRange.Click += (sender, e) =>
            {
                //On
                if (tweet_range)
                {
                    tweetRange.SetImageResource(Resource.Drawable.twitter_no);
                    tweet_range = false;
                }
                //No
                else
                {
                    if (sTwiiter_tokens == null)
                    {
                        sTwiiter_tokens = UserAction.GetTokens(pref_twitter);
                    }
                    if (sTwiiter_tokens != null)
                    {
                        tweetRange.SetImageResource(Resource.Drawable.twitter_yes);
                        tweet_range = true;
                    }
                    //moved twitter auth
                    else
                    {
                        Intent intent = new Intent(this, typeof(TwitterAuthActivity));
                        StartActivity(intent);
                    }

                }
            };

            //image uploader
            UploadAsyncTask.sVsDoneAttachment = new Attachment[4] { null, null, null, null };


            var image_upload_add = FindViewById<ImageView>(Resource.Id.imageuploadAdd);
            image_upload_add.Click += (sender, r) =>
            {
                int i = 0;
                for (i = 0; i < UploadAsyncTask.sVsDoneAttachment.Length; i++)
                {
                    var s = UploadAsyncTask.sVsDoneAttachment[i];
                    if (s == null) break;
                    else if (s != null && i == UploadAsyncTask.sVsDoneAttachment.Length - 1)
                    {
                        return;
                    }
                }
                Intent intent = new Intent();
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(intent, "select picture"), i);
            };

            YesNoDlg yesNoDlg = new YesNoDlg(yesNoDialog);

            var image_uploade = FindViewById<ImageView>(Resource.Id.imageupload);
            image_uploade.LongClick += (sender, e) =>
            {
                yesNoDlg("画像を消しますか", this, UploadAsyncTask.sVsDoneAttachment, (ImageView)sender, 0);
            };
            var image_uploade1 = FindViewById<ImageView>(Resource.Id.imageupload1);
            image_uploade1.LongClick += (sender, e) =>
            {
                yesNoDlg("画像を消しますか", this, UploadAsyncTask.sVsDoneAttachment, (ImageView)sender, 1);
            };
            var image_uploade2 = FindViewById<ImageView>(Resource.Id.imageupload2);
            image_uploade2.LongClick += (sender, e) =>
            {
                yesNoDlg("画像を消しますか", this, UploadAsyncTask.sVsDoneAttachment, (ImageView)sender, 2);
            };
            var image_uploade3 = FindViewById<ImageView>(Resource.Id.imageupload3);
            image_uploade3.LongClick += (sender, e) =>
            {
                yesNoDlg("画像を消しますか", this, UploadAsyncTask.sVsDoneAttachment, (ImageView)sender, 3);
            };


            //SPIN settings
            Spinner spin = FindViewById<Spinner>(Resource.Id.spinnerTootRange2);
            Bitmap public_icon = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.public_icon);
            Bitmap private_icon = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.private_icon);
            Bitmap direct_icon = BitmapFactory.DecodeResource(this.Resources, Resource.Drawable.direct);
            var spin_list2 = new List<EmojiItem>()
            {
                new EmojiItem("", public_icon),
                new EmojiItem("", private_icon),
                new EmojiItem("", direct_icon),
            };
            var spin_adapter2 = new CustomListAdapter(this, spin_list2);
            spin.Adapter = spin_adapter2;
            spin.SetSelection(spin_position);
            //saved pre position
            spin.ItemSelected += (sender, e) =>
            {
                spin_position = e.Position;
            };

        }


        /******************************
        *       post event
        *******************************/
        private async void Button_post_ClickAsync(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            var edittext = FindViewById<EditText>(Resource.Id.editTextTweet2);
            var tootRange = FindViewById<Spinner>(Resource.Id.spinnerTootRange2);
            int option_num = tootRange.SelectedItemPosition;//0:Public, 1:Private , 2:Direct,
            var option = Mastonet.Visibility.Public;
            switch (option_num)
            {
                case 0: option = Mastonet.Visibility.Public; break;     //public
                case 1: option = Mastonet.Visibility.Private; ; break;  //private
                case 2: option = Mastonet.Visibility.Direct; ; break;  //direct
                //case 3: option = Mastonet.Visibility.Unlisted; break;  //Unlisted
                default: option = Mastonet.Visibility.Public; break;
            }

            //media check
            media_id_list = new List<long>();
            media_id_list.Clear();
            foreach (Attachment at in UploadAsyncTask.sVsDoneAttachment)
            {
                if (at != null) media_id_list.Add(at.id);
            }

            //post
            if (edittext.Text.Length > 0)
            {
                var this_button = (Button)sender;
                var progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarMediaUpload);
                var emojiDic = FindViewById<ImageView>(Resource.Id.imageEmojiDictionary);
                var imageUpload = FindViewById<ImageView>(Resource.Id.imageuploadAdd);
                var tweetRange = FindViewById<ImageView>(Resource.Id.TweetRange);

                try
                {
                    /******************  function lock  ******************/
                    //send button
                    this_button.Enabled = false;
                    //progressbar
                    progressBar.Visibility = ViewStates.Visible;
                    //emojiDic
                    emojiDic.Enabled = false;
                    //imageUp
                    imageUpload.Enabled = false;
                    //twitter
                    tweetRange.Enabled = false;
                    //toot Range
                    tootRange.Enabled = false;
                    /******************  function lock  ******************/

                    //リプライする時
                    if (status_id != 0)
                    {
                        client.PostStatus(edittext.Text, option, status_id, media_id_list);
                        intent.PutExtra("reply", 1);
                        SetResult(Android.App.Result.Ok, intent);
                    }
                    //普通の投稿
                    else if (status_id == 0)
                    {
                        //post
                        var send_status = await client.PostStatus(edittext.Text, option, null, media_id_list);
                        //twitter
                        if (tweet_range)
                        {
                            if (sTwiiter_tokens != null)
                            {
                                string sendText = edittext.Text;
                                var mediaList = send_status.MediaAttachments;
                                foreach (var m in mediaList)
                                {
                                    sendText += "\n" + m.PreviewUrl;
                                }
                                if (sendText.Length <= 140)
                                {
                                    await sTwiiter_tokens.Statuses.UpdateAsync(status => sendText);
                                }
                                else
                                {
                                    UserAction.Toast_BottomFIllHorizontal_Show(
                                        "Twitterへの投稿失敗(文字数)" + sendText.Length + "/" + "140",
                                        this, ColorDatabase.FAILED);
                                }
                            }
                        }
                    }
                    edittext.Text = "";
                    UploadAsyncTask.ClearMedia();
                    
                    //toot range
                    var editor_toot = GetSharedPreferences(KEY_RANGE, FileCreationMode.Private).Edit();
                    editor_toot.PutInt(KEY_RANGE, spin_position);
                    editor_toot.Commit();

                    //tweet range
                    var editor_tweet = GetSharedPreferences(SETTINGS, FileCreationMode.Private).Edit();
                    editor_tweet.PutBoolean(TWEET_RANGE, tweet_range);
                    editor_tweet.Commit();

                    FinishAndRemoveTask();
                }
                catch (System.Exception ex)
                {
                    UserAction.Toast_BottomFIllHorizontal_Show(UserAction.UNKNOWN, this, ColorDatabase.FAILED);
                }
                finally
                {
                    /******************  function unlock  ******************/
                    //send button
                    this_button.Enabled = true;
                    //progressbar
                    progressBar.Visibility = ViewStates.Gone;
                    //emojiDic
                    emojiDic.Enabled = true;
                    //imageUp
                    imageUpload.Enabled = true;
                    //twitter
                    tweetRange.Enabled = true;
                    //toot Range
                    tootRange.Enabled = true;
                    /******************  function unlock  ******************/
                }
            }
            else
            {
                UserAction.Toast_BottomFIllHorizontal_Show(UserAction.UNKNOWN, this, ColorDatabase.FAILED);
            }
        }


        //image upload ret
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((resultCode == Result.Ok) && (data != null))
            {
                var button_post = FindViewById<Button>(Resource.Id.buttonPOST);
                button_post.Enabled = false;

                Android.Net.Uri uri = data.Data;

                //upload images
                UploadAsyncTask uploadAsyncTask = new UploadAsyncTask(this, requestCode);

                ImageView image;
                switch (requestCode)
                {
                    case 0:
                        image = FindViewById<ImageView>(Resource.Id.imageupload);
                        image.SetImageURI(uri);
                        uploadAsyncTask.Execute(uri);
                        break;
                    case 1:
                        image = FindViewById<ImageView>(Resource.Id.imageupload1);
                        image.SetImageURI(uri);
                        uploadAsyncTask.Execute(uri);
                        break;
                    case 2:
                        image = FindViewById<ImageView>(Resource.Id.imageupload2);
                        image.SetImageURI(uri);
                        uploadAsyncTask.Execute(uri);
                        break;
                    case 3:
                        image = FindViewById<ImageView>(Resource.Id.imageupload3);
                        image.SetImageURI(uri);
                        uploadAsyncTask.Execute(uri);
                        break;
                    case 10:
                        //shortcode emoji
                        try
                        {
                            var shortcode = data.GetStringExtra("shortcode");
                            var edittext = FindViewById<EditText>(Resource.Id.editTextTweet2);

                            int insert = edittext.SelectionStart;
                            string left = edittext.Text.Substring(0, insert);
                            string right = edittext.Text.Substring(left.Length, edittext.Text.Length - left.Length);

                            edittext.Text = left + " :" + shortcode + ": " + right;
                            edittext.SetSelection(left.Length + shortcode.Length + 4);
                        }
                        catch (System.Exception e)
                        {
                            UserAction.Toast_BottomFIllHorizontal_Show(UserAction.UNKNOWN, this, ColorDatabase.FAILED);
                        }
                        finally
                        {
                            button_post.Enabled = true;
                        }
                        break;

                    default: break;
                }

            }
        }

        //image reset
        private void UploadImageReset(Attachment[] attachments, ImageView imageView, int i)
        {
            attachments[i] = null;
            imageView.Visibility = ViewStates.Gone;
        }

        //image dialog
        private void yesNoDialog(string title, Android.Content.Context context, Attachment[] attachments, ImageView imageView, int i)
        {
            ContextThemeWrapper contextThemeWrapper;
            if (ColorDatabase.mode) contextThemeWrapper = new ContextThemeWrapper(this, Resource.Style.DarkDialogTheme);
            else contextThemeWrapper = new ContextThemeWrapper(this, Android.Resource.Style.ThemeMaterialLightDialog);

            var dlg = new AlertDialog.Builder(contextThemeWrapper);
            dlg.SetTitle(title);
            dlg.SetPositiveButton("Yes", (sender, e) =>
            {
                UploadImageReset(attachments, imageView, i);
            });
            dlg.SetNegativeButton("Cancel", (sender, e) => { });
            dlg.Create().Show();
        }


        //text watcher
        public void AfterTextChanged(IEditable s)
        {
            int count = s.ToString().Length;
            this.FindViewById<TextView>(Resource.Id.textview_wordsCount).Text = count + "/500 words";

            if (count > 500)
            {
                this.FindViewById<Button>(Resource.Id.buttonPOST).Clickable = false;
                this.FindViewById<EditText>(Resource.Id.editTextTweet2).SetTextColor(Android.Graphics.Color.Red);
            }
            else
            {
                this.FindViewById<Button>(Resource.Id.buttonPOST).Clickable = true;
                this.FindViewById<EditText>(Resource.Id.editTextTweet2).SetTextColor(ColorDatabase.TLTEXT);
            }
        }
        public void BeforeTextChanged(ICharSequence s, int start, int count, int after) { }
        public void OnTextChanged(ICharSequence s, int start, int before, int count) { }


        //dialog
        public override void Finish()
        {
            ContextThemeWrapper contextThemeWrapper;
            if (ColorDatabase.mode) contextThemeWrapper = new ContextThemeWrapper(this, Resource.Style.DarkDialogTheme);
            else contextThemeWrapper = new ContextThemeWrapper(this, Android.Resource.Style.ThemeMaterialLightDialog);

            var dlg = new AlertDialog.Builder(contextThemeWrapper);
            dlg.SetTitle("投稿を中止しますか？");
            dlg.SetMessage("データは保存されません");
            dlg.SetPositiveButton("Yes", (sender, e) =>
            {
                base.Finish();
            });
            dlg.SetNegativeButton("Cancel", (sender, e) => { });

            int editlength = FindViewById<EditText>(Resource.Id.editTextTweet2).Length();
            var str = Intent.GetStringExtra("status_AcountName");
            //imageがあるとき
            if (UploadAsyncTask.isMedia())
            {
                dlg.Create().Show();
            }
            //通常Post
            else if (string.IsNullOrEmpty(str) && editlength > 0)
            {
                dlg.Create().Show();
            }
            //reply
            else if(!string.IsNullOrEmpty(str) && editlength > str.Length + 2 /*@+space*/)
            {
                dlg.Create().Show();
            }
            else
            {
                base.Finish();
            }
        }
    }
}