namespace TextToSpeech
{
    class Driver
    {
        static void Main( string[] args )
        {
            SpeechOutput speaker = new SpeechOutput();
            speaker.OutputToAudio( "I'm hungry for brains" );
        }
    }
}
