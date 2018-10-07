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
    /****************************/
    //     Image upload task
    /****************************/
    public class UploadAsyncTask : AsyncTask<Android.Net.Uri, Android.Net.Uri, Attachment>
    {
        private Activity activity;
        private int requestcode;
        public static Attachment[] sVsDoneAttachment = new Attachment[4] { null, null, null, null };

        //ProgressBar
        private ProgressBar progressBar;
        //コンストラクタ
        public UploadAsyncTask(Activity activity, int requestcode)
        {
            this.activity = activity;
            this.requestcode = requestcode;
            progressBar = activity.FindViewById<ProgressBar>(Resource.Id.progressBarMediaUpload);
        }

        //バックグラウンド処理開始前の処理
        protected override void OnPreExecute()
        {
            activity.FindViewById<Button>(Resource.Id.buttonPOST).Enabled = false;
            progressBar.Visibility = ViewStates.Visible;
            //sDoneAttachment = null;//error
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
            //Toast.MakeText(activity, "アップロード完了", ToastLength.Short).Show();
            activity.FindViewById<Button>(Resource.Id.buttonPOST).Enabled = true;
            sVsDoneAttachment[requestcode] = result;

            switch (requestcode)
            {
                case 0:
                    activity.FindViewById<ImageView>(Resource.Id.imageupload1).Visibility = ViewStates.Visible;
                    break;
                case 1:
                    activity.FindViewById<ImageView>(Resource.Id.imageupload2).Visibility = ViewStates.Visible;
                    break;
                case 2:
                    activity.FindViewById<ImageView>(Resource.Id.imageupload3).Visibility = ViewStates.Visible;
                    break;
            }
            progressBar.Visibility = ViewStates.Gone;
        }


        ////独自アップローダー
        public async Task<string> UploadMedia(byte[] image)
        {
            var client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler())
            //var client = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = new Uri($"https://" + UserClient.instance)
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