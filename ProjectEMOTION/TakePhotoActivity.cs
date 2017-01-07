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
using System.Threading;
using Newtonsoft.Json;

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
            System.Console.WriteLine(_imageFileLocation);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Results);
            // Convert to byte array so we can send it to the microsoft API

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

            byte[] byteArray = File.ReadAllBytes(_imageFileLocation);

            var results = await AccessApi(_apiURL, _apiKey, byteArray);
            Console.WriteLine(results);

            // Figure out how to deserialize data
            //ImageData tmp = JsonConvert.DeserializeObject<ImageData>(results);

            //foreach (string typeStr in tmp)
            //{
            //    // Do something with typeStr
            //}

            _imgResult.Visibility = ViewStates.Visible;
            _progressLoad.Visibility = ViewStates.Gone;
            _txtPleaseWait.Visibility = ViewStates.Gone;
            _txtWaitMessage.Visibility = ViewStates.Gone;
        }

        public async Task<String> AccessApi(string url, string key, byte[] image)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", key);
            request.JsonSerializer.ContentType = "application/octet-stream";
            request.AddParameter("application/octet-stream", image, ParameterType.RequestBody);

            var cancellationTokenSource = new CancellationTokenSource();

            // Async execute request
            var restResponse = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            // Return deserialize string
            return restResponse.Content;
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
    }
}
