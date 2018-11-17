using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Mastonet;
using Mastonet.Entities;
using Android.Support.CustomTabs;

namespace FlashCardPager
{
    [Activity(Label = "Authentication", Theme = "@style/AppTheme")]
    public class SettingsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);

            var e_instance = FindViewById<EditText>(Resource.Id.edittextSettings1_Instance);
            var e_code = FindViewById<EditText>(Resource.Id.edittextSettings1_Code);
            //var e_address = FindViewById<EditText>(Resource.Id.edittextSettings1_Address);
            //var e_pass = FindViewById<EditText>(Resource.Id.edittextSettings1_Pass);
            Button urlOpen = FindViewById<Button>(Resource.Id.buttonOAuth);
            Button regist = FindViewById<Button>(Resource.Id.buttonSettings1_Registration);
            
            var pref = GetSharedPreferences("USER", FileCreationMode.Private);
            e_instance.Text = "";
            e_code.Text = "";
            //e_address.Text = "";
            //e_pass.Text = "";

            e_code.Enabled = false;
            regist.Enabled = false;


            FindViewById<TextView>(Resource.Id.textViewSettings1_1).LongClick+=(sender, e) =>
            {
                e_instance.Text = "taroedon.com";
            };

            //regist.LongClick += async (sender, e) =>
            //{
            //    if (e_instance.Text == "" || e_address.Text == "" || e_pass.Text == "")
            //    {
            //        Toast.MakeText(this, "未記入項目があります", ToastLength.Short).Show();
            //    }
            //    else
            //    {
            //        try
            //        {
            //            var authClient = new AuthenticationClient(e_instance.Text);
            //            var appRegistration = await authClient.CreateApp("たろえどんmobile", Scope.Read | Scope.Write | Scope.Follow);
            //            var auth = await authClient.ConnectWithPassword(e_address.Text, e_pass.Text);

            //            var editor = pref.Edit();
            //            editor.PutString("instance", e_instance.Text);
            //            editor.PutString("clientId", appRegistration.ClientId);
            //            editor.PutString("clientSecret", appRegistration.ClientSecret);
            //            editor.PutString("accessToken", auth.AccessToken);
            //            editor.PutString("redirectUri", "urn:ietf:wg:oauth:2.0:oob");
            //            editor.Commit();

            //            Toast.MakeText(this, "認証に成功しました！\nアプリを再起動してください", ToastLength.Short).Show();
            //            Finish();
            //        }
            //        catch (Exception ex)
            //        {
            //            Toast.MakeText(this, "認証に失敗しました．．．\n入力ミスがあるかも", ToastLength.Short).Show();
            //        }

            //    }
            //};

            AuthenticationClient authClient2 = null;
            AppRegistration appRegistration2 = null;

            urlOpen.Click+= async (sender, e) =>
            {
                try
                {
                    authClient2 = new AuthenticationClient(e_instance.Text);
                    appRegistration2 = await authClient2.CreateApp("たろえどんmobile", Scope.Read | Scope.Write | Scope.Follow);
                    var url = authClient2.OAuthUrl();
                    //UserAction.UrlOpenChrome(url, (View)sender);
                    UserAction.UrlOpen(url, (View)sender);

                    //var builder = new CustomTabsIntent.Builder();
                    //builder.SetToolbarColor(Resource.Color.colorPrimaryDark);
                    //var chromeIntent = builder.Build();
                    //chromeIntent.LaunchUrl(this.ApplicationContext, Android.Net.Uri.Parse(url));
                    //StartActivity(chromeIntent.Intent);

                    e_code.Enabled = true;
                    regist.Enabled = true;
                    
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "インスタンス名がおかしいか，ネットワークが不安定かも", ToastLength.Short).Show();
                }
            };

            regist.Click += async (sender, e) =>
            {
                try
                {
                    var auth = await authClient2.ConnectWithCode(e_code.Text);

                    var editor = pref.Edit();
                    editor.PutString("instance", e_instance.Text);
                    editor.PutString("clientId", appRegistration2.ClientId);
                    editor.PutString("clientSecret", appRegistration2.ClientSecret);
                    editor.PutString("accessToken", auth.AccessToken);
                    editor.PutString("redirectUri", "urn:ietf:wg:oauth:2.0:oob");
                    editor.Commit();

                    Toast.MakeText(this, "認証に成功しました！\nアプリを再起動してください", ToastLength.Short).Show();
                    Finish();
                }
                catch(Exception ex)
                {
                    Toast.MakeText(this, "コードが間違っているか，認証に失敗しました", ToastLength.Short).Show();
                }
            };

        }

    }

}