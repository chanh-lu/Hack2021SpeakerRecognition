using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;

namespace SpeakerRecognition
{
    public static class TextToSpeech
    {
        public static async Task SynthesizeAudioAsync(SpeechConfig config, string text)
        {
            Console.WriteLine(text);
            using var synthesizer = new SpeechSynthesizer(config);
            await synthesizer.SpeakTextAsync(text);
        }
    }
}
