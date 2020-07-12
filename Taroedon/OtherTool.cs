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

namespace Taroedon
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

            return _content;
        }

        public static List<string> DLG_ITEM_getURL(Status status)
        {
            List<string> additemlist = new List<string>();
            //get url in content
            Regex reg = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?@%&=]*)?");
            var matches = reg.Matches(status.Content);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    if (match.Value != "" && match.Length > 8)
                    {
                        if (!additemlist.Contains(match.Value))
                        {
                            additemlist.Add(match.Value);
                        }
                    }
                }
            }

            try
            {
                var urls = status.MediaAttachments;
                foreach (var url in urls)
                {
                    if (!string.IsNullOrWhiteSpace(url.PreviewUrl) && url.PreviewUrl.Length > 8)
                    {
                        if (UserAction.bImageQuality && url.RemoteUrl != null)
                        {
                            additemlist.Add(url.RemoteUrl);
                        }
                        else
                        {
                            additemlist.Add(url.PreviewUrl);
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return additemlist;
        }

        public static List<string> ImageUrlPreviewfromStatus (Status status)
        {
            var imageUrls = status.MediaAttachments;
            List<string> rtns = new List<string>();
            foreach(var url in imageUrls)
            {
                rtns.Add(url.PreviewUrl);
            }

            try
            {
                return rtns;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> ImageUrlRemotefromStatus(Status status)
        {
            var imageUrls = status.MediaAttachments;
            List<string> rtns = new List<string>();
            foreach (var url in imageUrls)
            {
                try
                {
                    if (url.RemoteUrl != null)
                    {
                        rtns.Add(url.RemoteUrl);
                    }
                    else
                    {
                        rtns.Add(url.PreviewUrl);
                    }
                }
                catch(Exception e)
                {
                    rtns.Add(url.PreviewUrl);
                }
            }

            try
            {
                return rtns;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

}