using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Speaker;

namespace SpeakerRecognition
{
    public static class SpeakerRecognition
    {
        // Add the ProfileId after enrollment to link to guid with human readable name.
        public static readonly IDictionary<string, string> KnownIdentities = new Dictionary<string, string>
        {
            {"c51e862b-78bd-410a-a815-c6e5431a291a", "Test"}
        };

        public static async Task<bool> VerifySpeakerIdentity(SpeechConfig config)
        {
            const double similarityScoreMinThreshold = 0.3;

            using var client = new VoiceProfileClient(config);

            var speakerRecognizer = new SpeakerRecognizer(config, AudioConfig.FromDefaultMicrophoneInput());
            var profiles = await client.GetAllProfilesAsync(VoiceProfileType.TextIndependentIdentification);
            var model = SpeakerIdentificationModel.FromProfiles(profiles);

            var result = await speakerRecognizer.RecognizeOnceAsync(model);
            var score = result.Score;

            KnownIdentities.TryGetValue(result.ProfileId, out var profileId);

            Console.WriteLine($"The most similar voice profile is <{profileId}> with similarity score <{score}>");

            return score > similarityScoreMinThreshold && !string.IsNullOrWhiteSpace(profileId);
        }

        public static async Task EnrollSpeakerAndIdentify(SpeechConfig config)
        {
            // persist profileMapping if you want to store a record of who the profile is
            var profileMapping = new Dictionary<string, string>();
            var profileNames = new List<string>() { "Test" };
    
            var enrolledProfiles = await IdentificationEnroll(config, profileNames, profileMapping);
            await SpeakerIdentification(config, enrolledProfiles, profileMapping);

            foreach (var profile in enrolledProfiles)
            {
                profile.Dispose();
            }
        }

        private static async Task<List<VoiceProfile>> IdentificationEnroll(SpeechConfig config, List<string> profileNames, Dictionary<string, string> profileMapping)
        {
            List<VoiceProfile> voiceProfiles = new List<VoiceProfile>();
            using (var client = new VoiceProfileClient(config))
            {
                foreach (string name in profileNames)
                {
                    using (var audioInput = AudioConfig.FromDefaultMicrophoneInput())
                    {
                        var profile = await client.CreateProfileAsync(VoiceProfileType.TextIndependentIdentification, "en-us");
                        Console.WriteLine($"Creating voice profile for {name}.");
                        profileMapping.Add(profile.Id, name);

                        VoiceProfileEnrollmentResult result = null;
                        while (result is null || result.RemainingEnrollmentsSpeechLength > TimeSpan.Zero)
                        {
                            Console.WriteLine($"Continue speaking to add to the profile enrollment sample for {name}.");
                            result = await client.EnrollProfileAsync(profile, audioInput);
                            Console.WriteLine($"Remaining enrollment audio time needed: {result.RemainingEnrollmentsSpeechLength}");
                            Console.WriteLine("");
                        }
                        voiceProfiles.Add(profile);
                    }
                }
            }
            return voiceProfiles;
        }

        private static async Task SpeakerIdentification(SpeechConfig config, List<VoiceProfile> voiceProfiles, Dictionary<string, string> profileMapping) 
        {
            var speakerRecognizer = new SpeakerRecognizer(config, AudioConfig.FromDefaultMicrophoneInput());
            var model = SpeakerIdentificationModel.FromProfiles(voiceProfiles);

            var result = await speakerRecognizer.RecognizeOnceAsync(model);
            Console.WriteLine($"The most similar voice profile is {profileMapping[result.ProfileId]} with similarity score {result.Score}");
        }
    }
}
