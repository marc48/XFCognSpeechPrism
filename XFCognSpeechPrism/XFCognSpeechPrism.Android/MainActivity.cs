using Android.App;
using Android.Content.PM;
using Android.OS;
using Prism;
using Prism.Ioc;
using XFCognSpeechPrism.Services;
using Xamarin.Forms;
using XFCognSpeechPrism.Droid.Services;
using Android.Runtime;

namespace XFCognSpeechPrism.Droid
{
    [Activity(Label = "XFCognSpeechPrism", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        IMicrophoneService micService;
        internal static MainActivity Instance { get; private set; }
        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.Essentials.Platform.Init(this, bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.Forms.FormsMaterial.Init(this, bundle);   // Material
            LoadApplication(new App(new AndroidInitializer()));    // ???

            micService = DependencyService.Resolve<IMicrophoneService>();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case AndroidMicrophoneService.RecordAudioPermissionCode:
                    if (grantResults[0] == Permission.Granted)
                    {
                        micService.OnRequestPermissionResult(true);
                    }
                    else
                    {
                        micService.OnRequestPermissionResult(false);
                    }
                    break;
            }
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }
}

