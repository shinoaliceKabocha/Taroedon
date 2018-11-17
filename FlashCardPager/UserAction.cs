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
using Context = Android.Content.Context;
using Android.Support.CustomTabs;

namespace FlashCardPager
{
    public static class UserAction
    {
        public static bool bBrowser = true;
        public static bool bDisplay =true;
        public static bool bImagePre = true;
        public static bool bImageQuality = false;
        
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
        }


        /***************************************************************
         *                             操作
         **************************************************************/
        private static async void FavAsync(long id, View view)
        {
            Mastonet.MastodonClient clientfav = new UserClient().getClient();
            try
            {
                await clientfav.Favourite(id);
                var snakbar = Snackbar.Make(view, "ふぁぼりました",
                    Snackbar.LengthLong);
                snakbar.View.SetBackgroundColor(new Color(193, 151, 0));
                snakbar.SetAction("ACTION", (Android.Views.View.IOnClickListener)null).Show();
            }
            catch (System.Exception ex)
            {
                //Toast.MakeText(view.Context, "ふぁぼに失敗しました", ToastLength.Short).Show();
                Snackbar.Make(view, "ふぁぼに失敗しました",
               Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
        }

        public static void FavAsync(Status status, View view)
        {
            if (status.Favourited.Equals(true))
            {
                Snackbar.Make(view, "愛が深すぎる．．．（ふぁぼ済み）",
                        Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
            else
            {
                UserAction.FavAsync(status.Id, view);
                status.Favourited = true;
            }
        }

        private static async void BoostAsync(long id, View view)
        {
            Mastonet.MastodonClient clientReb = new UserClient().getClient();
            try
            {
                await clientReb.Reblog(id);

                var snackbar = Snackbar.Make(view, "ぶーすとしました", Snackbar.LengthLong);
                snackbar.View.SetBackgroundColor(new Color(61, 153, 0));
                snackbar.SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
            catch (System.Exception ex)
            {
                Snackbar.Make(view, "ぶーすとに失敗しました",
                     Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }

        }

        public static void BoostAsync(Status status, View view)
        {
            if(status.Reblogged.Equals(true) )
            {
                Snackbar.Make(view, "愛が深すぎる．．．（ぶーすと済み）",
                        Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
            else
            {
                BoostAsync(status.Id, view);
                status.Reblogged = true;
            }
        }

        public static void UrlOpen(string item_url, View view)
        {
            try
            {
                if (bBrowser)
                {
                    ////内部ブラウザ
                    //Intent intentC = new Intent(view.Context, typeof(BrowserActivity));
                    //intentC.PutExtra("url", item_url);
                    //view.Context.StartActivity(intentC);
                    UrlOpenCustomChrome(item_url, view);
                }
                else
                {
                    UrlOpenChrome(item_url, view);
                }
            }
            catch (Exception ex)
            {
                //Toast.MakeText(view.Context, "URLの取得に失敗しました", ToastLength.Short).Show();
                Snackbar.Make(view, "URLの取得に失敗しました",
                       Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
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
                //Toast.MakeText(view.Context, "URLの取得に失敗しました", ToastLength.Short).Show();
                Snackbar.Make(view, "URLの取得に失敗しました",
                       Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
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
                //Toast.MakeText(view.Context, "URLの取得に失敗しました", ToastLength.Short).Show();
                Snackbar.Make(view, "URLの取得に失敗しました",
                       Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
        }

        public static void Reply(Status status, View view)
        {
            //intent -> PostStatusActivity(intent) 
            var intent = new Intent(view.Context, typeof(PostStatusActivity));
            intent.PutExtra("status_Id", status.Id);
            intent.PutExtra("status_AcountName", status.Account.AccountName);
            view.Context.StartActivity(intent);
        }

        public static void ListViewItemClick(Status status , View view)
        {
            int select;
            try
            {
                var re_status = status.Reblog;
                if (re_status != null) status = re_status;
            }
            catch (Exception ex) { }

            //URL をitemlistに一時登録
            List<string> itemlist = new List<string>() { "Reply", "Favarite", "Boost", "Close" };
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
                    + "さん\r\n" + OtherTool.HTML_removeTag(status.Content) );
                dlg.SetItems(items,  (s, ee) =>
                {
                    select = ee.Which;
                    switch (select)
                    {
                        case 0://reply
                            UserAction.Reply(status,view);
                            break;

                        case 1://fav
                            UserAction.FavAsync(status, view);
                            break;

                        case 2://boost
                            UserAction.BoostAsync(status, view);
                            break;

                        case 3://close
                            break;

                        default://urls
                            UserAction.UrlOpen(items[select],view );
                            break;
                    }

                });
                dlg.Create().Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(view.Context, "なにかがおかしいよ...", ToastLength.Short).Show();

                //Snackbar.Make(view, "なにかがおかしいよ...",
                //    Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
        }
    }

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