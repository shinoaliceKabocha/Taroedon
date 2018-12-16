using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Graphics;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Mastonet.Entities;
using Android.Content.PM;
using Android.Util;
using Context = Android.Content.Context;
using Android.Support.CustomTabs;
using System.IO;
using Newtonsoft.Json;

namespace FlashCardPager
{
    public static class UserAction
    {
        public static bool bBrowser = true;
        public static bool bDisplay =true;
        public static bool bImagePre = true;
        public static bool bImageQuality = false;

        /*******************************************************
         *      トースト 文章
         * *****************************************************/
        public readonly static string FAV_SUCCESS = "ふぁぼりました";
        public readonly static string FAV_FAILED = "ふぁぼに失敗しました";
        public readonly static string UNFAV = "愛が深すぎる．．．（ふぁぼ済み）";

        public readonly static string BOOST_SUCCESS = "ぶーすとしました";
        public readonly static string BOOST_FAILED = "ぶーすとに失敗しました";
        public readonly static string UNBOOT = "愛が深すぎる．．．（ぶーすと済み）";

        public readonly static string URL_FAILED = "URLの取得に失敗しました";
        public readonly static string UNKNOWN = "何かがおかしいよ";

        public readonly static string KEY_COMMAND = "commands";

        /**************************************************************
         *                              設定
         *************************************************************/
        public static void SettingsLoad(ISharedPreferences pref)
        {
            //現在の設定の読み込み
            bBrowser = pref.GetBoolean("browser", true);
            bDisplay = pref.GetBoolean("display", true);
            bImagePre = pref.GetBoolean("imagePre", true);
            bImageQuality = pref.GetBoolean("imageQuality", false);
            //bTheme = pref.GetBoolean("theme", false);//外出ししないとだめ
        }


        /***************************************************************
         *                             操作 Fav
         **************************************************************/
        private static async void FavAsync(Status status, View view)
        {
            Mastonet.MastodonClient clientfav = new UserClient().getClient();
            try
            {
                await clientfav.Favourite(status.Id);
                status.Favourited = true;
                status.FavouritesCount++;

                string s = OtherTool.HTML_removeTag(status.Content);
                ToastWithIcon_BottomFIllHorizontal_Show(FAV_SUCCESS+"\n"+s, status.Account.StaticAvatarUrl, view, ColorDatabase.FAV);
            }
            catch (System.Exception ex)
            {
                Toast_BottomFIllHorizontal_Show(FAV_FAILED, view.Context, ColorDatabase.FAILED);
            }
        }

        public static void Fav(Status status, View view)
        {
            if (status.Favourited.Equals(true))
            {
                Toast_BottomFIllHorizontal_Show(UNFAV, view.Context, ColorDatabase.FAILED);
            }
            else
            {
                UserAction.FavAsync(status, view);
            }
        }

        /***************************************************************
        *                             操作 Boost
        **************************************************************/
 
        private static async void BoostAsync(Status status, View view)
        {
            Mastonet.MastodonClient clientReb = new UserClient().getClient();
            try
            {
                await clientReb.Reblog(status.Id);
                status.Reblogged = true;
                status.ReblogCount++;

                string s = OtherTool.HTML_removeTag(status.Content);
                ToastWithIcon_BottomFIllHorizontal_Show(BOOST_SUCCESS + "\n" +s, status.Account.StaticAvatarUrl,
                    view, ColorDatabase.BOOST);
            }
            catch (System.Exception ex)
            {
                Toast_BottomFIllHorizontal_Show(BOOST_FAILED, view.Context, ColorDatabase.FAILED);
            }
        }

        public static void Boost(Status status, View view)
        {
            if (status.Reblogged.Equals(true))
            {
                Toast_BottomFIllHorizontal_Show(UNBOOT, view.Context, ColorDatabase.FAILED);
            }
            else
            {
                BoostAsync(status, view);
            }

        }


        /***************************************************************
        *                             操作 url open
        **************************************************************/

        public static void UrlOpen(string item_url, View view)
        {
            try
            {
                if (bBrowser)
                {
                    ////内部ブラウザ
                    UrlOpenCustomChrome(item_url, view);
                }
                else
                {
                    UrlOpenChrome(item_url, view);
                }
            }
            catch (Exception ex)
            {
                Toast_BottomFIllHorizontal_Show(URL_FAILED, view.Context, ColorDatabase.FAILED);
            }
        }

        public static void UrlOpenChrome(string item_url, View view)
        {
            try
            {
                //外部ブラウザ
                Android.Net.Uri uri;
                uri = Android.Net.Uri.Parse(item_url);
                Intent intentB = new Android.Content.Intent(Intent.ActionView, uri);
                view.Context.StartActivity(intentB);
            }
            catch (Exception ex)
            {
                Toast_BottomFIllHorizontal_Show(URL_FAILED, view.Context, ColorDatabase.FAILED);
            }
        }

