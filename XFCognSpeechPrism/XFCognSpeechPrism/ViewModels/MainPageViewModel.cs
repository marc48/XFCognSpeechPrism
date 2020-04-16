using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Diagnostics;
using XFCognSpeechPrism.Helpers;
using XFCognSpeechPrism.Models;
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

        //AudioRecorderService recorder;
        //SpeechApiClient speechClient;

        //public Array SpeechRegions => Enum.GetValues(typeof(SpeechRegion));
        //public SpeechRegion SpeechRegion { get; set; } = SpeechRegion.WestEurope;
        //public Array AuthenticationModes => Enum.GetValues(typeof(AuthenticationMode));
        //public AuthenticationMode AuthenticationMode { get; set; } = AuthenticationMode.SubscriptionKey;
        //public Array RecognitionModes => Enum.GetValues(typeof(RecognitionMode));
        //public RecognitionMode RecognitionMode { get; set; } = RecognitionMode.Interactive;
        //public Array ProfanityModes => Enum.GetValues(typeof(ProfanityMode));
        //public ProfanityMode ProfanityMode { get; set; } = ProfanityMode.Masked;
        //public Array OutputModes => Enum.GetValues(typeof(OutputMode));
        //public OutputMode OutputMode { get; set; } = OutputMode.Simple;

        private ObservableCollection<Sentence> _sents;
        public ObservableCollection<Sentence> Sents
        {
            get { return _sents; }
            set { SetProperty(ref _sents, value); }
        }
        private Sentence _sentence;
        public Sentence Sentence
        {
            get { return _sentence; }
            set { SetProperty(ref _sentence, value); }
        }

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
            //micService = 
            SettingsPageCommand = new DelegateCommand(NavToSettings);
            RecordCommand = new DelegateCommand(TranscribeSent);
            Title = "Record Page";

            _recBtnText = "Record Sent";
            _buttonEnabled = true;
            _sents = new ObservableCollection<Sentence>();

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

            // Liste belegen
            _sents.Add(new Sentence() { Text = "Der erste Satz." });

            micService = Xamarin.Forms.DependencyService.Resolve<IMicrophoneService>();
            
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
                recognizer = new SpeechRecognizer(config, "de-DE");  // 8 overloads!

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
                    InsertDateTimeRecord();
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
            //UpdateTranscription("Bis hierher und nicht weiter...");
        }

        void UpdateTranscription(string newText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    //ResultsText += $"{newText}\n";
                    _sents.Add(new Sentence() { Text = newText });
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
                    BtnColor = Color.Red;
                    SpinnerEnabled = true;
                }
                else
                {
                    RecBtnText = "Transcribe";
                    BtnColor = Color.Green;
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
