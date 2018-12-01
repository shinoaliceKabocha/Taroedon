using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FlashCardPager
{
    [Activity(Label = "EmojiDictionaryActivity", Theme = "@android:style/Theme.Material.Light.Dialog.NoActionBar")]
    public class EmojiDictionaryActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.EmojiDictionary);
            // Create your application here
            //絵文字辞書内容を全部取得する
            List<EmojiItem> emojiItems = GetEmojiItems();

            var listView = FindViewById<ListView>(Resource.Id.listViewEmojiDictionary);
            CustomListAdapter customListAdapter = new CustomListAdapter(this, emojiItems);
            listView.Adapter = customListAdapter;
            customListAdapter.NotifyDataSetChanged();

            listView.ItemClick += (sender, e) =>
            {
                var emojiItem = customListAdapter[e.Position];

                Intent intent = new Intent();
                intent.PutExtra("shortcode", emojiItem.Shortcode);
                SetResult(Result.Ok, intent);

                Finish();
            };

        }

        private List<EmojiItem> GetEmojiItems()
        {
            List<EmojiItem> rtn = new List<EmojiItem>();
            string emojiPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Emoji");
            try
            {
                if (Directory.Exists(emojiPath))
                {
                    //string[] filePaths = Directory.GetFiles(emojiPath);
                    string[] filePaths = Directory.GetFiles(emojiPath);
                    foreach (string filePath in filePaths)
                    {
                        string[] split = filePath.Split('/');
                        string shortcode = split[split.Length - 1];
                        EmojiItem addEmojiItem 
                            = new EmojiItem(shortcode, BinaryManager.ReadImage_To_Emoji(shortcode));
                        rtn.Add(addEmojiItem);
                    }
                }
            }
            catch(IOException e)
            {
                Android.Util.Log.Error("emoji:", e.StackTrace);
            }

            return rtn;
        }
    }

    public class CustomListAdapter : BaseAdapter<EmojiItem>
    {
        List<EmojiItem> emojiItems;
        Activity context;

        public CustomListAdapter(Activity context, List<EmojiItem> emojiItems)
        {
            this.emojiItems = emojiItems;
            this.context = context;
        }

        public override EmojiItem this[int position] => emojiItems[position];

        public override int Count => emojiItems.Count;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var emojiItem = emojiItems[position];
            View view = convertView;

            view = context.LayoutInflater.Inflate(Resource.Layout.EmojiDictionaryLine, null);
            var icon = view.FindViewById<ImageView>(Resource.Id.imageViewEmoji);
            var shortCode = view.FindViewById<TextView>(Resource.Id.textViewShortCode);

            //Bitmap bitmap = BitmapFactory.DecodeByteArray(emojiItem.emojiByte, 0, emojiItem.emojiByte.Length);
            Bitmap bitmap = emojiItem.EmojiBitmap;

            icon.SetImageBitmap(bitmap);
            shortCode.Text = emojiItem.Shortcode;

            return view;
        }
    }

    public class EmojiItem
    {
        public EmojiItem(string shortCode, Bitmap emojiBitmap)
        {
            this.EmojiBitmap = emojiBitmap;
            this.Shortcode = shortCode;
        }
        public string Shortcode { get; set; }
        public Bitmap EmojiBitmap { get; set; }
    }
}