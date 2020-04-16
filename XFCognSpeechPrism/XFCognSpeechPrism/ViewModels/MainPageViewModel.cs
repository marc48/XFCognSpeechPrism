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
using System.Diagnostics;
using XFCognSpeechPrism.Helpers;
using Microsoft.CognitiveServices.Speech;
using XFCognSpeechPrism.Services;

namespace XFCognSpeechPrism.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private IPageDialogService _dialogService;
        private SpeechRecognizer recognizer = null;
        IMicrophoneService micService;
        private bool isTranscribing = false;
        private bool firsttime;
        private string language;

        private bool _streamSwitch;
        public bool StreamSwitch
        {
            get { return _streamSwitch; }
            set
            {
                SetProperty(ref _streamSwitch, value);
                SwitchStreamToggle();
            }
        }
        private void SwitchStreamToggle()
        {
            if (_streamSwitch == true)
            {
                //Settings.Streamswitch == true ???
            }
        }

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

        public DelegateCommand SettingsPageCommand { get; set; }
        public DelegateCommand RecordCommand { get; set; }


        public MainPageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService)
        {
            _dialogService = dialogService;
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
            language = "de-DE";
        }

        private async void TranscribeSent()
        {
            //_dialogService.DisplayAlertAsync("Record", "(TODO)", "OK");
            //await RecordAudio();

            bool isMicEnabled = await micService.GetPermissionAsync();

            // EARLY OUT: make sure mic is accessible
            if (!isMicEnabled)
            {
                UpdateTranscription("Please grant access to the microphone!");
                return;
            }

            // initialize speech recognizer 
            if (recognizer == null)
            {
                var config = SpeechConfig.FromSubscription(Constants.CognitiveServicesApiKey, Constants.CognitiveServicesRegion);
                recognizer = new SpeechRecognizer(config, language);  // 8 overloads!

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
                    if (firsttime == true)
                    {
                        InsertDateTimeRecord();
                        firsttime = false;
                    }
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
            _dialogService.DisplayAlertAsync("Settings", "Goto Settings (TODO)", "OK");
        }



    }
}
