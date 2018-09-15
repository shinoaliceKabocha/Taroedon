using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace FlashCardPager
{
    [Activity(Label = "", Theme = "@android:style/Theme.Material.Light.Dialog", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateVisible)]
    public class PostStatusActivity : Activity, Android.Text.ITextWatcher
    {
        Mastonet.MastodonClient client = new UserClient().getClient();
        long status_id;
        List<long> media_id_list;
        static int spin_position = 0;
        private Android.Net.Uri uploadUri = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PostStatus);
            //いい感じの大きさにする
            IWindowManager windowManager = (IWindowManager)this.GetSystemService(Android.Content.Context.WindowService).JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetRealSize(point);
            Window.SetLayout((int)(point.X * 0.98), (int)(point.Y * 0.55));
            Window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Transparent));

            //Edittext にTextwatcherを仕込む
            var edittext = FindViewById<EditText>(Resource.Id.editTextTweet2);
            edittext.AddTextChangedListener(this);

            //POST button
            var button_post = FindViewById<Button>(Resource.Id.buttonPOST);
            button_post.Text = "POST";

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

            FindViewById<ImageView>(Resource.Id.imageupload).Visibility = ViewStates.Gone;
            /****************************************************
            //image uploader
            media_id_list = new List<long>();
            var image_uploade = FindViewById<ImageView>(Resource.Id.imageupload);
            image_uploade.Click += (sender, e) =>
            {
                Image_uploade_Click(sender, e);

                BackgroundWorker imageWorker = new BackgroundWorker();
                imageWorker.DoWork += Worker_DoWork;
                imageWorker.RunWorkerCompleted += Worker_RunWorkerCompletedAsync;
                imageWorker.RunWorkerAsync();
            };
            //image_uploade.Click += Image_uploade_Click;
            //image_uploade.Visibility = ViewStates.Gone;
            /*
             BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync();*/
            //****************************************************/

            //SPIN settings
            Spinner spin = FindViewById<Spinner>(Resource.Id.spinnerTootRange);
            spin.Background = button_post.Background;

            ArrayAdapter spin_adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem);
            List<string> spin_list = new List<string>() { "1. Public", "2. Private", "3. Direct", "4. Unlistrd" };
            spin_adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleDropDownItem1Line);
            spin_adapter.AddAll(spin_list);
            spin.Adapter = spin_adapter;
            spin.SetSelection(spin_position);
            //前回の位置を保存する
            spin.ItemSelected += (sender, e) =>
            {
                spin_position = e.Position;
            };


            //POST button event
            button_post.Click += Button_post_Click;

        }


        /******************************
        *       post event
        *******************************/
        private void Button_post_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            var edittext = FindViewById<EditText>(Resource.Id.editTextTweet2);
            int option_num = FindViewById<Spinner>(Resource.Id.spinnerTootRange).SelectedItemPosition;//0:Public, 1:Private , 2:Direct, 3:Ubnlited
            var option = Mastonet.Visibility.Public;
            switch (option_num)
            {
                case 0: option = Mastonet.Visibility.Public; break;     //public
                case 1: option = Mastonet.Visibility.Private; ; break;  //private
                case 2: option = Mastonet.Visibility.Direct; ; break;  //direct
                case 3: option = Mastonet.Visibility.Unlisted; break;  //unlimited
            }

            if (edittext.Text.Length > 0)
            {
                try
                {
                    //リプライする時
                    if (status_id != 0)
                    {
                        client.PostStatus(edittext.Text, option, status_id);
                        intent.PutExtra("reply", 1);
                        SetResult(Android.App.Result.Ok, intent);
                    }
                    //普通の投稿
                    else if (status_id == 0)
                    {
                        //post
                        client.PostStatus(edittext.Text, option);
                    }
                    edittext.Text = "";
                    Finish();
                }
                catch (System.Exception ex)
                {
                    var ts = Toast.MakeText(this, "Posted fail", ToastLength.Short);
                    ts.SetGravity(GravityFlags.Center, 0, 0);
                    ts.Show();
                }
            }
            else
            {
                var ts = Toast.MakeText(this, "Posted fail", ToastLength.Short);
                ts.SetGravity(GravityFlags.Center, 0, 0);
                ts.Show();
            }
        }


        /*
        //image upload
        int PickImageId = 1000;
        private void Image_uploade_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "select picture"), PickImageId);
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                ImageView image = FindViewById<ImageView>(Resource.Id.imageupload);
                image.SetImageURI(uri);

                this.uploadUri = uri;
                //media_id_list.Insert(0,Image_Upload_getimageID(uri) ) ;//image id get
                //media_id_list.Add(Image_Upload_getimageID(uri) );//image id get
                //var get_attach =  Image_Upload_getimageID(uri);
                
                //media_id_list.Add(get_attach.Id);
            }
        }
        //this method 廃止予定
        private Attachment Image_Upload_getimageID(Android.Net.Uri uri)
        {
            Task<Attachment> media = null;
            Attachment rtn = null;
            try
            {
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                Android.Graphics.Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, uri);
                bitmap = Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, 200, 200, false);//低画質
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);//->byte

                media = client.UploadMedia(memoryStream);
                rtn = media.Result;
            }
            catch (Java.Lang.Exception e)
            {
                Android.Util.Log.Debug("bitmap image", e.StackTrace);
            }
            catch (System.Exception e)
            {
                Android.Util.Log.Debug("bitmap image", e.StackTrace);
            }

            return rtn;
        }

        private async void Worker_RunWorkerCompletedAsync(object sender, RunWorkerCompletedEventArgs e)
        {
            //Task<Attachment> media = null;
            Attachment rtn = null;
            try
            {
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                Android.Graphics.Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, uploadUri);
                bitmap = Android.Graphics.Bitmap.CreateScaledBitmap(bitmap, 200, 200, false);//低画質
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, memoryStream);//->byte

                rtn =await  client.UploadMedia(memoryStream);
                //rtn = media.Result;

                Toast.MakeText(this, "OK uploaded", ToastLength.Short).Show();
                
            }
            catch (Java.Lang.Exception ex)
            {
                Android.Util.Log.Debug("hoge *********", ex.Message);
            }
            catch (System.Exception)
            {
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }
        */




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