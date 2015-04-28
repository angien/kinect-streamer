using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceEnrollment
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        private string filepath = @"C:\Test\";
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // CREATE ALL THE FILES HERE!
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
                Directory.CreateDirectory(filepath + "feed");
                File.Create(filepath + "contacts.txt");
                File.Create(filepath + "context.txt");
                File.Create(filepath + "nameDB.txt");
                File.Create(filepath + "faceDB.txt");
                EnrollmentManager.window.Content = new EnterNamePage();
            }
            else
            {
                if (File.ReadAllLines(@"C:\Test\faceDB.txt").Length == 0)
                {
                    EnrollmentManager.window.Content = new EnterNamePage();
                }
                else
                    EnrollmentManager.Finish(true);
            }

            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            EnrollmentManager.isKinect2();
            // CREATE ALL THE FILES HERE!
            if (!Directory.Exists(filepath))
            {
               
                EnrollmentManager.window.Content = new ErrorPage();
            }
            else
            {
                if (File.ReadAllLines(@"C:\Test\faceDB.txt").Length == 0)
                {
                    EnrollmentManager.window.Content = new EnterNamePage();
                }
                else
                    EnrollmentManager.Finish(true);
            }
        }
       
    }
}
