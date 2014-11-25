/*
 * Copyright (c) 2013-present, The Eye Tribe. 
 * All rights reserved.
 *
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree. 
 *
 */
using System;
using System.Net.Mime;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using TETControls.Calibration;
using TETControls.TrackBox;
using TETCSharpClient.Data;
using System.Windows.Interop;
using TETCSharpClient;


namespace HeyYou.EyeTracking
{
    public partial class EyeTrackingWindow : IConnectionStateListener, IGazeListener
    {
        private double gazeX;
        private double gazeY;
        private double prevGazeX = 0, prevGazeY = 0;
        private double prevTime = 0;
        private double timeFixed = 0;

        private bool isDoubleBlink = false, isLongBlink = false, eyesClosed = false;
        private long prevTimeOfBlink = 0, timeOpened = 0, timeClosed = 0;
        private int blinkCount = 0;

        private int blinkCounter = 0;
        private ulong blinkTrackingId = 0;

        private bool usingFilter = true; //change this value to false if you wish to use raw eyetribe data

        public EyeTrackingWindow()
        {
            ConnectClient();
            InitializeComponent();
            Loaded += (sender, args) => InitClient();
        }

        private void ConnectClient()
        {
            // Create a client for the eye tracker
            GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);
            GazeManager.Instance.AddGazeListener(this);
        }

        public void OnGazeUpdate(GazeData gazeData)
        {

            //grab the gaze position and either filter it or use raw data
            if (usingFilter)
            {
                filterGaze(gazeData.SmoothedCoordinates.X, gazeData.SmoothedCoordinates.Y, gazeData.TimeStamp, ref gazeX, ref gazeY);

            }
            else
            {
                gazeX = gazeData.SmoothedCoordinates.X;
                gazeY = gazeData.SmoothedCoordinates.Y;
            }

            checkForDoubleAndLongBlink(gazeData);



            //check for double blink
            //if (gazeX == 0 && gazeY == 0)
            //{
            //    blinkCounter++;
            //}


        }

        private void filterGaze(double currGazeX, double currGazeY, double currTime, ref double gazeX, ref double gazeY)
        {

            double distance = 0;
            double dTime = 0;

            //if prev data exists, calculate the change in distance and time
            if (prevGazeX != 0 && prevGazeY != 0 && prevTime != 0)
            {
                if (currGazeX != 0 && currGazeY != 0)
                {
                    distance = Math.Sqrt((currGazeX - prevGazeX) * (currGazeX - prevGazeX) + (currGazeY - prevGazeY) * (currGazeY - prevGazeY));
                    dTime = currTime - prevTime;
                }
                else
                {
                    //distance and time remain 0 and the teleport to 0,0 is filtered out
                    //Console.Write("Unable to detect eyes!\n");
                }
            }
            //if the first callback, initialize the previous gaze data
            else
            {
                prevGazeX = currGazeX;
                prevGazeY = currGazeY;
                prevTime = currTime;
            }

            //if the saccade is far enough, move the cursor
            if (distance > 50 && timeFixed > 30000)
            {
                gazeX = currGazeX;
                gazeY = currGazeY;

                prevGazeX = currGazeX;
                prevGazeY = currGazeY;

                timeFixed = 0;
            }
            //otherwise, keep the point in its original postion
            else
            {
                gazeX = prevGazeX;
                gazeY = prevGazeY;

                //increase the time spent at the original position
                timeFixed += dTime;
            }

            //currTime = gazeData.TimeStamp;

            //System.Diagnostics.Debug.WriteLine("Distance: {0}, Time: {1}", distance, dTime);
            //Console.Write("Distance: {0}, Time: {1} TimeFixed: {2}\n", distance, dTime, timeFixed);

        }

        private void checkForDoubleAndLongBlink(GazeData gazeData)
        {
            long currTimeOfBlink = 0;
            long betweenBlinksTime = 0;
            long blinkDuration = 0;
            //Console.Write("State {0}\n", gazeData.State);

            //if the eyes just closed
            if (gazeData.State == GazeData.STATE_TRACKING_FAIL && !eyesClosed)
            {

                eyesClosed = true;
                timeClosed = gazeData.TimeStamp;
            }
            //when the eyes reopen
            else if (gazeData.State == 7 && eyesClosed)
            {
                eyesClosed = false;
                timeOpened = gazeData.TimeStamp;
                //the time the blink occured is defined on opening
                currTimeOfBlink = timeOpened;
                blinkCount++;
                blinkDuration = timeOpened - timeClosed;
                Console.Write("Blink! {0} Duration: {1}\n", blinkCount, blinkDuration);

                //check for a long blink gesture
                if (blinkDuration > 600 && blinkDuration < 1000)
                {
                    isLongBlink = true;
                }
                else isLongBlink = false;

                //check for the double blink
                if (prevTimeOfBlink != 0)
                {
                    betweenBlinksTime = currTimeOfBlink - prevTimeOfBlink;
                    Console.Write("Time between blinks: {0}\n", betweenBlinksTime);
                    //the double blink is only triggered on the second blink and when meets the thresholds
                    if (betweenBlinksTime > 100 && betweenBlinksTime < 500)
                    {
                        isDoubleBlink = true;
                    }

                    else
                    {
                        isDoubleBlink = false;
                    }

                    prevTimeOfBlink = currTimeOfBlink;
                }
                else
                {

                    isDoubleBlink = false;
                    prevTimeOfBlink = currTimeOfBlink;
                }
            }

            /*
            if (gazeData.State == GazeData.STATE_TRACKING_FAIL) {
                currTimeOfBlink = gazeData.TimeStamp;
                Console.Write("Blink!\n");
                //if on the second blink
                if (prevTimeOfBlink != 0)
                {
                    dTime = currTimeOfBlink - prevTimeOfBlink;
                    //Console.Write("Time between blinks: {0}\n", dTime);
                    //the double blink is only triggered on the second blink and when meets the thresholds
                    if (dTime > 2000 && dTime < 5000)
                    {
                        isDoubleBlink = true;
                    }

                    else {
                        isDoubleBlink = false;
                    }

                    prevTimeOfBlink = 0;
                }
                else {

                    isDoubleBlink = false;
                    prevTimeOfBlink = currTimeOfBlink;
                }
            }*/
        }

