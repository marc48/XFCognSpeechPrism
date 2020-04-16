using AVFoundation;
using XFCognSpeechPrism.iOS.Services;
using XFCognSpeechPrism.Services;
using System.Threading.Tasks;
using Xamarin.Forms;

using Foundation;   // ??
using UIKit;        // ??

[assembly: Dependency(typeof(iOSMicrophoneService))]
namespace XFCognSpeechPrism.iOS.Services
{
    public class iOSMicrophoneService : IMicrophoneService
    {
        TaskCompletionSource<bool> tcsPermissions;

        public Task<bool> GetPermissionAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();
            RequestMicPermission();
            return tcsPermissions.Task;
        }

        public void OnRequestPermissionResult(bool isGranted)
        {
            tcsPermissions.TrySetResult(isGranted);
        }

        void RequestMicPermission()
        {
            var session = AVAudioSession.SharedInstance();
            session.RequestRecordPermission((granted) =>
            {
                tcsPermissions.TrySetResult(granted);
            });
        }
    }
}