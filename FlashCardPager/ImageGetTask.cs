//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Mastonet;
//using Mastonet.Entities;
//using Android.Graphics;
//using Java.Net;
//using Java.IO;
//using System.Net;
//using System.IO;
//using System.Text.RegularExpressions;
//using Android.Util;

//namespace FlashCardPager
//{
//    //アイコン用
//    public class ImageGetTask : AsyncTask<string, string, Bitmap>
//    {
//        public static readonly string TAG = "ImageGetTask";
//        private ImageView image;
//        //static BinaryManager bm = new BinaryManager();

//        public ImageGetTask(ImageView _image)
//        {
//            image = _image;
//        }

//        protected override void OnPreExecute() { }

//        protected override Bitmap RunInBackground(params string[] @params)
//        {
//            Bitmap bitmap_image = null;
//            //bool convert_error_flg = false;

//            //1 or 2 次キャッシュにあるか確認する
//            //byte[] imageBytes = BinaryManager.ReadBin_To_Byte(@params[0]);
//            bitmap_image = BinaryManager.ReadImage_From_File(@params[0]);
//            //ある場合
//            if (bitmap_image != null) return bitmap_image;
//            else
//            {
//                try
//                {
//                    using (var webClient = new WebClient())
//                    {
//                        var bytedata = webClient.DownloadData(@params[0]);
//                        if (bytedata != null && bytedata.Length > 0)
//                        {
//                            try
//                            {
//                                bitmap_image = BitmapFactory.DecodeByteArray(bytedata, 0, bytedata.Length);//byte -> bitmpap
//                                bitmap_image = Bitmap.CreateScaledBitmap(bitmap_image, 50, 50, false);//低画質化
//                                BinaryManager.WriteImage_To_File(@params[0], bitmap_image);
//                            }
//                            catch (Exception ex)
//                            {
//                                Log.Error("download low error", ex.Message);
//                            }
//                            return bitmap_image;
//                        }
//                    }
//                }
//                catch (MalformedURLException e)
//                {
//                    Log.Error(TAG, e.Message);
//                    return bitmap_image;
//                }
//                catch (System.IO.IOException e)
//                {
//                    Log.Error(TAG, e.Message);
//                    return bitmap_image;
//                }
//            }
//            return null;
//        }
        
//        protected override void OnPostExecute(Bitmap result)
//        {
//            image.SetImageBitmap(result);//imageview にBitmapデータ格納

//        }
//    }







//    //イメージ用
//    public class ImageGetTask2 : AsyncTask<string, string, Bitmap>
//    {
//        public static readonly string TAG = "ImageGetTask2";
//        private ImageView image;
 
//        public ImageGetTask2(ImageView _image)
//        {
//            image = _image;
//        }

//        protected override void OnPreExecute() { }

//        protected override Bitmap RunInBackground(params string[] @params)
//        {
//            Bitmap bitmap_image = null;
//            bool convert_error_flg = false;

//            //1 or 2 次キャッシュにあるか確認する
//            bitmap_image = BinaryManager.ReadMap_To_Bitmap(@params[0]);
//            if (bitmap_image != null) return bitmap_image;
//            //ない場合
//            else
//            {
//                try
//                {
//                    using (var webClient = new WebClient())
//                    {
//                        var bytedata = webClient.DownloadData(@params[0]);
//                        if (bytedata != null && bytedata.Length > 0)
//                        {
//                            bitmap_image = BitmapFactory.DecodeByteArray(bytedata, 0, bytedata.Length);//byte -> bitmpap
//                            int x = bitmap_image.Width;
//                            int y = bitmap_image.Height;
//                            int MAX = 180;
//                            //縦長  h:w = 48:ww
//                            if (bitmap_image.Height > bitmap_image.Width)
//                            {
//                                //x:y = x*210/y : 210
//                                x = x * MAX / y;
//                                y = MAX;
//                            }
//                            //正方形 or 横長  h:w = hh:48
//                            else
//                            {
//                                //x:y = 210 : y*210/x
//                                y = y * MAX / x;
//                                x = MAX;
//                            }
//                            bitmap_image = Bitmap.CreateScaledBitmap(bitmap_image, x, y, false);//低画質化
//                            BinaryManager.WriteBitmap_To_Map(@params[0], bitmap_image);//Cache登録

//                            return bitmap_image;
//                        }
//                        else
//                        {
//                            return bitmap_image;
//                        }
//                    }
//                }
//                catch (MalformedURLException e)
//                {
//                    Log.Error(TAG, e.Message);
//                    return bitmap_image;
//                }
//                catch (System.IO.IOException e)
//                {
//                    Log.Error(TAG, e.Message);
//                    return bitmap_image;
//                }
//            }
           
//        }


//        protected override void OnPostExecute(Bitmap result)
//        {
//            image.SetImageBitmap(result);//imageview にBitmapデータ格納

//        }
//    }

//}