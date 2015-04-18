﻿using System;
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
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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