using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Speech.Synthesis;

namespace HeyYouGui
{
    public partial class Form1 : Form
    {

        SpeechSynthesizer synthesizer;

        public Form1()
        {
            InitializeComponent();

            synthesizer = new SpeechSynthesizer();

            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10
        }

        private void button1_Click(object sender, EventArgs e)
        {
            synthesizer.Speak("Hey you.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            synthesizer.Speak("Hello");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            synthesizer.Speak("Help me");
        }
    }
}
