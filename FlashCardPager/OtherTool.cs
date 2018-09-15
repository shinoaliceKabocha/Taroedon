using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mastonet;
using Mastonet.Entities;

namespace FlashCardPager
{
    public static class OtherTool
    {
        public static string HTML_removeTag(string content)
        {
            string _content = content;
            _content = _content.Replace("<br />", "\r\n");
            _content = _content.Replace("<br/>", "\r\n");
            _content = _content.Replace("< p >", "");
            _content = _content.Replace("</ p >", "");
            _content = _content.Replace("<p>", "");
            _content = _content.Replace("</p>", "");
            _content = Regex.Replace(_content, "<a [^>]*?</a>", "");
            _content = Regex.Replace(_content, "<[^>]*?>", "");

            //Regex reg = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            //var matches = reg.Matches(_content);
            //foreach(Match m in matches)
            //{
            //    //_content = _content.Replace(m.Value, "");
            //}

            return _content;
        }

        public static List<string> DLG_ITEM_getURL(Status status)
        {
            List<string> additemlist = new List<string>();
            //content内部のURLを取得する
            Regex reg = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
            var matches = reg.Matches(status.Content);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    if (match.Value != "" && match.Length > 8)
                    {
                        additemlist.Add(match.Value);
                    }
                }
            }

            //画像とか
            try
            {
                var urls = status.MediaAttachments;
                foreach (var url in urls)
                {
                    if (url.PreviewUrl != "" && url.PreviewUrl.Length > 8)
                    {
                        additemlist.Add(url.PreviewUrl);
                    }
                }
            }
            catch (Exception ex) { }

            return additemlist;
        }

        public static string ImageUrl_x_from_Status (Status status, int n)
        {
            var imageUrls = status.MediaAttachments;
            List<string> rtns = new List<string>();
            foreach(var url in imageUrls)
            {
                if (url.PreviewUrl.Contains("media")) rtns.Add(url.PreviewUrl);
                if (n + 1 == rtns.Count) return rtns[n];
            }

            try
            {
                return rtns[n];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

}