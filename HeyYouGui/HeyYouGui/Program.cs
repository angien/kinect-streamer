using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using ReadWriteCsv;

using System.Speech.Synthesis;
using System.Speech;
using System.Speech.AudioFormat;

namespace HeyYouGui
{
    static class Program
    {
        static public List<List<String>> profiles = new List<List<String>>();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
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

            CsvFileReader read = new CsvFileReader("dbcsv.csv");
            int count = 0;
            while (true)
            {
                CsvRow row = new CsvRow();
                if(!read.ReadRow(row))
                {
                    break;
                }

                profiles.Add(new List<String>());
                foreach (String value in row)
                {
                    profiles[count].Add(value);
                }
                count++;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
