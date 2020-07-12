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
using Android.Support.V4.App;

namespace Taroedon
{
    class StatusDeckAdapter : FragmentPagerAdapter
    {
        public StatusDeck statusDeck;

        public StatusDeckAdapter(Android.Support.V4.App.FragmentManager fm, StatusDeck _statusDeck)
            :base(fm)
        {
            this.statusDeck = _statusDeck;
        }

        public override int Count { get { return statusDeck.NumTimeLines; } }

        public override Android.Support.V4.App.Fragment GetItem(int position)
        {
            switch (position)
            {
                case 0:
                    return (Android.Support.V4.App.Fragment) StatusFragment.newInstance(statusDeck[position]);
                case 1:
                    return (Android.Support.V4.App.Fragment)StatusFragment2.newInstance();
                default:
                    return (Android.Support.V4.App.Fragment)StatusFragment3.newInstance(statusDeck[position]);
            }

        }
    }
}