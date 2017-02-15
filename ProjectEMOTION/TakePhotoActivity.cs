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
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Android.Media;
using Android.Util;

namespace ProjectEMOTION
{
    [Activity(Label = "Results")]
    public class TakePhotoActivity : Activity
    {
        private ImageView _imgResult;
        private ProgressBar _progressLoad;
        private TextView _txtPleaseWait;
        private TextView _txtWaitMessage;
        private TextView _txtResult;
        string tag = "ProjectEMOTION";

        List<RectF> _rectangles = new List<RectF>();

        private string _imageFileLocation;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            // RequestWindowFeature(WindowFeatures.NoTitle);// - Remove when image is on page

            _imageFileLocation = Intent.GetStringExtra("imageLocation") ?? "Data not available";

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Results);
            
            // Create your application here
            _imgResult = FindViewById<ImageView>(Resource.Id.imgResults);
            _progressLoad = FindViewById<ProgressBar>(Resource.Id.progressLoad);
            _txtPleaseWait = FindViewById<TextView>(Resource.Id.txtPleaseWait);
            _txtWaitMessage = FindViewById<TextView>(Resource.Id.txtLoadingText);
            _txtResult = FindViewById<TextView>(Resource.Id.txtResults);

            var metrics = Resources.DisplayMetrics;
            double heightImageView = metrics.HeightPixels * 0.6D;
            double widthImageView = metrics.WidthPixels * 0.6D;

            BitmapFactory.Options options = await GetImageSizeAsync();
            Bitmap bitmapToDisplay = await LoadScaledDownBitmapForDisplayAsync(_imageFileLocation, options, (int)heightImageView, (int)widthImageView);

            // Delete original image
            Java.IO.File originalImage = new Java.IO.File(_imageFileLocation);
            bool deleted = originalImage.Delete();

            // Set location to NEW scaled down location
            _imageFileLocation = await getImageUri(Application.Context, bitmapToDisplay);
            Log.Info(tag, _imageFileLocation);

            // Convert to byte array so we can send it to the microsoft API
            byte[] byteArray = File.ReadAllBytes(_imageFileLocation);

            var results = await AccessApi(byteArray);

            // Deserialize data async
            var task = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<ImageData>>(results));
            var imageResults = await task;
            // List<ImageData> imageResults = JsonConvert.DeserializeObject<List<ImageData>>(results); <-- This isn't async

            Paint paint = new Paint();
            paint.Color = Color.Blue;
            paint.SetStyle(Paint.Style.Stroke);
            paint.StrokeWidth = 5;
            Bitmap drawableBitmap = bitmapToDisplay.Copy(Bitmap.Config.Argb8888, true);
            bitmapToDisplay.Recycle();
            Canvas canvas = new Canvas(drawableBitmap);

            RectF rect;

            foreach (ImageData data in imageResults)
            {
                canvas.DrawBitmap(drawableBitmap, 0, 0, null);
                rect = new RectF(data.faceRectangle.left, data.faceRectangle.top, (data.faceRectangle.width + data.faceRectangle.left), (data.faceRectangle.top + data.faceRectangle.height));
                canvas.DrawRect(rect, paint);
                _rectangles.Add(rect);
                //_rectangles.Add(new Rect(data.faceRectangle.left, data.faceRectangle.top, (data.faceRectangle.width + data.faceRectangle.left), (data.faceRectangle.top + data.faceRectangle.height)));
            }

            _imgResult.SetImageBitmap(drawableBitmap);
            // drawableBitmap.Recycle();

            _progressLoad.Visibility = ViewStates.Gone;
            _txtPleaseWait.Visibility = ViewStates.Gone;
            _txtWaitMessage.Visibility = ViewStates.Gone;
            _imgResult.Visibility = ViewStates.Visible;
            _txtResult.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();
            _txtResult.Visibility = ViewStates.Visible;
            _txtResult.Text = results;
        }

        //public override bool OnTouchEvent(MotionEvent e)
        //{
        //    MotionEventActions action = e.Action & MotionEventActions.Mask;

        //    switch (action)
        //    {
        //        case MotionEventActions.Down:

        //            float touchX = e.GetX();
        //            float touchY = e.GetY();
        //            Console.WriteLine(touchX);
        //            Console.WriteLine(touchY);

        //            foreach (RectF rect in _rectangles)
        //            {
        //                Console.WriteLine(rect);
        //                Console.WriteLine(rect.Contains(touchX, touchY));
        //                if (rect.Contains(touchX, touchY))
        //                {
        //                    Console.WriteLine("You touched a rectangle");
        //                    _txtResult.Visibility = ViewStates.Visible;
        //                }
        //            }
        //            break;
        //    }
        //    return base.OnTouchEvent(e);
        //}
        // Get new location for byteArray
        public async Task<string> getImageUri(Context inContext, Bitmap inImage)
        {
            string sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Android/data/ProjectEMOTION.ProjectEMOTION/files/Pictures/temp";
            var path = System.IO.Path.Combine(sdCardPath, "image.jpg");
            var stream = new FileStream(path, FileMode.Create);
            await inImage.CompressAsync(Bitmap.CompressFormat.Jpeg, 25, stream);
            stream.Close();

            return path.ToString();
        }

        //public string GetRealPathFromURI(Android.Net.Uri contentUri)
        //{
        //    var mediaStoreImagesMediaData = "_data";
        //    string[] projection = { mediaStoreImagesMediaData };
        //    Android.Database.ICursor cursor = this.ManagedQuery(contentUri, projection, null, null, null);
        //    int columnIndex = cursor.GetColumnIndexOrThrow(mediaStoreImagesMediaData);
        //    cursor.MoveToFirst();
        //    return cursor.GetString(columnIndex);
        //}

        public async Task<String> AccessApi(Byte[] image)
        {
            const string url = "https://api.projectoxford.ai/emotion/v1.0/recognize";
            const string key = "6ef65328819442a6aebc8de396b69f1b";

            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", key);
            request.JsonSerializer.ContentType = "application/octet-stream";
            request.AddParameter("application/octet-stream", image, ParameterType.RequestBody);

            var cancellationTokenSource = new CancellationTokenSource();

            // Async execute request
            var restResponse = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            // Return string
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