        private static void UrlOpenCustomChrome(string url, View view)
        {
            try
            {
                var builder = new CustomTabsIntent.Builder();
                builder.SetToolbarColor(Resource.Color.colorPrimaryDark);
                var chromeIntent = builder.Build();
                chromeIntent.LaunchUrl(view.Context, Android.Net.Uri.Parse(url));
                //view.Context.StartActivity(chromeIntent.Intent);
            }
            catch (Exception ex)
            {
                Toast_BottomFIllHorizontal_Show(URL_FAILED, view.Context, ColorDatabase.FAILED);
            }
        }


        /***************************************************************
        *                             操作 reply
        **************************************************************/

        public static void Reply(Status status, View view)
        {
            //intent -> PostStatusActivity(intent) 
            var intent = new Intent(view.Context, typeof(PostStatusActivity));
            intent.PutExtra("status_Id", status.Id);
            intent.PutExtra("status_AcountName", status.Account.AccountName);
            view.Context.StartActivity(intent);
        }


        /***************************************************************
        *                             操作 listitem
        **************************************************************/
        public static void ListViewItemClick(Status status, View view)
        {
            Intent intent = new Intent(view.Context, typeof(DialogActivity));
            Status _status = status;

            //コマンドセットのPush
            string SHOW_THE_CONVERSATION = "会話を表示";
            try
            {
                var re_status = status.Reblog;
                if (re_status != null) _status = re_status;
            }
            catch (Exception ex) { }
            //URL をitemlistに一時登録
            List<string> itemlist
                = new List<string>() { "Reply", "Favarite", "Boost", (_status.Account.DisplayName + "@" + _status.Account.UserName) };
            ////追加
            if (status.InReplyToId != null)
            {
                itemlist.Add(SHOW_THE_CONVERSATION);
            }
            //URL，画像の取得
            foreach (string add in OtherTool.DLG_ITEM_getURL(_status))
            {
                itemlist.Add(add);
            }
            var commands = itemlist.ToArray();
            intent.PutStringArrayListExtra(KEY_COMMAND, commands);



            var json_status = IntentExtensionStatus.PutExtra<Status>(intent, "status", _status);

            view.Context.StartActivity(intent);
        }


        /* old method */
        public static void ListViewItemClick2(Status status, View view)
        {
            string SHOW_THE_CONVERSATION = "会話を表示";

            int select;
            try
            {
                var re_status = status.Reblog;
                if (re_status != null) status = re_status;
            }
            catch (Exception ex) { }

            //URL をitemlistに一時登録
            List<string> itemlist
                = new List<string>() { "Reply", "Favarite", "Boost", (status.Account.DisplayName + "@" + status.Account.UserName) };

            ////追加
            if(status.InReplyToId != null)
            {
                itemlist.Add(SHOW_THE_CONVERSATION);
            }


            //URL，画像の取得
            foreach (string add in OtherTool.DLG_ITEM_getURL(status))
            {
                itemlist.Add(add);
            }
            //格納用に変換する
            string[] items = itemlist.ToArray();

            try
            {
                var dlg = new Android.App.AlertDialog.Builder(view.Context);
                dlg.SetTitle(status.Account.DisplayName + "@" + status.Account.UserName
                    + "さん\r\n" + OtherTool.HTML_removeTag(status.Content));
                dlg.SetItems(items, (s, ee) =>
               {
                   select = ee.Which;
                   switch (select)
                   {
                       case 0://reply
                            UserAction.Reply(status, view);
                           break;

                       case 1://fav
                            UserAction.Fav(status, view);
                           break;

                       case 2://boost
                            UserAction.Boost(status, view);
                           break;

                       case 3://USER
                            Profile(status.Account, view.Context);
                           break;

                       default://urls
                           string urlOrAccountName = items[select];

                           //会話を表示する
                           if (urlOrAccountName.Equals(SHOW_THE_CONVERSATION))
                           {
                               Intent intent = new Intent(view.Context, typeof(ConversationActivity));
                               intent.PutExtra("statusId", status.Id);
                               view.Context.StartActivity(intent);
                               break;
                           }

                           //urls
                           UserAction.UrlOpen(urlOrAccountName, view);
                           break;
                   }

               });
                dlg.Create().Show();
            }
            catch (Exception ex)
            {
                Toast_BottomFIllHorizontal_Show(URL_FAILED, view.Context, ColorDatabase.FAILED);
            }
        }

        /*
        * profile open
        * @param Account
        * @param view
        */
        public static void Profile(Mastonet.Entities.Account account, Context context)
        {
            Intent intent = new Intent(context, typeof(ProfileActivity));
            intent.PutExtra("header", account.StaticHeaderUrl);
            intent.PutExtra("avatar", account.AvatarUrl);
            intent.PutExtra("display_name", account.DisplayName);
            intent.PutExtra("account_name", account.AccountName);
            intent.PutExtra("note", account.Note);
            intent.PutExtra("follow", account.FollowingCount.ToString());
            intent.PutExtra("follower", account.FollowersCount.ToString());
            intent.PutExtra("id", account.Id);
            intent.PutExtra("lock", account.Locked);
            context.StartActivity(intent);
        }



