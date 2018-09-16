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

namespace FlashCardPager
{
    [Activity(Label = "Certification", Theme = "@style/AppTheme")]
    public class SettingsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Settings);

            var e_instance = FindViewById<EditText>(Resource.Id.edittextSettings1_Instance);
            var e_address = FindViewById<EditText>(Resource.Id.edittextSettings1_Address);
            var e_pass = FindViewById<EditText>(Resource.Id.edittextSettings1_Pass);

            Button regist = FindViewById<Button>(Resource.Id.buttonSettings1_Registration);
            
            var pref = GetSharedPreferences("USER", FileCreationMode.Private);
            e_instance.Text = "";
            e_address.Text = "";
            e_pass.Text = "";

            FindViewById<TextView>(Resource.Id.textViewSettings1_1).LongClick+=(sender, e) =>
            {
                e_instance.Text = "taroedon.com";
            };

            regist.Click += async (sender, e) =>
            {
                if (e_instance.Text == "" || e_address.Text == "" || e_pass.Text == "")
                {
                    Toast.MakeText(this, "未記入項目があります", ToastLength.Short).Show();
                }
                else
                {
                    try
                    {
                        var authClient = new AuthenticationClient(e_instance.Text);
                        var appRegistration = await authClient.CreateApp("たろえどんmobile", Scope.Read | Scope.Write | Scope.Follow);
                        var auth = await authClient.ConnectWithPassword(e_address.Text, e_pass.Text);

                        var editor = pref.Edit();
                        editor.PutString("instance", e_instance.Text);
                        editor.PutString("clientId", appRegistration.ClientId);
                        editor.PutString("clientSecret", appRegistration.ClientSecret);
                        editor.PutString("accessToken", auth.AccessToken);
                        editor.PutString("redirectUri", "urn:ietf:wg:oauth:2.0:oob");
                        editor.Commit();

                        Toast.MakeText(this, "認証に成功しました！\nアプリを再起動してください", ToastLength.Short).Show();
                        Finish();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, "認証に失敗しました．．．\n入力ミスがあるかも", ToastLength.Short).Show();
                    }

                }
            };

        }

    }

}