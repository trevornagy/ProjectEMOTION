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
using RestSharp;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Graphics.Drawables;
using System.IO;

namespace ProjectEMOTION
{
    [Activity(Label = "Results")]
    public class TakePhotoActivity : Activity
    {
        private ImageView _imgResult;
        private ProgressBar _progressLoad;
        private TextView _txtPleaseWait;
        private TextView _txtWaitMessage;

        private string _imageFileLocation;
        const string _apiURL = "https://api.projectoxford.ai/emotion/v1.0/recognize";
        const string _apiKey = "6ef65328819442a6aebc8de396b69f1b";

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            // RequestWindowFeature(WindowFeatures.NoTitle); - Remove when image is on page

            _imageFileLocation = Intent.GetStringExtra("imageLocation") ?? "Data not available";
            Console.WriteLine(_imageFileLocation);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Results);
            // Convert to byte array so we can send it to the microsoft API
            // byte[] byteArray = System.IO.File.ReadAllBytes(_imageFileLocation);

            // Create your application here
            _imgResult = FindViewById<ImageView>(Resource.Id.imgResults);
            _progressLoad = FindViewById<ProgressBar>(Resource.Id.progressLoad);
            _txtPleaseWait = FindViewById<TextView>(Resource.Id.txtPleaseWait);
            _txtWaitMessage = FindViewById<TextView>(Resource.Id.txtLoadingText);

            var metrics = Resources.DisplayMetrics;
            double heightImageView = metrics.HeightPixels * 0.6D;
            double widthImageView = metrics.WidthPixels * 0.6D;

            BitmapFactory.Options options = await GetImageSizeAsync();
            Bitmap bitmapToDisplay = await LoadScaledDownBitmapForDisplayAsync(_imageFileLocation, options, (int)heightImageView, (int)widthImageView);
            _imgResult.SetImageBitmap(bitmapToDisplay);

            System.IO.Stream stream = new System.IO.MemoryStream();
            bitmapToDisplay.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            byte[] byteArray = null;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                byteArray = memoryStream.ToArray();
            }

            AccessApi(_apiURL, _apiKey, byteArray);

            stream.Dispose();

            _imgResult.Visibility = ViewStates.Visible;
            _progressLoad.Visibility = ViewStates.Gone;
            _txtPleaseWait.Visibility = ViewStates.Gone;
            _txtWaitMessage.Visibility = ViewStates.Gone;


        }

        private byte[] ReadAllBytes(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void AccessApi(string url, string key, byte[] image)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", key);
            request.JsonSerializer.ContentType = "application/octet-stream";
            request.AddParameter("application/octet-stream", image, ParameterType.RequestBody);
            //request.AddFile("file", imageLocation);
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            Console.WriteLine(content);
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetMessage(content);
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        async Task<BitmapFactory.Options> GetImageSizeAsync()
        {
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InJustDecodeBounds = true
            };

            Bitmap result = await BitmapFactory.DecodeFileAsync(_imageFileLocation, options);
            int imageHeight = options.OutHeight;
            int imageWidth = options.OutWidth;
            return options;
        }
        public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // original height and width of image
            float height = options.OutHeight;
            float width = options.OutWidth;
            double inSampleSize = 1D;

            if (height > reqHeight || width > reqWidth)
            {
                int halfHeight = (int)(height / 2);
                int halfWidth = (int)(width / 2);

                while ((halfHeight / inSampleSize) > reqHeight && (halfWidth / inSampleSize) > reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return (int)inSampleSize;

        }
        public async Task<Bitmap> LoadScaledDownBitmapForDisplayAsync(string fileLocation, BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);
            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;

            return await BitmapFactory.DecodeFileAsync(fileLocation, options);
        }
        //public void GetImageViewDimensions(ImageView imageView)
        //{
        //    int finalHeight, finalWidth;

        //    ViewTreeObserver viewTree = imageView.getViewTreeObserver();
        //    viewTree.addOnPreDrawListener(new ViewTreeObserver.OnPreDrawListener()
        //    {
        //        public Boolean onPreDraw()
        //            {
        //                iv.getViewTreeObserver().removeOnPreDrawListener(this);
        //                finalHeight = imageView.getMeasuredHeight();
        //                finalWidth = imageView.getMeasuredWidth();
        //                return true;
        //            }
        //    });
        //}
    }
}
