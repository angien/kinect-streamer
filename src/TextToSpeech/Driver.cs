namespace TextToSpeech
{
    class Driver
    {
        static void Main( string[] args )
        {
            SpeechOutput speaker = new SpeechOutput("Microsoft David Desktop");
            speaker.OutputToAudio( "I'm hungry for brains" );
        }
    }
}
