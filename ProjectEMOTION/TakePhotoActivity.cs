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

namespace ProjectEMOTION
{
    [Activity(Label = "TakePhotoActivity")]
    public class TakePhotoActivity : Activity
    {
        private ImageView _imgResult;
        private ProgressBar _progressLoad;
        private string _imageFileLocation;
        const string _apiURL = "https://api.projectoxford.ai/emotion/v1.0/recognize";
        const string _apiKey = "6ef65328819442a6aebc8de396b69f1b";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // RequestWindowFeature(WindowFeatures.NoTitle); - Remove when image is on page

            _imageFileLocation = Intent.GetStringExtra("imageLocation") ?? "Data not available";
            Console.WriteLine(_imageFileLocation);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Results);

            // Convert to byte array so we can send it to the mifrosoft API
            byte[] byteArray = System.IO.File.ReadAllBytes(_imageFileLocation);

            // Create your application here
            _imgResult = FindViewById<ImageView>(Resource.Id.imgResults);
            _progressLoad = FindViewById<ProgressBar>(Resource.Id.progressLoad);
            AccessApi(_apiURL , _apiKey, byteArray);
        }

        public async void AccessApi(string url, string key, byte[] image)
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
    }
}
