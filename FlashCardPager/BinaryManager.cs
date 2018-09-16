using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BinaryManager
    {
        //パラメーター
        readonly string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/";//(url).bin で保存 2次キャッシュ
        static Dictionary<string, byte[]> map = new Dictionary<string, byte[]>();//1次キャッシュ
        static Dictionary<string, byte[]> thumMap = new Dictionary<string, byte[]>();//イメージ用のキャッシュ機構
        Log Log;


        //ReadBin_To_Byte
        public byte[] ReadBin_To_Byte(string url)
        {
            string[] name;
            byte[] rtn = null;

            name = url.Split('/');//10が名前
            string image_name = name[name.Length - 1];

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
        public void WriteBin_To_File(string url, byte[] write_byte)
        {
            string[] name;

            try
            {
                name = url.Split('/');//10が名前
                string image_name = name[name.Length - 1];

                map.Add(image_name, write_byte);

                System.IO.File.WriteAllBytes(path + image_name + ".bin", write_byte);

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
        public byte[] ReadMap_to_Byte(string url)
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


        //WriteMap
        public void WriteBin_To_Map(string url, byte[] write_byte)
        {
            try
            {
                thumMap.Add(url, write_byte);
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


    }
}