using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Net;

namespace FlashCardPager
{
    public class ImageProvider
    {
        private string TAG = "ImageProvider";

        public ImageProvider() { }

        public async void ImageIconSetAsync(string url, ImageView image)
        {
            Bitmap bitmap = null;
            bitmap = BinaryManager.ReadImage_From_File(url);
            if (bitmap != null) image.SetImageBitmap(bitmap);

            else
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        var bytedata = await webClient.DownloadDataTaskAsync(url);
                        if (bytedata != null && bytedata.Length > 0)
                        {
                            try
                            {
                                bitmap = BitmapFactory.DecodeByteArray(bytedata, 0, bytedata.Length);//byte -> bitmpap
                                bitmap = Bitmap.CreateScaledBitmap(bitmap, 50, 50, false);//低画質化
                                BinaryManager.WriteImage_To_File(url, bitmap);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("download low error", ex.Message);
                            }
                            image.SetImageBitmap(bitmap);
                        }
                        else
                        {
                            image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                        }
                    }
                }
                catch (MalformedURLException e)
                {
                    Log.Error(TAG, e.Message);
                    image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                }
                catch (System.IO.IOException e)
                {
                    Log.Error(TAG, e.Message);
                    image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                }
                catch (WebException e)
                {
                    Log.Error(TAG, e.Message);
                    image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                }
                catch (ArgumentException e)
                {
                    Log.Error(TAG, e.Message);
                    image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                }
            }
        }

        public async void ImageThumnailSetAsync(string url, ImageView image)
        {
            Bitmap bitmap = null;
            //1 or 2 次キャッシュにあるか確認する
            bitmap = BinaryManager.ReadMap_To_Bitmap(url);
            if (bitmap != null) image.SetImageBitmap(bitmap);

            //ない場合
            else
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        var bytedata = await webClient.DownloadDataTaskAsync(url);
                        if (bytedata != null && bytedata.Length > 0)
                        {
                            bitmap = BitmapFactory.DecodeByteArray(bytedata, 0, bytedata.Length);//byte -> bitmpap
                            int x = bitmap.Width;
                            int y = bitmap.Height;
                            int MAX = 180;
                            //縦長  h:w = 48:ww
                            if (bitmap.Height > bitmap.Width)
                            {
                                //x:y = x*210/y : 210
                                x = x * MAX / y;
                                y = MAX;
                            }
                            //正方形 or 横長  h:w = hh:48
                            else
                            {
                                //x:y = 210 : y*210/x
                                y = y * MAX / x;
                                x = MAX;
                            }
                            bitmap = Bitmap.CreateScaledBitmap(bitmap, x, y, false);//低画質化
                            BinaryManager.WriteBitmap_To_Map(url, bitmap);//Cache登録
                            image.SetImageBitmap(bitmap);
                        }
                        else
                        {
                            image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                        }
                    }
                }
                catch (MalformedURLException e)
                {
                    Log.Error(TAG, e.Message);
                    image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                }
                catch (System.IO.IOException e)
                {
                    Log.Error(TAG, e.Message);
                    image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                }
                catch (WebException e)
                {
                    Log.Error(TAG, e.Message);
                    image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                }
                catch (ArgumentException e)
                {
                    Log.Error(TAG, e.Message);
                    image.SetImageResource(Android.Resource.Drawable.IcMenuGallery); //ic_menu_gallery
                }
            }
        }

    }
}