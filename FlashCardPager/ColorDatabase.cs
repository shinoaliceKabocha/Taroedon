using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FlashCardPager
{
    public static class ColorDatabase
    {
        public static bool mode { get; set; }
        //readonly static bool mode = false;
        //カラーセット
        //TimeLine
        //テキストカラー
        public static Color TLTEXT
        {
            get
            {
                return (! mode ? TLTEXT1 : TLTEXT2);
            }
        }
        public static int TLLINK
        {
            get
            {
                return (! mode ? TLLINK1 : TLLINK2);
            }
        }
        public static Color PROFILE
        {
            get
            {
                return (! mode ? PROFILE1 : PROFILE2);
            }
        }
        public static Color TIME
        {
            get
            {
                return (! mode ? TIME1 : TIME2);
            }
        }
        //background normal
        public static Color TL_BACK
        {
            get
            {
                return (! mode ? TL_BACK1 : TL_BACK2);
            }
        }
        //background follow申請
        public static Color FF_BACK
        {
            get
            {
                return (! mode ? FF_BACK1 : FF_BACK2);
            }
        }
        //background fav
        public static Color FAV_BACK
        {
            get
            {
                return (! mode ? FAV_BACK1 : FAV_BACK2);
            }
        }
        //background boost
        public static Color BOOST_BACK
        {
            get
            {
                return (! mode ? BOOST_BACK1 : BOOST_BACK2);
            }
        }
        //background reply
        public static Color REPLY_BACK
        {
            get
            {
                return (! mode ? REPLY_BACK1 : REPLY_BACK2);
            }
        }



        /* Default テーマ */
        //TimeLine
        //テキストカラー
        readonly static Color TLTEXT1 = new Color(21, 21, 21);
        readonly static int TLLINK1 = Resource.Color.colorAccent;
        readonly static Color PROFILE1 = new Color(10, 51, 0);
        readonly static Color TIME1 = Color.DarkGray;
        //background normal
        readonly static Color TL_BACK1 = Color.AliceBlue;
        //background follow申請
        readonly static Color FF_BACK1 = new Color(191, 216, 255);
        //background fav
        readonly static Color FAV_BACK1 = new Color(255, 249, 216);
        //background boost
        readonly static Color BOOST_BACK1 = new Color(211, 244, 203);
        //background reply
        readonly static Color REPLY_BACK1 = new Color(255, 226, 216);


        /* ダークモード */
        //TimeLine
        //テキストカラー
        readonly static Color TLTEXT2 = new Color(198,198,198);
        readonly static int TLLINK2 = Resource.Color.colorAccent;
        readonly static Color PROFILE2 = new Color(196,196,196);
        readonly static Color TIME2 = new Color(175,175,175);
        //background normal
        readonly static Color TL_BACK2 = new Color(18,36,46);
        //background follow申請
        readonly static Color FF_BACK2 = new Color(19,35,56);
        //background fav
        readonly static Color FAV_BACK2 = new Color(56,56,19);
        //background boost
        readonly static Color BOOST_BACK2 = new Color(19,46,27);
        //background reply
        readonly static Color REPLY_BACK2 = new Color(56,39,19);




        /* Toast */
        //Fav 背景 黄色
        public readonly static Color FAV = new Color(193, 151, 0);
        //Boost 背景 緑
        public readonly static Color BOOST = new Color(61, 153, 0);
        //Follow リクエスト 成功 青
        public readonly static Color FOLLOW = new Color(127, 176, 255);
        //reply 赤色
        public readonly static Color REPLY = new Color(255, 131, 109);

        //失敗  グレー 
        public readonly static Color FAILED = new Color(160, 160, 160);
        //情報  シアン系
        public readonly static Color INFO = new Color(27, 49, 71);




    }
}