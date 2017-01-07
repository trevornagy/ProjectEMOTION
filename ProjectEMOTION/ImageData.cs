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

namespace ProjectEMOTION
{
    public class ImageData
    {
        public FaceRectangle[] faceRectangle { get; set; }
        public Scores[] scores { get; set; }
    }
    public class FaceRectangle
    {
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
    public class Scores
    {
        public decimal anger { get; set; }
        public decimal contempt { get; set; }
        public decimal disgust { get; set; }
        public decimal fear { get; set; }
        public decimal happness { get; set; }
        public decimal neutral { get; set; }
        public decimal sadness { get; set; }
        public decimal surprise { get; set; }
    }
}