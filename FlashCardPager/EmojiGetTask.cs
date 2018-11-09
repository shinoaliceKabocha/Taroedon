﻿using System;
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
using Newtonsoft.Json;
using System.Net.Http;

namespace FlashCardPager
{
    public class EmojiGetTask
    {
        //コンストラクタ
        public EmojiGetTask() { }

        //メインの処理
        public Bitmap GetBitmap(string emojiShortcode)
        {
            byte[] imageBytes = null;

            imageBytes = BinaryManager.ReadBin_To_Emoji(emojiShortcode);
            //キャッシュにある場合
            if(imageBytes != null)
            {
                try
                {
                    return BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
                catch (Exception e)
                {
                    Log.Error("Emoji Error: ", e.Message);
                    return null;
                }
            }
            //ない場合
            else
            {
                return null;
            }
        }


        public async void InitEmojiListAsync()
        {
            var client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler())
            {
                BaseAddress = new Uri($"https://" + UserClient.instance)
            };
            var content = new MultipartFormDataContent();

            var response = await client.GetAsync("/api/v1/custom_emojis");
            var res_json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var emojis = JsonConvert.DeserializeObject<List<Emoji>>(res_json, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            foreach(Emoji e in emojis)
            {
                //if (BinaryManager.ReadBin_To_Emoji(shortcode) == null)
                if( !BinaryManager.isEmojiCache(e.shortcode))
                {
                    byte[] imageBytes = null;
                    using(var webClient = new WebClient())
                    {
                        imageBytes = webClient.DownloadData(e.static_url);
                        if(imageBytes != null)
                        {
                            Bitmap b = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);//byte -> bitmpap
                            b = Bitmap.CreateScaledBitmap(b, 60, 60, false);
                            MemoryStream memoryStream = new MemoryStream();//byte[] stream
                            b.Compress(Bitmap.CompressFormat.Png, 100, memoryStream);//bitmap -> byte[]
                            BinaryManager.WriteBin_To_Emoji(e.shortcode, memoryStream.ToArray());
                        }
                    }
                }
            }

        }

        public List<EmojiPosition> EmojiPostions(string context)
        {
            List<EmojiPosition> emojiPositions = new List<EmojiPosition>();

            Regex reg = new Regex(":[0-9a-zA-Z_]{2,}:");
            var matchs = reg.Matches(context);
            string s = context;
            int i = 0;
            foreach (Match m in matchs)
            {
                string shortcode = m.Value.Substring(1, m.Value.Length - 2);

                EmojiPosition newEmojiPosition 
                    = new EmojiPosition((i + s.IndexOf(m.Value)), 
                    (i + s.IndexOf(m.Value) + m.Value.Length ), shortcode);
                emojiPositions.Add(newEmojiPosition);

                i += s.IndexOf(m.Value) + m.Value.Length;
                s = s.Substring(s.IndexOf(m.Value) + m.Value.Length);
            }

            return emojiPositions;
        }

    }

    [JsonObject("media")]
    public class Emoji
    {
        [JsonProperty("shortcode")]
        public string shortcode { get; set; }
        [JsonProperty("static_url")]
        public string static_url { get; set; }
    }

    public class EmojiPosition
    {
        public int start;
        public int end;
        public string shortcode;

        public EmojiPosition(int start, int end, string shortcode)
        {
            this.start = start;
            this.end = end;
            this.shortcode = shortcode;
        }

    }

}