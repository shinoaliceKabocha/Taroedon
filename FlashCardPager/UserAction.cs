using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Mastonet.Entities;

namespace FlashCardPager
{
    public static class UserAction
    {
        private static Mastonet.MastodonClient client = new UserClient().getClient();

        /***************************************************************
         *                             操作
         **************************************************************/
        public static async void FavAsync(long id, View view)
        {
            try
            {
                await client.Favourite(id);
                var snakbar = Snackbar.Make(view, "ふぁぼりました",
                    Snackbar.LengthLong);
                snakbar.SetAction("ACTION", (Android.Views.View.IOnClickListener)null).Show();
            }
            catch (System.Exception ex)
            {
                Snackbar.Make(view, "ふぁぼに失敗しました",
               Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
        }

        public static async void BoostAsync(long id, View view)
        {
            try
            {
                await client.Reblog(id);
                Snackbar.Make(view, "ぶーすとしました",
                    Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
            catch (System.Exception ex)
            {
                Snackbar.Make(view, "ぶーすとに失敗しました",
                     Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }

        }

        public static void UrlOpen(string item_url, View view)
        {
            try
            {
                //外部ブラウザ
                //Android.Net.Uri uri;
                //uri = Android.Net.Uri.Parse(item_url);
                //Intent intentB = new Android.Content.Intent(Intent.ActionView, uri);
                //view.Context.StartActivity(intentB);
                
                //内部ブラウザ
                Intent intentC = new Intent(view.Context, typeof(BrowserActivity));
                intentC.PutExtra("url", item_url);
                view.Context.StartActivity(intentC);
            }
            catch (Exception ex)
            {
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
            //StartActivityForResult(intent, 1);
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
                    + "さん\r\n" + OtherTool.HTML_removeTag(status.Content));
                dlg.SetItems(items, async (s, ee) =>
                {
                    select = ee.Which;
                    switch (select)
                    {
                        case 0://reply
                            UserAction.Reply(status,view);
                            break;

                        case 1://fav
                            UserAction.FavAsync(status.Id, view);
                            break;

                        case 2://boost
                            UserAction.BoostAsync(status.Id, view);
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
                Snackbar.Make(view, "なにかがおかしいよ...",
                    Snackbar.LengthLong).SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
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