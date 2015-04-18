using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.IO;

namespace FaceEnrollment
{
    /// <summary>
    /// Interaction logic for Page3.xaml
    /// </summary>
    public partial class EnterNamePage : Page
    {
        public EnterNamePage()
        {
            InitializeComponent();
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            PersonTrainingData person = new PersonTrainingData();
            person.name = personName.Text;
            string[] readText = File.ReadAllLines("C:\\Test\\nameDB.txt");
            int pos = Array.IndexOf(readText, person.name);
            if (pos > -1)
            {
                EnrollmentManager.doUpdate = true;
                // the array contains the string and the pos variable
                // will have its position in the array

                person.trainingId = pos;
            }
            else
            {

                EnrollmentManager.doUpdate = false;
                person.trainingId = readText.Length;
            }
            EnrollmentManager.personToTrain = person;
            EnrollmentManager.window.Content = new TrainingPage();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
          
            EnrollmentManager.Finish(true);
        
        }
    }
}
