﻿using System;
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

            //テーマ
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


            //認証画面への移行
            var textViewAuth = FindViewById<TextView>(Resource.Id.textViewAuth);
            textViewAuth.Click+=(sender, e) =>
            {
                intent = new Intent(this, typeof(SettingsActivity));
                StartActivity(intent);
            };


            //Twitter認証機能
            //var mTwitter_OnOff = FindViewById<Switch>(Resource.Id.switchTwitter);
            //mTwitter_OnOff.Checked = UserAction.bTwitterOnOff;
            //mTwitter_OnOff.CheckedChange += (sender, e) =>
            //{
            //    editor.PutBoolean("twitterOnOff", mTwitter_OnOff.Checked);
            //    editor.Commit();
            //    UserAction.bTwitterOnOff = mTwitter_OnOff.Checked;
            //};

            var twitter_Auth = FindViewById<TextView>(Resource.Id.textViewTwitterAuth);
            twitter_Auth.Click += (sender, e) =>
            {
                intent = new Intent(this, typeof(TwitterAuthActivity));
                StartActivity(intent);
            };

            //アプリについて への移行
            var textViewLicense = FindViewById<TextView>(Resource.Id.textViewLicense);
            textViewLicense.Click+=(sender, e) =>
            {
                intent = new Intent(this, typeof(LicenseActivity));
                StartActivity(intent);
            };

            //アプリ ver
            var textview_ver = FindViewById<TextView>(Resource.Id.textViewVersion);
            var info = this.PackageManager.GetPackageInfo(this.PackageName, 0);
            textview_ver.Text = "Version." + info.VersionName;
        }
    }
}