SETUP
======

Software Needed
- Windows 8.1
- Visual Studio
- Open CV 2.4.9
+ make sure you set the path for this to work
%OPENCV_DIR%/bin
+ add bin x86/vc12 (latest one) to path 
setx -m OPENCV_DIR C:\opencv\build\x86\vc12
- .NET framework, needs to be 3.5
https://msdn.microsoft.com/en-us/library/hh506443(v=vs.110).aspx
- Kinect SDK C#

1) Navigate into kinect-streamer/src/ folder. Double click Main (a Visual Studio project file)
2) Once Visual Studio opens up with all the files, 
- make sure FaceBasics is bolded (select it and Project > Set as startup ...)
- set filepath in FaceRecognition.cpp and EnrollmentManager.cs to correct filepath (this is where feed, name/face database, pictures will get saved)
3) open Network folder, make sure to connect to main eyehome computer

======= How it works

FaceBasics-WPF: main (pulls data from the Kinect)
 - pulls frames from Kinect -> to openCV
FaceEnrollment: 
 - openCV -> adapted for Kinect
 - does the training
 FaceRecognition:
 - FaceRecognition.h: wrapper for bridging Kinect (C#) to OpenCV (C++)
 
 - Train (in FaceRecognition.cpp) array
 images: d.jpg, d.jpg, d.jpg, d.jpg
 labels: dylan, dylan, dylan, dylan
 crop: <x,y coordinates for where to crop>
 --> calls the OpenCV train
 FaceRecognitionResult: label (integer, needs to be mapped to database), confidence

- context.txt (write 'company' or 'alone')
- contacts.txt (people in the room) - (id, time they came in, distance)
- /<id>/
	+ info.txt (contains name, and later other details)
	+ 10 x <date><time>.jpg buffer
    	- 10 hertz (take face pics 10th of a second)

Flow at start of program

MainWindow -> EnrollmentManager -> WelcomePage -> EnterNamePage -> TrainingPage -> MainWindow -> FaceRecognition

FaceRecognition
- opencv port
- calls opencv methods

FaceEnrollment
- contains the gui with the pages
- page 4 controls some training

FaceBasics-WPF
- Mainwindow calls methods inside FaceRecognition
- boxes (kinect face basics stuff)

======

Known bugs/TODO
- FIX RESOLUTION ON SOME OF THE PAGES!
- not sure what the kinect does when someone has the same name already
- cannot update a user that is already in the 
- there is no progressive learning when it comes to training
- lots of unnecessary debug messages
- does not know if someone is a new person (will try to map to most similar person)
- does not tell you when training someone (add "Training Data" page so it doesn't look like its crashing)
- cannot remove a person
- crashes when not connected to the internet (so there isnt an exception) because cannot find folder to save images to
- does it actually update on both kinects? 

======= This is notes from Brian

HOW TO ADD AN APP TO EYEHOME
1. make physical folder in Documents > eyehome > “Test”
2. add a file “test” into folder for the scripts (can name anything, as long as its a ahk)
3. Open eyehome on visual studio and edit the menu.json to include the new application (use other examples in there as template)
4. Add an image to images/menu folder, same name as the ID from menu.json (needs to be png, ex. test.png)
5. Include new image to project with right click + “Include …”
6. if need be, in common.js, add one line of code:

localStorage.clear(); 

to “reload” the menu.json


