﻿using System;
using System.Collections.Generic;
using System.Text;

namespace XFCognSpeechPrism.Helpers
{
    public static class Constants
    {
        // API key can be a shared, multi-resource key or an individual service key
        // and can be found and regenerated in the Azure portal 
        public static string CognitiveServicesApiKey = "PutYourApiStringHere";

        // Endpoint is based on your configured region, for example "westus"
        public static string CognitiveServicesRegion = "YourApiRegion";
    }
}
