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

namespace FlashCardPager
{
    [Activity(Label = "Settings", Theme = "@style/AppTheme")]
    public class SettingListActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SettingList);
            Intent intent;

            var pref = GetSharedPreferences("SETTING", FileCreationMode.Private);
            var editor = pref.Edit();

            //ブラウザの設定
            var mBrowser = FindViewById<Switch>(Resource.Id.switchBrowser);
            mBrowser.Checked = UserAction.bBrowser;
            mBrowser.CheckedChange+=(sender, e) =>
            {
                editor.PutBoolean("browser", mBrowser.Checked);
                editor.Commit();
                UserAction.bBrowser = mBrowser.Checked;
            };

            //起動中は画面を・・・
            var mDisplay = FindViewById<Switch>(Resource.Id.switchDisplayOn);
            mDisplay.Checked = UserAction.bDisplay;
            mDisplay.CheckedChange += (sender, e) =>
            {
                editor.PutBoolean("display", mDisplay.Checked);
                editor.Commit();
                UserAction.bDisplay = mDisplay.Checked;

                if (mDisplay.Checked)
                {
                    SetResult(Result.Ok);
                }
                else
                {
                    SetResult(Result.Canceled);
                }
            };

            //画像のプレビュー
            var mImagePreview = FindViewById<Switch>(Resource.Id.switchImagePreview);
            mImagePreview.Checked = UserAction.bImagePre;
            mImagePreview.CheckedChange += (sender, e) =>
            {
                editor.PutBoolean("imagePre", mImagePreview.Checked);
                editor.Commit();
                UserAction.bImagePre = mImagePreview.Checked;
            };


            //認証画面への移行
            var textViewAuth = FindViewById<TextView>(Resource.Id.textViewAuth);
            textViewAuth.Click+=(sender, e) =>
            {
                intent = new Intent(this, typeof(SettingsActivity));
                StartActivity(intent);
            };
            //アプリについて への移行
            var textViewLicense = FindViewById<TextView>(Resource.Id.textViewLicense);
            textViewLicense.Click+=(sender, e) =>
            {
                Toast.MakeText(this, "このアプリについて", ToastLength.Short).Show();
            };
        }
    }
}