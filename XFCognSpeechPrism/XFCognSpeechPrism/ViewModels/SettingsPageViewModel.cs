using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using XFCognSpeechPrism.Helpers;

namespace XFCognSpeechPrism.ViewModels
{
    public class SettingsPageViewModel : BindableBase
    {
        private string _inputLang;
        public string InputLang
        {
            get { return _inputLang; }
            set { SetProperty(ref _inputLang, value); }
        }

        private string _akey;
        public string Akey
        {
            get { return _akey; }
            set { SetProperty(ref _akey, value); }
        }

        private string _aregion;
        public string Aregion
        {
            get { return _aregion; }
            set { SetProperty(ref _aregion, value); }
        }

        public DelegateCommand SaveSettingsCommand { get; set; }

        public SettingsPageViewModel()
        {
            _inputLang = Settings.SpeechLanguage;
            _akey = Settings.Akey;
            _aregion = Settings.Aregion;
            SaveSettingsCommand = new DelegateCommand(SaveSettings);
        }

        private void SaveSettings()
        {
            // Save the setting strings
            Settings.SpeechLanguage = InputLang;
            Settings.Akey = Akey;
            Settings.Aregion = Aregion;
        }
    }
}
