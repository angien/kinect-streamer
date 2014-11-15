using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using ReadWriteCsv;

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
