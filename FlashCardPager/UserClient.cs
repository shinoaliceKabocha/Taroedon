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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlashCardPager
{
    public class UserClient
    {
        public static string redirectUri { get; set; }
        public static string instance { get; set; }
        public static string clientId { get; set; }
        public static string clientSecret { get; set; }
        public static string accessToken { get; set; }
        public static string currentAccountName { get; set; }

        //指定番号の読み取り
        //データの保存
        public MastodonClient getClient()
        {
            //設定番号の読み取り→ファイル展開しておく
            var appRegistration = new AppRegistration
            {
                Id = 39,
                RedirectUri = redirectUri,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Instance = instance,
                Scope = Scope.Write | Scope.Read | Scope.Follow

            };

            var auth = new Auth
            {
                AccessToken = accessToken,
                CreatedAt = "1534704318",
                Scope = "read write follow",
                TokenType = "bearer"
            };

            var client = new MastodonClient(appRegistration, auth);

            return client;
        }

        public void setClient(string _instance, string _clientId, string _clientSecret, string _accessToken, string _redirectUri)
        {
            instance = _instance;
            clientId = _clientId;
            clientSecret = _clientSecret;
            accessToken = _accessToken;
            redirectUri = _redirectUri;
        }

    }

}

