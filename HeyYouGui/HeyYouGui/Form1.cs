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
        int currentprofile = 1;
        SpeechSynthesizer synthesizer;

        public Form1()
        {
            InitializeComponent();

            label1.Text = Program.profiles[currentprofile][0];
            button1.Text = Program.profiles[currentprofile][1];
            button2.Text = Program.profiles[currentprofile][2];
            button3.Text = Program.profiles[currentprofile][3];


            synthesizer = new SpeechSynthesizer();

            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10
        }

        private void button1_Click(object sender, EventArgs e)
        {
            synthesizer.Speak(button1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            synthesizer.Speak(button2.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            synthesizer.Speak(button3.Text);
        }
    }
}
