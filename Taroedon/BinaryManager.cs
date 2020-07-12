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

namespace Taroedon
{
    public static class BinaryManager
    {
        static readonly string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) ;
        static readonly string emojiPath = System.IO.Path.Combine(path, "Emoji");

        static Dictionary<string, Bitmap> map = new Dictionary<string, Bitmap>();
        static Dictionary<string, Bitmap> thumMap = new Dictionary<string, Bitmap>();

        static Log Log;

        public static Bitmap ReadImage_From_File(string url)
        {
            Bitmap bitmap = null;
            string[] name = url.Split('/');
            string image_name = name[name.Length - 1];

            try
            {
                bitmap = map[image_name];
                return bitmap;
            }
            catch (KeyNotFoundException keynot)
            {
                try
                {
                    string path2 = System.IO.Path.Combine(path, image_name);
                    using (var stream = new FileStream(path2, FileMode.Open))
                    {
                        bitmap = BitmapFactory.DecodeStream(stream);
                    }

                    if (bitmap != null)
                    {
                        map.Add(image_name, bitmap);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("BinaryManager Read", ex.Message);
                }
            }

            return bitmap;

        }


        public static void WriteImage_To_File(string url, Bitmap bitmap)
        {
            string[] name;
            try
            {
                name = url.Split('/');
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
        

        public static Bitmap ReadMap_To_Bitmap(string url)
        {
            string[] name;
            Bitmap bitmap = null;

            name = url.Split('/');
            string image_name = name[name.Length - 1];

            try
            {
                bitmap = thumMap[image_name];
                return bitmap;
            }
            catch (KeyNotFoundException keynot)
            {
                return bitmap;
            }
        }


        //WriteMap
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


        public static void WriteImage_To_Emoji(string shortcode, Bitmap emojiData)
        {
            string emojipath2 = System.IO.Path.Combine(emojiPath, shortcode);

            if (!Directory.Exists(emojiPath)) Directory.CreateDirectory(emojiPath);
            try
            {
                using(var stream = new FileStream(emojipath2, FileMode.Create))
                {
                    emojiData.Compress(Bitmap.CompressFormat.Png, 100, stream);
                }
            }
            catch (IOException e)
            {
                Log.Info("BinaryManager WriteEmoji", e.Message);
            }
        }

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