        private void InitClient()
        {
            // Default content of the action button
            btnAction.Content = "Calibrate";

            // Add a fresh instance of the trackbox in case we reinitialize the client connection.
            TrackingStatusGrid.Children.Clear();
            TrackingStatusGrid.Children.Add(new TrackBoxStatus());

            // Add listener if EyeTribe Server is closed
            GazeManager.Instance.AddConnectionStateListener(this);

            // What is the current connection state
            OnConnectionStateChanged(GazeManager.Instance.IsActivated);

            if (GazeManager.Instance.IsCalibrated)
            {
                // Get the latest successful calibration from the EyeTribe server
                RatingText.Text = RatingFunction(GazeManager.Instance.LastCalibrationResult);
                btnAction.Content = "Re-Calibrate";
            }
        }

        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (GazeManager.Instance.IsActivated)
            {
                Calibrate();
            }
            else
            {
                ConnectClient();
                InitClient();
            }
        }

        private void Calibrate()
        {
            btnAction.Content = "Re-Calibrate";

            //Run the calibration on 'this' monitor
            var ActiveScreen = Screen.FromHandle(new WindowInteropHelper(this).Handle);

            // Initialize and start the calibration
            CalibrationRunner calRunner = new CalibrationRunner(ActiveScreen, ActiveScreen.Bounds.Size, 9);
            calRunner.OnResult += calRunner_OnResult;
            calRunner.Start();
        }

        private void calRunner_OnResult(object sender, CalibrationRunnerEventArgs e)
        {
            if (RatingText.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new MethodInvoker(() => calRunner_OnResult(sender, e)));
                return;
            }

            if (e.Result == CalibrationRunnerResult.Success)
            {
                RatingText.Text = RatingFunction(e.CalibrationResult);
            }
            else
            {
                System.Windows.MessageBox.Show("Calibration failed, please try again");
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            GazeManager.Instance.Deactivate();
        }

        public string RatingFunction(CalibrationResult result)
        {
            if (result == null)
                return "";

            var accuracy = result.AverageErrorDegree;

            if (accuracy < 0.5)
                return "Calibration Quality: PERFECT";

            if (accuracy < 0.7)
                return "Calibration Quality: GOOD";

            if (accuracy < 1)
                return "Calibration Quality: MODERATE";

            if (accuracy < 1.5)
                return "Calibration Quality: POOR";

            return "Calibration Quality: REDO";
        }

        public void OnConnectionStateChanged(bool IsActivated)
        {
            // The connection state listener detects when the connection to the EyeTribe server changes
            if (btnAction.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new MethodInvoker(() => OnConnectionStateChanged(IsActivated)));
                return;
            }
            if (!IsActivated)
            {
                GazeManager.Instance.Deactivate();
                RatingText.Text = "";
                btnAction.Content = "Re-Connect";
            }
        }

        public double GetX()
        {
            return gazeX;
        }

        public double GetY()
        {
            return gazeY;
        }

        public bool getDoubleBlink()
        {
            return isDoubleBlink;
        }
        public void resetDoubleBlink()
        {
            isDoubleBlink = false;
        }

        public bool getLongBlink()
        {
            return isLongBlink;
        }
        public void resetLongBlink()
        {
            isLongBlink = false;
        }

        public int GetBlinkCount()
        {
            return blinkCounter;
        }

        public void ResetBlinkCount()
        {
            blinkCounter = 0;
            blinkTrackingId = 0;
        }

        public ulong GetBlinkTrackingId()
        {
            return blinkTrackingId;
        }

        public void SetBlinkTrackingId(ulong id)
        {
            blinkTrackingId = id;
        }

        // check for double blinks
        /*
        /// <summary>
        /// Keeps count of blinks for selecting
        /// </summary>
        private int blinkCounter = 0;

        /// <summary>
        /// Keeps track of who is being looked at to select
        /// </summary>
        private ulong blinkTrackingId = 0;
        
        if (eyeTracker.GetX() == 0 && eyeTracker.GetY() == 0)
        {
            if (blinkTrackingId == 0)
            {
                blinkTrackingId = faceFrameSources[i].TrackingId;
                blinkCounter++;
            }
            else
            {
                if (blinkTrackingId == faceFrameSources[i].TrackingId)
                {
                    blinkCounter++;
                }
                else
                {
                    blinkTrackingId = faceFrameSources[i].TrackingId;
                    blinkCounter = 1;
                }
            }
        }
        if (blinkCounter == 2)
        {
            Console.WriteLine("Say Hey You to person with tracking id: " + faceFrameSources[i].TrackingId);
            blinkTrackingId = 0;
            blinkCounter = 0;
        }
         * */
    }
}
