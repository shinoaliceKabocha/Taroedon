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
    [Activity(Label = "LicenseActivity", Theme = "@style/AppTheme")]
    public class LicenseActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.License);

            var licenseTextView = FindViewById<TextView>(Resource.Id.textViewLicense);
            string amazon = "https://www.amazon.co.jp/registry/wishlist/1RU8JEI9QE27B/ref=cm_sw_r_tw";


            

            var textviewMessage = FindViewById<TextView>(Resource.Id.textViewMessage);
            textviewMessage.Text =
                "ハジメマシテ．本アプリの作者 かぼちゃ(@taroedon @taroedon.com)と申します．\n"
                + "本アプリは，Android向けMastodonクライアントアプリケーションです．"
                + "\n\n"
                + "自由に使っていただいて構いません．\n"
                + "下記のAmazon 欲しいものリストにて，サーバー運営の寄付を募っております．\n"
                + "Dr Pepperを恵んでいただけると，明日もがんばれます．";


            var textviewAmazon = FindViewById<TextView>(Resource.Id.textViewAMAZON);
            textviewAmazon.Click += (sender, e) =>
            {
                Android.Net.Uri uri;
                uri = Android.Net.Uri.Parse(amazon);
                Intent intentB = new Android.Content.Intent(Intent.ActionView, uri);
                this.StartActivity(intentB);
            };
            textviewAmazon.Text = "欲しいものリスト(タップするとブラウザが開きます)\n" + amazon ;


            licenseTextView.Text =
            "■ ArcLight\n\n"
            + "MIT License\n\n"
            + "Copyright(c) 2018 Kenta Maehata\n\n"
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
            + "SOFTWARE.";

            licenseTextView.Text +=
                "\n\n"
                +
                "■ Mastonet\n"
                + "https://github.com/glacasa/Mastonet/blob/master/LICENSE"
                + "\n\n"
                + "■ Newtonsoft.Json\n"
                + "https://raw.githubusercontent.com/JamesNK/Newtonsoft.Json/master/LICENSE.md"
                + "\n\n"
                + "■ Xamarin / AndroidSupportComponents\n"
                + "https://github.com/xamarin/AndroidSupportComponents/blob/master/LICENSE.md"
                + "\n\n";

        }
    }
}