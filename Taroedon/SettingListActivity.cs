using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AlertDialog = Android.App.AlertDialog;

namespace Taroedon
{
    [Activity(Label = "Settings", Theme = "@android:style/Theme.Material.Light.Dialog")]
    public class SettingListActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SettingList);
            Intent intent;

            var pref = GetSharedPreferences("SETTING", FileCreationMode.Private);
            var editor = pref.Edit();

            //setting browser
            var mBrowser = FindViewById<Switch>(Resource.Id.switchBrowser);
            mBrowser.Checked = UserAction.bBrowser;
            mBrowser.CheckedChange+=(sender, e) =>
            {
                editor.PutBoolean("browser", mBrowser.Checked);
                editor.Commit();
                UserAction.bBrowser = mBrowser.Checked;
            };

            //keep display
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

            //preview
            var mImagePreview = FindViewById<Switch>(Resource.Id.switchImagePreview);
            mImagePreview.Checked = UserAction.bImagePre;
            mImagePreview.CheckedChange += (sender, e) =>
            {
                editor.PutBoolean("imagePre", mImagePreview.Checked);
                editor.Commit();
                UserAction.bImagePre = mImagePreview.Checked;
            };

            //image of quarity
            var mImageQuolity = FindViewById<Switch>(Resource.Id.switchImageQuality);
            mImageQuolity.Checked = UserAction.bImageQuality;
            mImageQuolity.CheckedChange += (sender, e) =>
            {
                editor.PutBoolean("imageQuality", mImageQuolity.Checked);
                editor.Commit();
                UserAction.bImageQuality = mImageQuolity.Checked;
            };

            //thema
            var mTheme = FindViewById<Switch>(Resource.Id.switchTheme);
            mTheme.Checked = ColorDatabase.mode;
            mTheme.CheckedChange += (sender, e) =>
            {
                editor.PutBoolean("theme", mTheme.Checked);
                editor.Commit();
                ColorDatabase.mode = mTheme.Checked;
                UserAction.Toast_BottomFIllHorizontal_Show("次回起動時に反映されます", this, ColorDatabase.INFO);
            };

            //CacheClear
            //textViewCacheClearh
            var textViewCacheClearh = FindViewById<TextView>(Resource.Id.textViewCacheClearh);
            textViewCacheClearh.Click +=(sender, e) =>
            {
                var dlg = new AlertDialog.Builder(this);
                dlg.SetTitle("キャッシュを消しますか？");
                dlg.SetMessage("動作が不安定なときに安定するかもしれません");
                dlg.SetPositiveButton(
                    "OK", (s, a) =>
                    {
                        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                        try
                        {
                            UserAction.CacheClear();
                            UserAction.Toast_BottomFIllHorizontal_Show("キャッシュデータを削除しました．\n再起動してください", this, ColorDatabase.INFO);
                        }
                        catch(IOException ex)
                        {
                            Android.Util.Log.Debug("CacheClear", ex.Message);
                        }

                    });
                dlg.SetNegativeButton(
                    "Cancel", (s, a) =>
                    {

                    });
                dlg.Create().Show();
                
            };


            //moved mstdon auth
            var textViewAuth = FindViewById<TextView>(Resource.Id.textViewAuth);
            textViewAuth.Click+=(sender, e) =>
            {
                intent = new Intent(this, typeof(SettingsActivity));
                StartActivity(intent);
            };


            //mvoed twitter auth
            var twitter_Auth = FindViewById<TextView>(Resource.Id.textViewTwitterAuth);
            twitter_Auth.Click += (sender, e) =>
            {
                intent = new Intent(this, typeof(TwitterAuthActivity));
                StartActivity(intent);
            };

            //moved about app
            var textViewLicense = FindViewById<TextView>(Resource.Id.textViewLicense);
            textViewLicense.Click+=(sender, e) =>
            {
                intent = new Intent(this, typeof(LicenseActivity));
                StartActivity(intent);
            };

            //app ver
            var textview_ver = FindViewById<TextView>(Resource.Id.textViewVersion);
            var info = this.PackageManager.GetPackageInfo(this.PackageName, 0);
            textview_ver.Text = "Version." + info.VersionName;
        }
    }
}