using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.App;
using Android.Support.V7.App;
using System.Threading.Tasks;

namespace Taroedon
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        private static string TOOLBAR_TITLE;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            try
            {
                //setting load
                var pref = GetSharedPreferences("SETTING", FileCreationMode.Private);
                ColorDatabase.mode = pref.GetBoolean("theme", false);

                UserAction.SettingsLoad(pref);

                pref = GetSharedPreferences("USER", FileCreationMode.Private);//file name + style
                string instance = pref.GetString("instance", "");
                string clientId = pref.GetString("clientId", "");
                string clientSecret = pref.GetString("clientSecret", "");
                string accessToken = pref.GetString("accessToken", "");
                string redirectUri = pref.GetString("redirectUri", "");
                if (instance == "" || clientId == ""
                    || clientSecret == "" || accessToken == "" || redirectUri == "")
                {
                    Intent intent1 = new Intent(this, typeof(SettingsActivity));
                    StartActivity(intent1);
                    Finish();
                }

                //check networking
                NetworkStatus networkStatus = new NetworkStatus(this.ApplicationContext);
                if (networkStatus.IsNetwork())
                {
                    //userdata set
                    var _cl = UserClient.getInstance();
                    _cl.setClient(instance, clientId, clientSecret, accessToken, redirectUri);
                }
                else
                {
                    throw new Java.Net.UnknownHostException("offline");
                }

            }
            catch (Java.Net.UnknownHostException e)
            {
                UserAction.Toast_BottomFIllHorizontal_Show("ネットワークの接続状態が悪いようです\n時間をおいて起動してください", this, ColorDatabase.INFO);
                this.Finish();
            }
            catch (Exception e)
            {
                UserAction.Toast_BottomFIllHorizontal_Show("ユーザ情報が破損したようです．\nすいませんが再登録してください．．．", this, ColorDatabase.INFO);
                Intent intent1 = new Intent(this, typeof(SettingsActivity));
                StartActivity(intent1);
                Finish();
            }

            try
            {
                //emoji load
                if (!string.IsNullOrEmpty(UserClient.instance))
                {
                    EmojiGetTask emojiGetTask = new EmojiGetTask();
                    emojiGetTask.InitEmojiListAsync();
                }
            }
            catch(Java.Net.UnknownHostException e)
            {
                UserAction.Toast_BottomFIllHorizontal_Show("ネットワークの接続状態が悪いようです\n時間をおいて起動してください", this, ColorDatabase.INFO);
                this.Finish();
            }
            catch(Exception e)
            {
                UserAction.Toast_BottomFIllHorizontal_Show("ネットワークの接続状態が悪いようです\n時間をおいて起動してください", this, ColorDatabase.INFO);
                Finish();
            }


            //fragment make -> push
            StatusDeck statusDeck = new StatusDeck();
            StatusDeckAdapter adapter = new StatusDeckAdapter(SupportFragmentManager, statusDeck);
            ViewPager pager = (ViewPager)FindViewById(Resource.Id.pager);
            pager.SetBackgroundColor(ColorDatabase.TL_BACK);
            pager.Adapter = adapter;

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);//on
            if (UserAction.bDisplay)
            {
                Window.AddFlags(WindowManagerFlags.KeepScreenOn);//on
            }
            else
            {
                Window.ClearFlags(WindowManagerFlags.KeepScreenOn);//off
            }


            //toolbar
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            //toolbar title
            //SupportActionBar.Title = "Home   ";//init
            pager.PageSelected += (sender, e) =>
            {
                string state_title;
                switch (e.Position)
                {
                    case 0: state_title = "Home"; break;
                    case 1: state_title = "Mention"; break;
                    default: state_title = "Public"; break;
                }
                SupportActionBar.Title = state_title + "   " + TOOLBAR_TITLE;
            };
            Setting_ToolbarTitle();
            
            //FAB
            var fabButton = FindViewById<Android.Support.Design.Widget.FloatingActionButton>(Resource.Id.fab);
            fabButton.Click+=(sender, e) =>
            {
                var intent = new Intent(this, typeof(PostStatusActivity));
                this.StartActivity(intent);
            };

        }


        /***************************************************************
         *                   toolbar
        **************************************************************/
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                Intent intent = new Intent(this, typeof(SettingListActivity));
                //StartActivity(intent);
                StartActivityForResult(intent, 0);
            }

            return base.OnOptionsItemSelected(item);
        }
        //title
        private async Task Setting_ToolbarTitle()
        {
            var currentuser = await UserClient.getInstance().getClient().GetCurrentUser();
            TOOLBAR_TITLE = currentuser.DisplayName + "@" + currentuser.AccountName;
            UserClient.currentAccountName = currentuser.AccountName;//currentAccountNameを使い回す

            ViewPager pager = (ViewPager)FindViewById(Resource.Id.pager);
            int n = pager.CurrentItem;
            switch (n)
            {
                case 0: SupportActionBar.Title = "Home"; break;
                case 1: SupportActionBar.Title = "Mention"; break;
                default: SupportActionBar.Title = "Public"; break;
            }
            SupportActionBar.Title += "   "+ TOOLBAR_TITLE;
        }


        /***************************************************************
         *                   Activity ret data
        **************************************************************/
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case 0:
                    //Display On
                    if(resultCode == Result.Ok)
                    {
                        Window.AddFlags(WindowManagerFlags.KeepScreenOn);//on
                    }
                    //Display off
                    else
                    {
                        Window.ClearFlags(WindowManagerFlags.KeepScreenOn);//off
                    }
                    break;
                default: break;

            }
        }
    }
}

