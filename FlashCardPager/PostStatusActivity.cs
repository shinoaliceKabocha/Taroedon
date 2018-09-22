using System;
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
    [Activity(Label = "", Theme = "@android:style/Theme.Material.Light.Dialog", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateVisible)]
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

            //FindViewById<ImageView>(Resource.Id.imageupload).Visibility = ViewStates.Gone;
            /****************************************************/
            //image uploader
            media_id_list = new List<long>();
            var image_uploade = FindViewById<ImageView>(Resource.Id.imageupload);
            image_uploade.Click += (sender, e) =>
            {
                Intent intent = new Intent();
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(intent, "select picture"), 0);
            };
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

            //media check
            if(UploadAsyncTask.sDoneAttachment != null)
            {
                media_id_list.Clear();//TODO:マルチ投稿するときは，外す．
                media_id_list.Add(UploadAsyncTask.sDoneAttachment.id);
            }

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


        //image upload 結果の受け取り（アクティビティからの）
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == 0) && (resultCode == Result.Ok) && (data != null))
            {
                Toast.MakeText(this, "upload now...", ToastLength.Short).Show();
                var button_post = FindViewById<Button>(Resource.Id.buttonPOST);
                button_post.Enabled = false;
                
                Android.Net.Uri uri = data.Data;
                ImageView image = FindViewById<ImageView>(Resource.Id.imageupload);
                image.SetImageURI(uri);
                //画像のアップロード
                Thread.Sleep(10);
                UploadAsyncTask uploadAsyncTask = new UploadAsyncTask(this);
                uploadAsyncTask.Execute(uri);
                ////投稿準備
                //media_id_list.Clear();//TODO:マルチ投稿するときは，外す．
                //try
                //{
                //    media_id_list.Add(uploadAsyncTask.GetResult().id);
                //}
                //catch(NullPointerException nullpo)
                //{
                //    Toast.MakeText(this, "おや？何かがおかしいよ", ToastLength.Short).Show();
                //}
                //finally
                //{
                //    button_post.Enabled = true;
                //}

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

    /****************************/
    //     Image upload task
    /****************************/
    public class UploadAsyncTask : AsyncTask<Android.Net.Uri, Android.Net.Uri, Attachment>
    {
        private Activity activity;
        public static Attachment sDoneAttachment;

        public UploadAsyncTask(Activity activity)
        {
            this.activity = activity;
        }

        //バックグラウンド処理開始前の処理
        protected override void OnPreExecute()
        {
            activity.FindViewById<Button>(Resource.Id.buttonPOST).Enabled = false;
            sDoneAttachment = null;//error
        }

        //バックグラウンド処理
        protected override Attachment RunInBackground(params Android.Net.Uri[] @params)
        {
            try
            {
                Android.Net.Uri uploadUri = @params[0];
                //Uri からビットマップの生成→圧縮→byte[]化
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
                Android.Graphics.Bitmap bitmap = MediaStore.Images.Media.GetBitmap(activity.ContentResolver, uploadUri);
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 85, memoryStream);//->byte
                var bytedata = memoryStream.ToArray();

                var uploadTask = UploadMedia(bytedata);
                uploadTask.Wait();
                var jsonStylUploadResult = uploadTask.Result;
                Android.Util.Log.Info("", jsonStylUploadResult);

                Attachment attachment =
                    JsonConvert.DeserializeObject<Attachment>(jsonStylUploadResult,
                        new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

                return attachment;
            }
            catch (System.Exception e)
            {
                return null;
            }
        }
        ////バックグラウンドのUI側処理
        protected override void OnProgressUpdate(params Android.Net.Uri[] values) { }

        //終了処理
        protected override void OnPostExecute(Attachment result)
        {
            Toast.MakeText(activity, "upload complated", ToastLength.Short).Show();
            activity.FindViewById<Button>(Resource.Id.buttonPOST).Enabled = true;

            sDoneAttachment = result;
        }



        ////独自アップローダー
        public async Task<string> UploadMedia(byte[] image)
        {
            var client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler())
            //var client = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri($"https://"+UserClient.instance)
            };
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", UserClient.accessToken);

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(image), "file", "file");

            var response = await client.PostAsync("/api/v1/media", content).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

    }

    //独自 media class
    [JsonObject("media")]
    public class Attachment
    {
        [JsonProperty("id")]
        public long id { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("url")]
        public string url { get; set; }
        [JsonProperty("preview_url")]
        public string preview_url { get; set; }
        [JsonProperty("remote_url")]
        public string remote_url { get; set; }
        [JsonProperty("text_url")]
        public string text_url { get; set; }

        /*形式は以下の通り．した２つは飛ばす．
        {
        "id":"2503",
        "type":"image",
        "url":"https://taroedon.com/system/media_attachments/files/000/002/503/original/7209e2502ead12be.png?1537629234",
        "preview_url":"https://taroedon.com/system/media_attachments/files/000/002/503/small/7209e2502ead12be.png?1537629234",
        "remote_url":null,
        "text_url":"https://taroedon.com/media/Kzqmc0Ef5bvC4ftqXtk",
        "meta":{"original":{"width":312,"height":416,"size":"312x416","aspect":0.75},
        "small":{"width":312,"height":416,"size":"312x416","aspect":0.75}},"description":null
        }*/

    }

}