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

        //************************************************************************************************************
        //////////dev用
        //string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
        //string instance = "taroedon.com";

        //string clientId = "a5fa1fb490b3c9eafab86811fa8787aa6710df7fd910e0ea69a53d864ebdb265";
        //string clientSecret = "2f184f41f0a38e3b67d5c5ffaabd6d4ff44d8369e5bc92a2cda7df8cc1f98315";
        //string accessToken = "66f878facb670d552fbd6a3d71b3649ea3905fa8ce4dea5370bb6876d681de6a";
        //*************************************************************************************************************
        //////////管理者のアカウント
        //public string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
        //public static string instance = "taroedon.com";

        //string clientId = "ec0a2ec41e95ce3d9b1b4c3e845b6225bf7eb14fcde316fb0c19075cc0c76ac1";
        //string clientSecret = "46c05f8206a6bc44c0977efe145b79de08ea0327194ff989467e96c222dd9b68";
        //string accessToken = "ec34c7783a1f782db47d1f5d4677349dda138826434bb45d5627436574116822";
        //*************************************************************************************************************
        ////////wokerのほう
        //string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
        //string instance = "mstdn-workers.com";

        //string clientId = "1643bcd644d53a93c8e83ff37ac7939dc19c6e5b1c6301172b7d9db5070013c5";
        //string clientSecret = "c7613429530b15463a64a7a7fdc07c9e2dbe2db7a26ecbf3e2e9fd62373be21c";
        //string accessToken = "f57843c2f5739ef5908f7f69ab420e2b92bea1d967f23d2389206f8473e544d0";
        //*************************************************************************************************************

        //指定番号の読み取り
        //データの保存

        public MastodonClient getClient()
        {
            //AuthenticationClient authClient = new AuthenticationClient(instance);
            //var appRegistration = await authClient.CreateApp("taroetest", Scope.Read | Scope.Write | Scope.Follow);

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

            //var auth = await authClient.ConnectWithPassword("wknine99@yahoo.co.jp", "ww9012ww");
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

