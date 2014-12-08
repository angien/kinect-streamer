// This is the main DLL file.

#include "stdafx.h"

#include "FaceRecognition.h"

using namespace FaceRecognition;

Mat bitmapToMat(Bitmap^ image) {
	ImageConverter converter;
	array<Byte>^ temp = gcnew array<Byte>(1);
	array<Byte>^ data = (array<Byte>^) converter.ConvertTo(image, temp->GetType());

	int size = data->Length;
	int rows = image->Height;

	pin_ptr<unsigned char> pointer = &data[0];
	unsigned char* rawData = (unsigned char*)pointer;

	vector<char> buffer(rawData, rawData + size);
	return imdecode(buffer, CV_LOAD_IMAGE_GRAYSCALE);
}

FaceRecognitionResult^ FaceRecognizerBridge::Predict(Bitmap^ image, System::Windows::Rect faceCrop) {
	Mat nativeImage = bitmapToMat(image);
	Mat croppedImage = nativeImage(Rect(faceCrop.Left, faceCrop.Top, faceCrop.Width, faceCrop.Height));

	double confidence;
	int label;
	(*faceRecognizer)->predict(croppedImage, label, confidence); 
	FaceRecognitionResult^ result = gcnew FaceRecognitionResult();
	result->confidence = confidence;
	result->label = label;
	return result;
}

void FaceRecognizerBridge::Train(array<Bitmap^>^ images, array<int>^ labels, array<System::Windows::Rect>^ faceCrops) {
	vector<Mat> nativeImages;
	vector<int> nativeLabels;

	for (int i = 0; i < images->Length; i++)
	{
		System::Windows::Rect faceCrop = faceCrops[i];
		Mat image = bitmapToMat(images[i]);
		Mat croppedImage = image(Rect(faceCrop.Left, faceCrop.Top, faceCrop.Width, faceCrop.Height));
		nativeImages.push_back(croppedImage);
		int id = labels[i];
		nativeLabels.push_back(id);
	}

	(*faceRecognizer)->train(nativeImages, nativeLabels);
}