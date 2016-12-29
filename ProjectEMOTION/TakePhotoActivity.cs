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
using Microsoft.ProjectOxford.Emotion;

namespace ProjectEMOTION
{
    [Activity(Label = "TakePhotoActivity")]
    public class TakePhotoActivity : Activity
    {
        static string strImageLocation;
        private ImageView imgFace;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Results);

            // Create your application here
            strImageLocation = Intent.GetStringExtra("filePath") ?? "Data not available";
            Android.Net.Uri imageLocation = Android.Net.Uri.Parse(strImageLocation);

            Console.WriteLine(strImageLocation);

            imgFace = FindViewById<ImageView>(Resource.Id.imgFace);
            imgFace.SetImageURI(imageLocation);
        }
    }
}