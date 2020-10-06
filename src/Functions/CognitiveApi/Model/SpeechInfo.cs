using System;
using System.Collections.Generic;
using System.Text;

namespace CognitiveApi.Model
{
    public class SpeechInfo
    {
        public DateTime CreationDate { get; set; }

        public string TextToConvert { get; set; }

        public DateTime ProcessedDate { get; set; }

        public string AudioFileUri { get; set; }
    }
}
