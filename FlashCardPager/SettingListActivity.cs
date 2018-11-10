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

namespace FlashCardPager
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

            //画像の画質
            var mImageQuolity = FindViewById<Switch>(Resource.Id.switchImageQuality);
            mImageQuolity.Checked = UserAction.bImageQuality;
            mImageQuolity.CheckedChange += (sender, e) =>
            {
                editor.PutBoolean("imageQuality", mImageQuolity.Checked);
                editor.Commit();
                UserAction.bImageQuality = mImageQuolity.Checked;
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
                            if (Directory.Exists(path + "/Emoji"))
                            {
                                string[] filePaths = Directory.GetFiles(path + "/Emoji");
                                foreach (string file in filePaths)
                                {
                                    File.SetAttributes(file, FileAttributes.Normal);
                                    File.Delete(file);
                                }
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
                            Toast.MakeText(this, "キャッシュデータを削除しました．再起動してください．", ToastLength.Short).Show();
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
                intent = new Intent(this, typeof(LicenseActivity));
                StartActivity(intent);
            };
        }
    }
}