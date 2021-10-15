using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Speaker;

namespace SpeakerRecognition
{
    public class Program
    {
        public static readonly IReadOnlyCollection<VoiceProfileType> VoiceProfileTypes = new[]
        {
            VoiceProfileType.TextDependentVerification,
            VoiceProfileType.TextIndependentVerification,
            VoiceProfileType.TextIndependentIdentification
        };

        public static async Task Main(string[] args)
        {
            var config = SpeechConfig.FromSubscription(Settings.SubscriptionKey, Settings.Region);

            bool isVerified;

            do
            {
                await TextToSpeech.SynthesizeAudioAsync(config, "\nSpeak some text to identify enrolled speakers.");

                isVerified = await SpeakerRecognition.VerifySpeakerIdentity(config);

                if (isVerified)
                {
                    await TextToSpeech.SynthesizeAudioAsync(config, "Speaker verified.");
                    await TextToSpeech.SynthesizeAudioAsync(config, "\nWould you like to <approve> or <reject> the process <Time for lunch>");
                    var response = await SpeechToText.FromMic(config);

                    if (response.ToLower().Contains("approve"))
                    {
                        await TextToSpeech.SynthesizeAudioAsync(config, "Process approved.");
                    }
                    else
                    {
                        await TextToSpeech.SynthesizeAudioAsync(config, "Process rejected.");
                    }

                    Console.WriteLine($"RECOGNISED: Text={response}");
                }
                else
                {
                    await TextToSpeech.SynthesizeAudioAsync(config, "Unknown speaker.");
                }
            } while (!isVerified);
            
            //await PrintProfiles(config);
            //await DeleteProfiles(config);

            Console.WriteLine("\nTest ended. Press any key to close.");
            Console.ReadLine();
        }

        public static async Task PrintProfiles(SpeechConfig config)
        {

            using var client = new VoiceProfileClient(config);

            foreach (var voiceProfileType in VoiceProfileTypes)
            {
                var profiles = await client.GetAllProfilesAsync(voiceProfileType);

                Console.WriteLine($"\n{voiceProfileType} profile Ids");

                foreach (var profile in profiles)
                {
                    Console.WriteLine(profile.Id);
                    profile.Dispose();
                }
            }
        }

        public static async Task DeleteProfiles(SpeechConfig config)
        {
            using var client = new VoiceProfileClient(config);
            
            foreach (var voiceProfileType in VoiceProfileTypes)
            {
                var profiles = await client.GetAllProfilesAsync(voiceProfileType);

                Console.WriteLine($"\n{voiceProfileType} profile Ids deleted");

                foreach (var profile in profiles)
                {
                    await client.DeleteProfileAsync(profile);
                    Console.WriteLine(profile.Id);
                }
            }
        }
    }
}
