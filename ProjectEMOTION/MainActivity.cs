using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Plugin.Media;

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
        // Regular home page items
        private Button _btnTakePhoto;
        private Button _btnChoosePhoto;
        private Button _btnTutorial;
        private string _imageFileLocation;
        private TextView _txtTitle;
        private Android.App.ProgressDialog progress;

        // To simulate loading screen
        private ImageView _imgResult;
        private ProgressBar _progressLoad;
        private TextView _txtMessage;
        private TextView _txtPleaseWait;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            progress = new Android.App.ProgressDialog(this);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            _btnTakePhoto = FindViewById<Button>(Resource.Id.btnTakePhoto);
            _btnChoosePhoto = FindViewById<Button>(Resource.Id.btnChoosePhoto);
            _btnTutorial = FindViewById<Button>(Resource.Id.btnTutorial);
            _txtTitle = FindViewById<TextView>(Resource.Id.txtTitle);

            _progressLoad = FindViewById<ProgressBar>(Resource.Id.progressHomeLoad);
            _txtPleaseWait = FindViewById<TextView>(Resource.Id.txtHomePleaseWait);
            _txtMessage = FindViewById<TextView>(Resource.Id.txtHomeLoadingText);


            _btnTakePhoto.Click += _btnTakePhoto_Click;
            _btnChoosePhoto.Click += _btnChoosePhoto_Click;
            //_btnTutorial.Click += _btnTutorial_Click;
        }

        private async void _btnTakePhoto_Click(object sender, EventArgs e)
        {
            // Launch camera
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                Console.WriteLine("No Camera", ":( No camera available.", "OK");
                //progress.Dismiss();
                return;
            }

            //progress.Indeterminate = true;
            //progress.SetCancelable(false);
            //progress.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
            //progress.SetTitle("Please wait...");
            //progress.Show();


            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "temp",
                CompressionQuality = 25,
                SaveToAlbum = false
            });

            if (file == null)
            {
                //progress.Dismiss();
                return;
            }

            string filePath = file.Path;
            file.Dispose();

            var TakePhoto = new Android.Content.Intent(this, typeof(TakePhotoActivity));
            TakePhoto.PutExtra("imageLocation", filePath);
            StartActivity(TakePhoto);
        }

        private async void _btnChoosePhoto_Click(object sender, EventArgs e)
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

            var TakePhoto = new Android.Content.Intent(this, typeof(TakePhotoActivity));
            TakePhoto.PutExtra("imageLocation", filePath);
            StartActivity(TakePhoto);
        }

        protected override void OnResume()
        {
            base.OnResume();
            progress.Dismiss();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
            
}

