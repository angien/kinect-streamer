// FaceRecognition.h

#pragma once

#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/contrib/contrib.hpp>
#include <iostream>

using namespace cv;
using namespace std;
using namespace System;
using namespace System::Drawing;

namespace FaceRecognition {
	public ref class FaceRecognitionResult
	{
	public:
		int label;
		double confidence;
	};

	public ref class FaceRecognizerBridge
	{
	public:
		FaceRecognizerBridge() {
			faceRecognizer = new Ptr<FaceRecognizer>();
			*faceRecognizer = createLBPHFaceRecognizer();
		}

		FaceRecognitionResult^ Predict(Bitmap^ image, System::Windows::Rect faceCrop);
		void Train(array<Bitmap^>^ images, array<int>^ labels);

	private:
		Ptr<FaceRecognizer>* faceRecognizer;
	};
}
