using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CoreTweet;
using System.Diagnostics;

namespace FlashCardPager
{
    [Activity(Label = "Twitter Authentication", Theme = "@style/AppTheme")]
    public class TwitterAuthActivity : AppCompatActivity
    {
        private string API_KEY = "HlbyHC4pNLeiddF9UscurTOIT";
        private string API_SERCRET = "Mxq4N6osMnaCryk2NN48o5lOUtcGPzQqW8YYTAJfU8mkqKLGGT";
        private OAuth.OAuthSession mSession;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.TwitterAuth);

        }

        protected override void OnStart()
        {
            base.OnStart();

            var buttonTwiAuth = FindViewById<Button>(Resource.Id.buttonTwitterOAuth);
            var e_Pincode = FindViewById<EditText>(Resource.Id.edittextTwitterCode);
            var buttonPincode = FindViewById<Button>(Resource.Id.buttonTwitter_Registration);

            var pref = GetSharedPreferences("TWITTER", FileCreationMode.Private);
            buttonTwiAuth.Click += async (sender, e) =>
            {
                mSession = await OAuth.AuthorizeAsync(API_KEY, API_SERCRET);
                UserAction.UrlOpen(mSession.AuthorizeUri.AbsoluteUri, (View)sender);
            };


            buttonPincode.Click += (sender, e) =>
            {
                string pin = e_Pincode.Text;
                if (string.IsNullOrWhiteSpace(pin))
                {
                    UserAction.Toast_BottomFIllHorizontal_Show
                        ("コードが間違っているか\n認証に失敗しました", this, ColorDatabase.FAILED);
                    return;
                }

                try
                {
                    var tokens = OAuth.GetTokens(mSession, e_Pincode.Text);
                    var editor = pref.Edit();
                    editor.PutString("twiConsumerKey", tokens.ConsumerKey);
                    editor.PutString("twiConsumerSecret", tokens.ConsumerSecret);
                    editor.PutString("twiAccessToken", tokens.AccessToken);
                    editor.PutString("twiAccessTokenSecret", tokens.AccessTokenSecret);
                    editor.Commit();

                    UserAction.Toast_BottomFIllHorizontal_Show("認証に成功しました！\nアプリを再起動すると反映されます", this, ColorDatabase.INFO);
                    FinishAndRemoveTask();
                }
                catch (Exception ex)
                {
                    UserAction.Toast_BottomFIllHorizontal_Show("コードが間違っているか\n認証に失敗しました", this, ColorDatabase.FAILED);

                }
            };

        }
    }
}