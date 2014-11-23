using System.Speech.Synthesis;

namespace TextToSpeech
{
    public class SpeechOutput
    {
        SpeechSynthesizer synthesizer;

        public SpeechOutput()
        {
            synthesizer = new SpeechSynthesizer();

            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10
        }

        public void OutputToAudio( string output )
        {
            synthesizer.Speak( output );
        }
    }
}
