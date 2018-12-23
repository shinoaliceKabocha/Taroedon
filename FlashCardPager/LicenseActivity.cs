using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Views;
using Android.Widget;

namespace FlashCardPager
{
    [Activity(Label = "About this Application", Theme = "@style/AppTheme")]
    public class LicenseActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.License);

            //まずはじめに
            var textviewMessage = FindViewById<TextView>(Resource.Id.textViewMessage);
            textviewMessage.Text =
                "ハジメマシテ．本アプリの作者 かぼちゃ(@taroedon @taroedon.com)と申します．\n"
                + "本アプリは，Android向けMastodonクライアントアプリケーションです．"
                + "\n\n"
                + "大体のインスタンスに対応できるかと思います.\n"
                + "気に入って頂けたらドクペをください．";

            //このアプリについて
            var textViewHowToUse = FindViewById<TextView>(Resource.Id.textViewHowToUse);
            textViewHowToUse.Text =
                "・マルチカラム対応(Home，Mention，Public)\n"
                + "・HomeとPublicはStreamingでリアルタイムで取得します\n"
                + "・タイムラインを下に引っ張ると更新します\n"
                + "・下に遡ると過去のトゥートを表示できます\n"
                + "・v1.0.6 : カスタム絵文字表示に対応しました．\n"
                + "・v1.1.6 : Twitterへのクロスポストを実装しました \n"
                + "\n"
                + "画面右上の設定から．．．\n"
                + "    ・ブラウザの設定（標準かアプリ内のブラウザ）\n"
                + "    ・起動中はスリープにしないか\n"
                + "          などできます\n"
                + "\n"
                + "右下の投稿ボタンから．．．\n"
                + "    ・500文字以内，4枚までの画像でトゥート\n"
                + "    ・投稿範囲(Public, Private, Direct)に対応\n"
                + "\n"
                + "トゥートをタップして表示するダイアログから．．．\n"
                + "    ・favorite, boost，Mention，プロフィール表示に対応\n"
                + "\n"
                + "トゥートをロングタップすると．．．\n"
                + "    ・favoriteできます\n"
                + "    ・今後boostへの変更など機能追加予定\n"
                + "\n"
                + "v1.1.0からはプロフィール画面を実装しました(β版)\n"
                + "    ・フォロー・アンフォローできます\n";


            //欲しいものリスト
            string amazon = "https://www.amazon.co.jp/registry/wishlist/1RU8JEI9QE27B/ref=cm_sw_r_tw";
            string headerAmazon = "Amazon干芋リスト\n(タップするとブラウザが開きます)\n";
            var textviewAmazon = FindViewById<TextView>(Resource.Id.textViewAMAZON);
            textviewAmazon.SetText(headerAmazon + amazon, TextView.BufferType.Normal);
            textviewAmazon.SetLinkTextColor(GetColorStateList(ColorDatabase.TLLINK));
            textviewAmazon.Click += (sender, e) =>
            {
                Android.Net.Uri uri;
                uri = Android.Net.Uri.Parse(amazon);
                Intent intentB = new Android.Content.Intent(Intent.ActionView, uri);
                this.StartActivity(intentB);
            };

            //ライセンス
            var licenseTextView = FindViewById<TextView>(Resource.Id.textViewLicense);
            string[] otherlicense = new string[]
            {
              "■ ArcLight\n\n"
            + "MIT License\n\n"
            + "Copyright(c) 2018 Kabochan\n\n"
            + "Permission is hereby granted, free of charge, to any person obtaining a copy"
            + "of this software and associated documentation files(the " + "Software" + "), to deal"
            + "in the Software without restriction, including without limitation the rights"
            + "to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell"
            + "copies of the Software, and to permit persons to whom the Software is"
            + "furnished to do so, subject to the following conditions:\n\n"

            + "The above copyright notice and this permission notice shall be included in all"
            + "copies or substantial portions of the Software.\n\n"

            +"THE SOFTWARE IS PROVIDED "+"AS IS"+", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR"
            +"IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,"
            +"FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE"
            +"AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER"
            +"IABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,"
            +"UT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE"
            + "SOFTWARE.",

            "\n\n■ Mastonet\n",
            "https://github.com/glacasa/Mastonet/blob/master/LICENSE",

            "\n\n■ Newtonsoft.Json\n",
            "https://raw.githubusercontent.com/JamesNK/Newtonsoft.Json/master/LICENSE.md",

            "\n\n■ Xamarin / AndroidSupportComponents\n",
            "https://github.com/xamarin/xamarin-android/blob/master/LICENSE",

            "\n\n■ CoreTweet\n",
            "https://opensource.org/licenses/mit-license.php",
            };

            string licenses = "";
            foreach(string s in otherlicense)
            {
                licenses += s;
            }
            licenseTextView.SetText(licenses, TextView.BufferType.Normal);
            licenseTextView.SetLinkTextColor(GetColorStateList(ColorDatabase.TLLINK));
            licenseTextView.MovementMethod = new LocalLinkMovementMethod();

        }
    }

    public class LicenseClickableSpan : ClickableSpan
    {
        private string sString;

        public LicenseClickableSpan(string s)
        {
            this.sString = s;
        }

        public override void OnClick(View widget)
        {
            UserAction.UrlOpen(sString, widget);
        }
    }
}