        //Cacheクリア
        public static void CacheClear()
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            if (Directory.Exists(System.IO.Path.Combine(path, "Emoji")))
            {
                string[] filePaths = Directory.GetFiles(System.IO.Path.Combine(path, "Emoji"));
                foreach (string file in filePaths)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }
            if (Directory.Exists(System.IO.Path.Combine(path, "Emoji")))
            {
                Directory.Delete(System.IO.Path.Combine(path, "Emoji"), false);
            }
            if (Directory.Exists(path))
            {
                string[] filePaths = Directory.GetFiles(path);
                foreach (string file in filePaths)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }
        }

        //Custom Toast
        public static void Toast_BottomFIllHorizontal_Show(string str, Context context, Color color)
        {
            var toast = Toast.MakeText(context, str, ToastLength.Short);
            toast.SetGravity(GravityFlags.FillHorizontal | GravityFlags.Bottom, 0, 0);
            toast.View.SetBackgroundColor(color);

            TextView textView = toast.View.FindViewById<TextView>(Android.Resource.Id.Message);
            if (color.Equals(ColorDatabase.INFO))
            {
                textView.SetTextColor(Color.White);
            }
            textView.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);

            toast.Show();
        }

        public static void Toast_TopFIllHorizontal_Show(string str, Context context, Color color)
        {
            var toast = Toast.MakeText(context, str, ToastLength.Long);
            toast.SetGravity(GravityFlags.FillHorizontal | GravityFlags.Top, 0, 0);
            toast.View.SetBackgroundColor(color);

            TextView textView = toast.View.FindViewById<TextView>(Android.Resource.Id.Message);
            textView.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);

            toast.Show();
        }

        //with image
        public static void ToastWithIcon_BottomFIllHorizontal_Show(string str, string imageUrl, View view, Color color)
        {
            LayoutInflater layoutinflater = (LayoutInflater)view.Context.GetSystemService(Context.LayoutInflaterService);
            View layout = layoutinflater.Inflate(Resource.Layout.toastlayout, (ViewGroup)view.FindViewById(Resource.Layout.toastlayout), true);

            layout.SetBackgroundColor(color);
            var textView = layout.FindViewById<TextView>(Resource.Id.textViewToast);
            textView.Text = str;
            if (color.Equals(ColorDatabase.INFO))
            {
                textView.SetTextColor(Color.White);
            }
            textView.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);
            var image = layout.FindViewById<ImageView>(Resource.Id.imageViewToast);
            new ImageProvider().ImageIconSetAsync(imageUrl, image);

            var toast = Toast.MakeText(view.Context, str, ToastLength.Long);
            toast.View = layout;
            toast.SetGravity(GravityFlags.FillHorizontal | GravityFlags.Bottom, 0, 0);

            toast.Show();
        }

        public static void ToastWithIcon_TopFIllHorizontal_Show(string str, string imageUrl, View view, Color color)
        {
            LayoutInflater layoutinflater = (LayoutInflater)view.Context.GetSystemService(Context.LayoutInflaterService);
            View layout = layoutinflater.Inflate(Resource.Layout.toastlayout, (ViewGroup)view.FindViewById(Resource.Layout.toastlayout), true);

            layout.SetBackgroundColor(color);
            var textView = layout.FindViewById<TextView>(Resource.Id.textViewToast);
            textView.Text = str;
            if (color.Equals(ColorDatabase.INFO))
            {
                textView.SetTextColor(Color.White);
            }
            textView.SetTypeface(Typeface.DefaultBold, TypefaceStyle.Bold);
            var image = layout.FindViewById<ImageView>(Resource.Id.imageViewToast);
            new ImageProvider().ImageIconSetAsync(imageUrl, image);

            var toast = Toast.MakeText(view.Context, str, ToastLength.Long);
            toast.View = layout;
            toast.SetGravity(GravityFlags.FillHorizontal | GravityFlags.Top, 0, 0);

            toast.Show();
        }



    }


    //Json変換
    public static class IntentExtensionStatus
    {
        public static Intent PutExtra<Status>(Intent intent, string key, Status status)
        {
            var json = JsonConvert.SerializeObject(status);
            intent.PutExtra(key, json);
            return intent;
        }

        public static Status GetExtra<Status>(Intent intent, string key)
        {
            var json = intent.GetStringExtra(key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(Status);
            }
            return JsonConvert.DeserializeObject<Status>(json);
        }
    }



    /*non-use class*/
    public class MstWebViewClient : WebViewClient
    {
        // For API level 24 and later
        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            view.LoadUrl(request.Url.ToString());
            return false;
        }

        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            view.LoadUrl(url);
            return false;
        }

        public override void OnReceivedError(WebView view, [GeneratedEnum] ClientError errorCode, string description, string failingUrl)
        {
            Toast.MakeText(view.Context, "URL エラー", ToastLength.Short).Show();
        }
    }
}