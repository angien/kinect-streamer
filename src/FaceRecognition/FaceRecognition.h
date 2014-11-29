// FaceRecognition.h

#pragma once

#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <iostream>

using namespace cv;
using namespace std;
using namespace System;
using namespace System::Drawing;

namespace FaceRecognition {

	public ref class FaceRecognizerBridge
	{
	public:
		void Predict(Bitmap^ image);
	};
}
