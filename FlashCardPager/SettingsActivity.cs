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

namespace FlashCardPager
{
    [Activity(Label = "Mastodon Authentication", Theme = "@style/AppTheme")]
    public class SettingsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);

            var e_instance = FindViewById<EditText>(Resource.Id.edittextSettings1_Instance);
            var e_code = FindViewById<EditText>(Resource.Id.edittextSettings1_Code);

            Button urlOpen = FindViewById<Button>(Resource.Id.buttonOAuth);
            Button regist = FindViewById<Button>(Resource.Id.buttonSettings1_Registration);
            
            var pref = GetSharedPreferences("USER", FileCreationMode.Private);
            e_instance.Text = "";
            e_code.Text = "";

            e_code.Enabled = false;
            regist.Enabled = false;


            FindViewById<TextView>(Resource.Id.textViewSettings1_1).LongClick+=(sender, e) =>
            {
                e_instance.Text = "taroedon.com";
            };

            AuthenticationClient authClient2 = null;
            AppRegistration appRegistration2 = null;

            urlOpen.Click+= async (sender, e) =>
            {
                try
                {
                    authClient2 = new AuthenticationClient(e_instance.Text);
                    appRegistration2 = await authClient2.CreateApp("たろえどんmobile", Scope.Read | Scope.Write | Scope.Follow);
                    var url = authClient2.OAuthUrl();
                    UserAction.UrlOpen(url, (View)sender);

                    e_code.Enabled = true;
                    regist.Enabled = true;
                    
                }
                catch (Exception ex)
                {
                    UserAction.Toast_BottomFIllHorizontal_Show("インスタンス名がおかしいか\nネットワークが不安定かも", this, ColorDatabase.FAILED);
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

                    UserAction.Toast_BottomFIllHorizontal_Show("認証に成功しました！\nアプリを再起動してください", this, ColorDatabase.INFO);
                    UserAction.CacheClear();
                    FinishAndRemoveTask();
                }
                catch(Exception ex)
                {
                    UserAction.Toast_BottomFIllHorizontal_Show("コードが間違っているか\n認証に失敗しました", this, ColorDatabase.FAILED);

                }
            };

        }

    }

}