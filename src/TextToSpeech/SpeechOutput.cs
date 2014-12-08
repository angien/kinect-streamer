using System.Speech.Synthesis;

namespace TextToSpeech
{
    public class SpeechOutput
    {
        SpeechSynthesizer synthesizer;
        private int toggleCount = 0;
        private int numberOfVoices = 3;

        public SpeechOutput(string voiceType)
        {
           
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
