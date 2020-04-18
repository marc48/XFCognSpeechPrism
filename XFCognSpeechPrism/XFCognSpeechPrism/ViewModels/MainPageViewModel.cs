using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XFCognSpeechPrism.Helpers;
using Microsoft.CognitiveServices.Speech;
using XFCognSpeechPrism.Services;

namespace XFCognSpeechPrism.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private IPageDialogService _dialogService;
        private INavigationService _navigationService;
        private SpeechRecognizer recognizer = null;
        IMicrophoneService micService;
        private bool isTranscribing = false;
        private bool firsttime;

        private string _recBtnText;
        public string RecBtnText
        {
            get { return _recBtnText; }
            set { SetProperty(ref _recBtnText, value); }
        }

        private bool _spinnerEnabled;
        public bool SpinnerEnabled
        {
            get { return _spinnerEnabled; }
            set { SetProperty(ref _spinnerEnabled, value); }
        }

        private bool _buttonEnabled;
        public bool ButtonEnabled
        {
            get { return _buttonEnabled; }
            set { SetProperty(ref _buttonEnabled, value); }
        }

        private Color _btnColor;
        public Color BtnColor
        {
            get { return _btnColor; }
            set { SetProperty(ref _btnColor, value); }
        }

        private string _resultsText;
        public string ResultsText
        {
            get { return _resultsText; }
            set { SetProperty(ref _resultsText, value); }
        }

        private string _language;
        public string Language
        {
            get { return _language; }
            set { SetProperty(ref _language, value); }
        }

        public DelegateCommand SettingsPageCommand { get; set; }
        public DelegateCommand RecordCommand { get; set; }


        public MainPageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            SettingsPageCommand = new DelegateCommand(NavToSettings);
            RecordCommand = new DelegateCommand(TranscribeSent);
            Title = "Record Page";

            _recBtnText = "Transcribe";
            _buttonEnabled = true;
            _btnColor = Color.FromHex("#43a047");

            //recorder = new AudioRecorderService
            //{
            //    StopRecordingOnSilence = true,
            //    StopRecordingAfterTimeout = true,
            //    TotalAudioTimeout = TimeSpan.FromSeconds(15) // Speech REST API has 15 sec max
            //};

            //if (Keys.Speech.SubscriptionKey == Keys.Speech.BadSubscriptionKey)  >>> NEU constanrts
            //{
            //    throw new Exception("Get a Speech API key here: https://azure.microsoft.com/en-us/pricing/details/cognitive-services/speech-api/");
            //}

            //speechClient = new SpeechApiClient(Keys.Speech.SubscriptionKey, SpeechRegion);

            micService = Xamarin.Forms.DependencyService.Resolve<IMicrophoneService>();
            firsttime = true;
            _language = Settings.SpeechLanguage;
        }

        private async void TranscribeSent()
        {
            bool isMicEnabled = await micService.GetPermissionAsync();

            // EARLY OUT: make sure mic is accessible
            if (!isMicEnabled)
            {
                UpdateTranscription("Please grant access to the microphone!");
                return;
            }

            // API key can be a shared, multi-resource key or an individual service key
            // and can be found and regenerated in the Azure portal 
            // Endpoint is based on your configured region, for example "WestEurope"
            // SpeechConfig.FromSubscription(CognitiveServicesApiKey, CognitiveServicesRegion)
            if (Settings.Akey == "YourApiStringHere" || Settings.Aregion == "YourRegionHere")
            {
                await _dialogService.DisplayAlertAsync("Missing API key", "Generate your key in the Azure portal!", "OK");
                return;
            }

            // initialize speech recognizer 
            if (recognizer == null)
            {
 
                var config = SpeechConfig.FromSubscription(Settings.Akey, Settings.Aregion);
                recognizer = new SpeechRecognizer(config, Language);  // 8 overloads!

                recognizer.Recognized += (obj, args) =>
                {
                    UpdateTranscription(args.Result.Text);
                };
            }

            // if already transcribing, stop speech recognizer
            if (isTranscribing)
            {
                try
                {
                    await recognizer.StopContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = false;
            }

            // if not transcribing, start speech recognizer
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //if (firsttime == true)
                    //{
                        InsertDateTimeRecord();
                    //    firsttime = false;
                    //}
                });
                try
                {
                    await recognizer.StartContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = true;
            }
            
            UpdateDisplayState();
        }

        void UpdateTranscription(string newText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    ResultsText += $"{newText}\n";
                }
            });
        }

        void InsertDateTimeRecord()
        {
            var msg = $"=== {DateTime.Now.ToString()}";
            UpdateTranscription(msg);
        }

        void UpdateDisplayState()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (isTranscribing)
                {
                    RecBtnText = "Stop";
                    BtnColor = Color.FromHex("#e53935");
                    SpinnerEnabled = true;
                }
                else
                {
                    RecBtnText = "Transcribe";
                    BtnColor = Color.FromHex("#43a047");
                    SpinnerEnabled = false;
                }
            });
        }

        private void NavToSettings()
        {
            //_dialogService.DisplayAlertAsync("Settings", "Goto Settings (TODO)", "OK");
            _navigationService.NavigateAsync("SettingsPage");
        }



    }
}
