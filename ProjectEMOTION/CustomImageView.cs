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
using Android.Graphics;
using Android.Util;

namespace ProjectEMOTION
{
    class CustomImageView : ImageView
    {
        Context mContext;
        public CustomImageView(Context context) : base(context)
		{
            init(context);
        }
        public CustomImageView(Context context, IAttributeSet attrs) :
			base(context, attrs)
			{
            init(context);
        }

        public CustomImageView(Context context, IAttributeSet attrs, int defStyle) :
			base(context, attrs, defStyle)
			{
            init(context);
        }

        private void init(Context ctx)
        {
            mContext = ctx;
        }

        public class RectangleData
        {
            public string rectangleImageData { get; set; }
        }

        protected override void OnDraw(Canvas canvas)
        {
            ImageData data = new ImageData();

            base.OnDraw(canvas);
        }

        private void drawRect(Canvas canvas)
        {
            throw new NotImplementedException();
        }
    }
}