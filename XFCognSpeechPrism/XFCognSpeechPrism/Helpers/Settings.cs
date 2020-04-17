using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace XFCognSpeechPrism.Helpers
{
    public class Settings
    {

        public static string SpeechLanguage
        {
            get => Preferences.Get("SpeechLanguage", "de-DE");
            set => Preferences.Set("SpeechLanguage", value);
        }

        public static string Akey
        {
            get => Preferences.Get("Akey", "YourApiStringHere");
            set => Preferences.Set("Akey", value);
        }

        public static string Aregion
        {
            get => Preferences.Get("Aregion", "YourRegionHere");
            set => Preferences.Set("Aregion", value);
        }

    }
}
