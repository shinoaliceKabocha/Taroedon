﻿using System;
using System.Collections.Generic;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Mastonet.Entities;

namespace Taroedon
{
    public class LocalLinkMovementMethod : Android.Text.Method.LinkMovementMethod
    {
        public override bool OnTouchEvent(TextView widget, ISpannable buffer, MotionEvent e)
        {
            var action = e.Action;
            if (action == MotionEventActions.Down || action == MotionEventActions.Up)
            {
                int x = (int)e.GetX();
                int y = (int)e.GetY();

                x -= widget.TotalPaddingLeft;
                y -= widget.TotalPaddingTop;

                x += widget.ScrollX;
                y += widget.ScrollX;

                var layout = widget.Layout;
                int line = layout.GetLineForVertical(y);
                int off = layout.GetOffsetForHorizontal(line, x);

                Type type = typeof(ClickableSpan);
                var link = buffer.GetSpans(off, off, Java.Lang.Class.FromType(type));

                if (link.Length != 0)
                {
                    if (action == MotionEventActions.Up)
                    {
                        if (link[0] is URLSpan)
                        {
                            string url = ((URLSpan)link[0]).URL;
                            View view = (View)widget;
                            UserAction.UrlOpen(url, view);
                        }
                    }
                    else if (action == MotionEventActions.Down)
                    {
                        Selection.SetSelection(buffer, buffer.GetSpanStart(link[0]), buffer.GetSpanEnd(link[0]));
                    }
                    return true;
                }
                else
                {
                    Selection.RemoveSelection(buffer);
                }

            }
            return base.OnTouchEvent(widget, buffer, e);


        }
    }

}