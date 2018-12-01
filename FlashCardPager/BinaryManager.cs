using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace FlashCardPager
{
    //BinaryManage
    //設計 1次CacheとしてMapを使う．
    //2次Cache として，ファイルbin
    //なければ，url read
    //使ったら，1次に追加しておく
    public static class BinaryManager
    {
        //パラメーター
        static readonly string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) ;//(url) で保存 2次キャッシュ
        static readonly string emojiPath = System.IO.Path.Combine(path, "Emoji");

        static Dictionary<string, Bitmap> map = new Dictionary<string, Bitmap>();//1次キャッシュ
        static Dictionary<string, Bitmap> thumMap = new Dictionary<string, Bitmap>();//イメージ用のキャッシュ機構

        static Log Log;


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Bitmap ReadImage_From_File(string url)
        {
            Bitmap bitmap = null;
            string[] name = url.Split('/');//10が名前
            string image_name = name[name.Length - 1];

            try
            {
                //1次キャッシュ
                bitmap = map[image_name];
                return bitmap;
            }
            catch (KeyNotFoundException keynot)
            {
                //2次キャッシュ
                try
                {
                    string path2 = System.IO.Path.Combine(path, image_name);
                    using (var stream = new FileStream(path2, FileMode.Open))
                    {
                        bitmap = BitmapFactory.DecodeStream(stream);
                    }

                    if (bitmap != null)
                    {
                        map.Add(image_name, bitmap);//１次キャッシュに入れておく
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("BinaryManager Read", ex.Message);
                }
            }

            return bitmap;

        }




        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void WriteImage_To_File(string url, Bitmap bitmap)
        {
            string[] name;
            try
            {
                name = url.Split('/');//10が名前
                string image_name = name[name.Length - 1];
                string path2 = System.IO.Path.Combine(path, image_name);

                using(var stream = new FileStream(path2, FileMode.Create))
                {
                    bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                    map.Add(image_name, bitmap);
                }
            }
            catch (System.IO.IOException ioex)
            {
                //Log.Error("BinaryManager Write", ioex.Message);
            }
            catch (Exception e)
            {
                Log.Error("BinaryManager Write", e.Message);
            }
        }
        

        //For サムネイル＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Bitmap ReadMap_To_Bitmap(string url)
        {
            string[] name;
            Bitmap bitmap = null;

            name = url.Split('/');//10が名前
            string image_name = name[name.Length - 1];

            try
            {
                //1次キャッシュ
                bitmap = thumMap[image_name];
                return bitmap;
            }
            catch (KeyNotFoundException keynot)
            {
                return bitmap;
            }
        }


        //WriteMap
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void WriteBitmap_To_Map(string url, Bitmap bitmap)
        {
            string[] keys;

            try
            {
                keys = url.Split('/');
                string key = keys[keys.Length - 1];

                thumMap.Add(key, bitmap);
            }
            catch (System.IO.IOException ioex)
            {
                //Log.Error("BinaryManager Write", ioex.Message);
            }
            catch (Exception e)
            {
                //Log.Error("BinaryManager Write", e.Message);
            }
        }



        //Emoji =======================================================================

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Bitmap ReadImage_To_Emoji(string shortcode)
        {
            string emojipath2 = System.IO.Path.Combine(emojiPath, shortcode);
            Bitmap bitmap = null;
            try
            {
                using (var stream = new FileStream(emojipath2, FileMode.Open))
                {
                    bitmap = BitmapFactory.DecodeStream(stream);
                }
                return bitmap;
            }
            catch (IOException ioe)
            {
                return bitmap;
            }            
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void WriteImage_To_Emoji(string shortcode, Bitmap emojiData)
        {
            string emojipath2 = System.IO.Path.Combine(emojiPath, shortcode);

            //Emoji フォルダがなければ作る
            if (!Directory.Exists(emojiPath)) Directory.CreateDirectory(emojiPath);
            //ファイルをつくって書き込む
            try
            {
                using(var stream = new FileStream(emojipath2, FileMode.Create))
                {
                    emojiData.Compress(Bitmap.CompressFormat.Png, 100, stream);
                }
            }
            catch (IOException e)
            {
                //すでにあるか，エラー
                Log.Info("BinaryManager WriteEmoji", e.Message);
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool isEmojiCache(string shortcode)
        {
            if (File.Exists(System.IO.Path.Combine(emojiPath, shortcode)))
            {
                return true;
            }
            else return false;
        }

    }
}