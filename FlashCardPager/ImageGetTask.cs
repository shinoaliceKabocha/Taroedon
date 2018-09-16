using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mastonet;
using Mastonet.Entities;
using Android.Graphics;
using Java.Net;
using Java.IO;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Android.Util;

namespace FlashCardPager
{
    //アイコン用
    public class ImageGetTask : AsyncTask<string, string, Bitmap>
    {
        private ImageView image;
        static BinaryManager bm = new BinaryManager();

        public ImageGetTask(ImageView _image)
        {
            image = _image;
        }

        protected override void OnPreExecute() { }

        protected override Bitmap RunInBackground(params string[] @params)
        {
            Bitmap bitmap_image = null;
            bool convert_error_flg = false;

            //1 or 2 次キャッシュにあるか確認する
            byte[] imageBytes = bm.ReadBin_To_Byte(@params[0]);
            //ある場合
            if (imageBytes != null)
            {
                try
                {
                    bitmap_image = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    return bitmap_image;
                }
                catch (Exception ex)
                {
                    Log.Error("convert erorr", ex.Message);
                    convert_error_flg = true;
                }
            }
            else if (imageBytes == null || convert_error_flg == true) //ない場合 もしくはエラー
            {
                try
                {
                    /******************************************
                        URL imageUrl = new URL(@params[0]);
                        var imageIs = imageUrl.OpenStream();

                        image = BitmapFactory.DecodeStream(imageIs);

                        BitmapFactory.Options ops = new BitmapFactory.Options();
                        ops.InPreferredConfig = Bitmap.Config.Rgb565;
                        ops.InJustDecodeBounds = true;
                    ******************************************/
                    using (var webClient = new WebClient())
                    {

                        imageBytes = webClient.DownloadData(@params[0]);
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            try
                            {
                                bitmap_image = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);//byte -> bitmpap
                                bitmap_image = Bitmap.CreateScaledBitmap(bitmap_image, 32, 32, false);//低画質化

                                MemoryStream memoryStream = new MemoryStream();//byte[] stream
                                bitmap_image.Compress(Bitmap.CompressFormat.Png, 100, memoryStream);//bitmap -> byte[]

                                bm.WriteBin_To_File(@params[0], memoryStream.ToArray());//1 ，２次キャッシュに書き込み
                            }
                            catch (Exception ex)
                            {
                                Log.Error("download low error", ex.Message);
                            }
                            return bitmap_image;
                        }
                        else return bitmap_image;//null
                    };

                }
                catch (MalformedURLException e)
                {
                    return null;
                }
                catch (System.IO.IOException e)
                {
                    return null;
                }

            }
            return null;
        }

        protected override void OnPostExecute(Bitmap result)
        {
            image.SetImageBitmap(result);//imageview にBitmapデータ格納

        }
    }

    //イメージ用
    public class ImageGetTask2 : AsyncTask<string, string, Bitmap>
    {
        private ImageView image;
        static BinaryManager bm = new BinaryManager();

        public ImageGetTask2(ImageView _image)
        {
            image = _image;
        }

        protected override void OnPreExecute() { }

        protected override Bitmap RunInBackground(params string[] @params)
        {
            Bitmap bitmap_image = null;
            bool convert_error_flg = false;

            //1 or 2 次キャッシュにあるか確認する
            byte[] imageBytes = bm.ReadMap_to_Byte(@params[0]);
            //ある場合
            if (imageBytes != null)
            {
                try
                {
                    bitmap_image = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    return bitmap_image;
                }
                catch (Exception ex)
                {
                    Log.Error("convert erorr", ex.Message);
                    convert_error_flg = true;
                }
            }
            else if (imageBytes == null || convert_error_flg == true) //ない場合 もしくはエラー
            {
                try
                {
                    using (var webClient = new WebClient())
                    {

                        imageBytes = webClient.DownloadData(@params[0]);
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            try
                            {
                                bitmap_image = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);//byte -> bitmpap
                                int bmpWidth, bmpHeight;
                                //縦長  h:w = 48:ww
                                if (bitmap_image.Height > bitmap_image.Width)
                                {
                                    bmpHeight = 60;
                                    bmpWidth = bitmap_image.Width * bmpHeight / bitmap_image.Height;
                                }
                                //正方形 or 横長  h:w = hh:48
                                else
                                {
                                    bmpWidth = 60;
                                    bmpHeight = bitmap_image.Height * bmpWidth / bitmap_image.Width;
                                }
                                bitmap_image = Bitmap.CreateScaledBitmap(bitmap_image, bmpWidth, bmpHeight, false);//低画質化

                                MemoryStream memoryStream = new MemoryStream();//byte[] stream
                                bitmap_image.Compress(Bitmap.CompressFormat.Png, 100, memoryStream);//bitmap -> byte[]

                                bm.WriteBin_To_Map(@params[0], memoryStream.ToArray());//1 ，２次キャッシュに書き込み
                            }
                            catch (Exception ex)
                            {
                                Log.Error("download low error", ex.Message);
                            }
                            return bitmap_image;
                        }
                        else return bitmap_image;//null
                    };

                }
                catch (MalformedURLException e)
                {
                    return null;
                }
                catch (System.IO.IOException e)
                {
                    return null;
                }

            }
            return null;
        }


        protected override void OnPostExecute(Bitmap result)
        {
            image.SetImageBitmap(result);//imageview にBitmapデータ格納

        }
    }

}