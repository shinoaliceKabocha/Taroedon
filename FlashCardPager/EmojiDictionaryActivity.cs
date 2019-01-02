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
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace FlashCardPager
{
    [Activity(Label = "EmojiDictionaryActivity", Theme = "@android:style/Theme.Material.Light.Dialog.NoActionBar")]
    public class EmojiDictionaryActivity : Activity, ITextWatcher
    {
        ListView listView;
        CustomListAdapter customListAdapter;
        List<EmojiItem> emojiItems, allEmojiItems;
        EditText searchEditText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.EmojiDictionary);
            // Create your application here
            //絵文字辞書内容を全部取得する
            emojiItems = GetEmojiItems();
            allEmojiItems = emojiItems;//all set

            listView = FindViewById<ListView>(Resource.Id.listViewEmojiDictionary);
            listView.SetBackgroundColor(ColorDatabase.TL_BACK);
            customListAdapter = new CustomListAdapter(this, emojiItems);
            listView.Adapter = customListAdapter;
            customListAdapter.NotifyDataSetChanged();

            listView.ItemClick += (sender, e) =>
            {
                int p = e.Position - 1;
                var emojiItem = customListAdapter[p];

                Intent intent = new Intent();
                intent.PutExtra("shortcode", emojiItem.Shortcode);
                SetResult(Result.Ok, intent);

                Finish();
            };

            //search text
            searchEditText = new EditText(this);
            searchEditText.AddTextChangedListener(this);
            searchEditText.SetSingleLine();
            searchEditText.Hint = "Input search word...";
            searchEditText.SetHintTextColor(ColorDatabase.TLTEXT);
            searchEditText.SetTextColor(ColorDatabase.TLTEXT);
            listView.AddHeaderView(searchEditText);
        }

        private List<EmojiItem> GetEmojiItems()
        {
            List<EmojiItem> rtn = new List<EmojiItem>();
            string emojiPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Emoji");
            try
            {
                if (Directory.Exists(emojiPath))
                {
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

        private List<EmojiItem> GetEmojiItems(string key)
        {
            if (allEmojiItems == null || allEmojiItems.Count == 0)
            {
                allEmojiItems = GetEmojiItems();
            }

            List<EmojiItem> rtn = new List<EmojiItem>();
            foreach (var s in allEmojiItems)
            {
                if (s.Shortcode.Contains(key)) rtn.Add(s);
            }
            return rtn;
        }

        //textwatcher
        public void AfterTextChanged(IEditable s)
        {
            customListAdapter.clear();
            emojiItems = GetEmojiItems(s.ToString());
            customListAdapter.addAll(emojiItems);
            customListAdapter.NotifyDataSetChanged();

        }
        public void BeforeTextChanged(ICharSequence s, int start, int count, int after) { }
        public void OnTextChanged(ICharSequence s, int start, int before, int count) { }
        //textwatcher

    }


    /* text + Bitmap adapter */
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

            shortCode.SetTextColor(ColorDatabase.TLTEXT);

            return view;
        }

        public void clear()
        {
            emojiItems.Clear();
        }
        public void addAll(List<EmojiItem> ts)
        {
            if (emojiItems == null) emojiItems = new List<EmojiItem>();
            foreach(var t in ts)
            {
                emojiItems.Add(t);
            }
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