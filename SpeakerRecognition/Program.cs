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

            await TextToSpeech.SynthesizeAudioAsync(config, "Speak some text to identify who it is from your list of enrolled speakers.");

            var isVerified = await SpeakerRecognition.VerifySpeakerIdentity(config);

            if (isVerified)
            {
                await TextToSpeech.SynthesizeAudioAsync(config, "\nSpeaker verified.");
                await TextToSpeech.SynthesizeAudioAsync(config, "\nWould you like to <approve> or <reject> the process <Time for lunch>");
                var response = await SpeechToText.FromMic(config);

                if (response.ToLower().Contains("approve"))
                {
                    Console.WriteLine("Process approved.");
                }
                else
                {
                    Console.WriteLine("Process rejected.");
                }

                Console.WriteLine($"RECOGNISED: Text={response}");
            }
            else
            {
                await TextToSpeech.SynthesizeAudioAsync(config, "Unknown speaker.");
            }
            
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
