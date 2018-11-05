﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
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
        static readonly string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/";//(url).bin で保存 2次キャッシュ
        static readonly string emojiPath = path + "Emoji/";

        static Dictionary<string, byte[]> map = new Dictionary<string, byte[]>();//1次キャッシュ
        static Dictionary<string, byte[]> thumMap = new Dictionary<string, byte[]>();//イメージ用のキャッシュ機構
        //static Dictionary<string, byte[]> emojiMap = new Dictionary<string, byte[]>();//emojiキャッシュ
        static Log Log;

        //ReadBin_To_Byte
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static byte[] ReadBin_To_Byte(string url)
        {
            //string[] name;
            byte[] rtn = null;

            string[] name = url.Split('/');//10が名前
            string image_name = name[name.Length - 1];
            //string image_name = accountName;

            try
            {
                //1次キャッシュ
                rtn = map[image_name];
                return rtn;
            }
            catch (KeyNotFoundException keynot)
            {
                //2次キャッシュ
                try
                {
                    rtn = System.IO.File.ReadAllBytes(path + image_name + ".bin");

                    if (rtn != null) map.Add(image_name, rtn);//１次キャッシュに入れておく
                    return rtn;
                }
                catch (Exception ex)
                {
                    Log.Error("BinaryManager Read", ex.Message);
                }
            }

            return rtn;
        }

        //WriteBin_To_File
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void WriteBin_To_File(string url, byte[] write_byte)
        {
            string[] name;

            try
            {
                name = url.Split('/');//10が名前
                string image_name = name[name.Length - 1];
                //string image_name = accountName;

                System.IO.File.WriteAllBytes(path + image_name + ".bin", write_byte);
                map.Add(image_name, write_byte);


            }
            catch (System.IO.IOException ioex)
            {
                Log.Error("BinaryManager Write", ioex.Message);
            }
            catch (Exception e)
            {
                Log.Error("BinaryManager Write", e.Message);
            }
        }

        //For サムネイル＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝

        //ReadMap
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static byte[] ReadMap_to_Byte(string url)
        {
            string[] name;
            byte[] rtn = null;

            name = url.Split('/');//10が名前
            string image_name = name[name.Length - 1];

            try
            {
                //1次キャッシュ
                rtn = thumMap[image_name];
                return rtn;
            }
            catch (KeyNotFoundException keynot)
            {
                return null;
            }
        }


        //WriteMap
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void WriteBin_To_Map(string url, byte[] write_byte)
        {
            string[] keys;

            try
            {
                keys = url.Split('/');
                string key = keys[keys.Length -1 ];

                thumMap.Add(key, write_byte);
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
        public static byte[] ReadBin_To_Emoji(string shortcode)
        {
            byte[] rtn = null;
            try
            {
                rtn = File.ReadAllBytes(emojiPath + shortcode);
            }
            catch (IOException ioe)
            {
                rtn = null;
            }
            ////1次キャッシュ 読み込み
            //try
            //{
            //    rtn = emojiMap[shortcode];
            //    return rtn;
            //}
            ////2次キャッシュに移行
            //catch(KeyNotFoundException e)
            //{
            //    try
            //    {
            //        rtn = File.ReadAllBytes(emojiPath + shortcode);
            //        //if (rtn != null) emojiMap.TryAdd(shortcode, rtn);
            //    }
            //    catch (IOException ioe)
            //    {
            //        rtn = null;
            //    }
            //}
            return rtn;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void WriteBin_To_Emoji(string shortcode, byte[] emojiData)
        {
            //Emoji フォルダがなければ作る
            if (!Directory.Exists(emojiPath)) Directory.CreateDirectory(emojiPath);
            //ファイルをつくって書き込む
            try
            {
                //if (!(emojiMap.TryAdd(shortcode, emojiData))) return; 
                File.WriteAllBytes(emojiPath + shortcode, emojiData);
            }
            catch(IOException e)
            {
                //すでにあるか，エラー
                Log.Info("BinaryManager WriteEmoji", e.Message);
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool isEmojiCache(string shortcode)
        {
            if (File.Exists(emojiPath + shortcode)) return true;
            else return false;
        }

    }
}