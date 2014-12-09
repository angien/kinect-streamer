using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Speech;
using System.Speech.AudioFormat;

namespace TextToSpeech
{
    public class SpeechOutput
    {
        SpeechSynthesizer synthesizer;
        private int toggleCount = 0;
        private int numberOfVoices = 3;

        public SpeechOutput(string voiceType)
        {
    /*
            foreach(var v in synthesizer.GetInstalledVoices().Select(v => v.VoiceInfo))
            {
                Console.WriteLine("Name:{0}, Gender:{1}, Age:{2}", v.Description, v.Gender, v.Age);
            }

            Console.WriteLine("SelectVoice Example");
            SpeechSynthesizer ttsSynth = new SpeechSynthesizer();

            Console.WriteLine("Listing installed speech synthesizer voices...");
            foreach (InstalledVoice ttsVoice in ttsSynth.GetInstalledVoices())
            {
                Console.WriteLine("Name:\t{0}", ttsVoice.VoiceInfo.Name);
                Console.WriteLine("Desc:\t{0}", ttsVoice.VoiceInfo.Description);
                Console.WriteLine("Id:\t{0}", ttsVoice.VoiceInfo.Id);
                Console.WriteLine("Gender:\t{0}", ttsVoice.VoiceInfo.Gender);
                Console.WriteLine("Age:\t{0}", ttsVoice.VoiceInfo.Age);

                Console.WriteLine("Supported Audio Formats:");
                foreach (SpeechAudioFormatInfo audioFormat in ttsVoice.VoiceInfo.SupportedAudioFormats)
                {
                    Console.WriteLine("\tEncodingFormat:\t{0}", audioFormat.EncodingFormat);
                    Console.WriteLine("\tChannelCount:\t{0}", audioFormat.ChannelCount);
                    Console.WriteLine("\tBits/sec:\t{0}", audioFormat.BitsPerSample);
                    Console.WriteLine("\tAvg Bytes/sec:\t{0}", audioFormat.AverageBytesPerSecond);
                    Console.WriteLine("\tSamples/sec:\t{0}", audioFormat.SamplesPerSecond);
                    Console.WriteLine("\tBlockAlign:\t{0}", audioFormat.BlockAlign);
                }

                Console.WriteLine("Additional Information:");
                foreach (KeyValuePair<string, string> kvp in ttsVoice.VoiceInfo.AdditionalInfo)
                    Console.WriteLine("\t{0}:  {1}", kvp.Key, kvp.Value);
                Console.WriteLine();
            }

    */

           
            synthesizer = new SpeechSynthesizer();
            synthesizer.SelectVoice(voiceType);

            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10
        }

        public void OutputToAudio( string output )
        {
            synthesizer.SpeakAsync( output );
        }

        public string nextVoice() {
            int choice = toggleCount % numberOfVoices;
            string voiceStyle = "Magic Voice";
            if (choice == 0) {
                synthesizer.SelectVoice("Microsoft Zira Desktop");
                voiceStyle = "Female: English US";

            }
            else if (choice == 1) {
                synthesizer.SelectVoice("Microsoft Hazel Desktop");
                voiceStyle = "Female: English GB";


            }
            else if (choice == 2) {
                synthesizer.SelectVoice("Microsoft David Desktop");
                voiceStyle = "Male: English US";


            }
            toggleCount++;

            return voiceStyle;
        }
    }
}
