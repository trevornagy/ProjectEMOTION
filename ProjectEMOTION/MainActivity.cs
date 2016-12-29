using Android.App;
using Android.Widget;
using Android.OS;
using Plugin.Media;
using System;

/* 
 To-Do:
     - Make a new page tabbed page, tabs only appear once progress bar has dissapeared and results are ready
     - Show results and smart people results in their tabs
     - Refactor pick and take photo to use function
     - Check for network connection
     */


namespace ProjectEMOTION
{
    [Activity(Label = "ProjectEMOTION", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Button _btnTakePhoto;
        private Button _btnChoosePhoto;
        private Button _btnTutorial;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            _btnTakePhoto = FindViewById<Button>(Resource.Id.btnTakePhoto);
            _btnChoosePhoto = FindViewById<Button>(Resource.Id.btnChoosePhoto);
            _btnTutorial = FindViewById<Button>(Resource.Id.btnTutorial);

            _btnTakePhoto.Click += _btnTakePhoto_Click;
            _btnChoosePhoto.Click += _btnChoosePhoto_Click;
            _btnTutorial.Click += _btnTutorial_Click;
        }

        private void _btnTutorial_Click(object sender, System.EventArgs e)
        {
            // New page
            throw new System.NotImplementedException();
        }

        private async void _btnChoosePhoto_Click(object sender, System.EventArgs e)
        {
            // Choose from gallery
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Console.WriteLine("No Gallery", ":( No gallery available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync();

            if (file == null)
                return;

            Console.WriteLine(file.Path);

            string filePath = file.Path;

            file.Dispose();
        }

        private async void _btnTakePhoto_Click(object sender, System.EventArgs e)
        {
            // Launch camera
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                Console.WriteLine("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "temp",
                Name = "facetotest.jpg",
                SaveToAlbum = false,
            });

            if (file == null)
                return;

            //Console.WriteLine(file.Path);

            string filePath = file.Path;

            file.Dispose();
            var intent = new Android.Content.Intent(this, typeof(TakePhotoActivity));
            intent.PutExtra("filePath", filePath);
            StartActivity(intent);

            // After this call function that opens new page and submits it to the microsoft api

        }
    }
            
}

