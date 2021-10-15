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

        public static readonly IReadOnlyCollection<string> Tasks = new[]
        {
            "lunch on the Moon",
            "drive around Mars"
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
                    await TextToSpeech.SynthesizeAudioAsync(config, $"\nYou have {Tasks.Count} pending tasks.");

                    foreach (var task in Tasks)
                    {
                        await TextToSpeech.SynthesizeAudioAsync(config, $"\n<Approve> or <reject> task <{task}>?");

                        var response = await SpeechToText.FromMic(config);
                        var emailResponseSuccessful = await SendGrid.Send(task, response);

                        if (emailResponseSuccessful)
                        {
                            await TextToSpeech.SynthesizeAudioAsync(config, $"\n{response} <{task}> sent.");
                        }
                        else
                        {
                            await TextToSpeech.SynthesizeAudioAsync(config, "Oops, something went wrong.  Please try again later.");
                        }
                    }
                }
                else
                {
                    await TextToSpeech.SynthesizeAudioAsync(config, "Unknown speaker.");
                }
            } while (!isVerified);
            
            //await PrintProfiles(config);
            //await DeleteProfiles(config);

            await TextToSpeech.SynthesizeAudioAsync(config, "\nAll tasks completed.  Goodbye.");
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
