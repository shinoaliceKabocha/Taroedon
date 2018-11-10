﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FlashCardPager
{
    [Activity(Label = "", Theme = "@style/PostTheme", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateVisible)]
    public class PostStatusActivity : Activity, Android.Text.ITextWatcher
    {
        Mastonet.MastodonClient client = new UserClient().getClient();
        long status_id;
        List<long> media_id_list;
        static int spin_position = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PostStatus);
            //いい感じの大きさにする
            IWindowManager windowManager = (IWindowManager)this.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetRealSize(point);
            Window.SetLayout((int)(point.X * 0.98), (int)(point.Y * 0.53));
            Window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Transparent));

            //Edittext にTextwatcherを仕込む
            var edittext = FindViewById<EditText>(Resource.Id.editTextTweet2);
            edittext.AddTextChangedListener(this);


            //POST button
            var button_post = FindViewById<Button>(Resource.Id.buttonPOST);
            button_post.Text = "POST";
            //POST button event
            button_post.Click += Button_post_Click;

            //Emoji Dictionary
            var imageEmojiDictionary = FindViewById<ImageView>(Resource.Id.imageEmojiDictionary);
            imageEmojiDictionary.Click +=(sender, e) =>
            {
                Intent intent = new Intent(this, typeof(EmojiDictionaryActivity));
                StartActivityForResult(intent, 10);
            };


            //Reply時のデータの受取
            try
            {
                var _status_id = this.Intent.GetLongExtra("status_Id", 0);
                this.status_id = _status_id;

                var status_AccountName = this.Intent.GetStringExtra("status_AcountName");
                if (_status_id != 0)
                {
                    edittext.Text = "@" + status_AccountName + " ";
                    button_post.Text = "Reply POST";
                }

            }
            catch (System.Exception ee) { status_id = 0; }
            finally
            {
                edittext.SetSelection(edittext.Text.Length);
            }

            //FindViewById<ImageView>(Resource.Id.imageupload).Visibility = ViewStates.Gone;
            /****************************************************/
            //image uploader
            UploadAsyncTask.sVsDoneAttachment = new Attachment[4] { null, null, null, null };

            media_id_list = new List<long>();
            var image_uploade = FindViewById<ImageView>(Resource.Id.imageupload);
            image_uploade.Click += (sender, e) =>
            {
                Intent intent = new Intent();
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(intent, "select picture"), 0);
            };
            var image_uploade1 = FindViewById<ImageView>(Resource.Id.imageupload1);
            image_uploade1.Click += (sender, e) =>
            {
                Intent intent = new Intent();
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(intent, "select picture"), 1);
            };
            var image_uploade2 = FindViewById<ImageView>(Resource.Id.imageupload2);
            image_uploade2.Click += (sender, e) =>
            {
                Intent intent = new Intent();
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(intent, "select picture"), 2);
            };
            var image_uploade3 = FindViewById<ImageView>(Resource.Id.imageupload3);
            image_uploade3.Click += (sender, e) =>
            {
                Intent intent = new Intent();
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(intent, "select picture"), 3);
            };
            //****************************************************/

            //SPIN settings
            Spinner spin = FindViewById<Spinner>(Resource.Id.spinnerTootRange);
            //spin.Background = button_post.Background;

            ArrayAdapter spin_adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem);
            List<string> spin_list = new List<string>() { "1. Public", "2. Private", "3. Direct", "4. Unlisted" };
            spin_adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleDropDownItem1Line);
            spin_adapter.AddAll(spin_list);
            spin.Adapter = spin_adapter;
            spin.SetSelection(spin_position);
            //前回の位置を保存する
            spin.ItemSelected += (sender, e) =>
            {
                spin_position = e.Position;
            };

        }


        /******************************
        *       post event
        *******************************/
        private void Button_post_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            var edittext = FindViewById<EditText>(Resource.Id.editTextTweet2);
            int option_num = FindViewById<Spinner>(Resource.Id.spinnerTootRange).SelectedItemPosition;//0:Public, 1:Private , 2:Direct, 3:Unlisted
            var option = Mastonet.Visibility.Public;
            switch (option_num)
            {
                case 0: option = Mastonet.Visibility.Public; break;     //public
                case 1: option = Mastonet.Visibility.Private; ; break;  //private
                case 2: option = Mastonet.Visibility.Direct; ; break;  //direct
                case 3: option = Mastonet.Visibility.Unlisted; break;  //Unlisted
            }

            //media check
            media_id_list.Clear();
            foreach(Attachment at in UploadAsyncTask.sVsDoneAttachment)
            {
                if(at != null)  media_id_list.Add(at.id);
            }

            //投稿
            if (edittext.Text.Length > 0)
            {
                try
                {
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
                        client.PostStatus(edittext.Text, option, null, media_id_list);
                    }
                    edittext.Text = "";
                    Finish();
                }
                catch (System.Exception ex)
                {
                    var ts = Toast.MakeText(this, "なにかがおかしいよ", ToastLength.Short);
                    ts.SetGravity(GravityFlags.Center, 0, 0);
                    ts.Show();
                }
            }
            else
            {
                var ts = Toast.MakeText(this, "なにかがおかしいよ", ToastLength.Short);
                ts.SetGravity(GravityFlags.Center, 0, 0);
                ts.Show();
            }
        }


        //image upload 結果の受け取り（アクティビティからの）
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((resultCode == Result.Ok) && (data != null))
            {
                //Toast.MakeText(this, "upload now...", ToastLength.Short).Show();
                var button_post = FindViewById<Button>(Resource.Id.buttonPOST);
                button_post.Enabled = false;

                Android.Net.Uri uri = data.Data;

                //画像のアップロード
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
                        catch(System.Exception e)
                        {
                            Toast.MakeText(this, e.Message, ToastLength.Short).Show();
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


        //text watcher
        public void AfterTextChanged(IEditable s)
        {
            int count = s.ToString().Length;
            this.FindViewById<TextView>(Resource.Id.textview_wordsCount).Text = count + "/500 words";

            if (count > 500)
            {
                this.FindViewById<Button>(Resource.Id.buttonPOST).Clickable = false;
            }
            else this.FindViewById<Button>(Resource.Id.buttonPOST).Clickable = true;

        }
        public void BeforeTextChanged(ICharSequence s, int start, int count, int after) { }
        public void OnTextChanged(ICharSequence s, int start, int before, int count) { }
    }
}