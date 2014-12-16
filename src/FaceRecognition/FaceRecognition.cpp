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

Mat cropAndResize(Mat image, System::Windows::Rect crop) {
	Mat croppedImage = image(Rect(crop.Left, crop.Top, crop.Width, crop.Height));
	Mat croppedImageResized;
	resize(croppedImage, croppedImageResized, cv::Size(200, 200), 0, 0, INTER_LINEAR);
	return croppedImageResized;
}

FaceRecognitionResult^ FaceRecognizerBridge::Predict(Bitmap^ image, System::Windows::Rect faceCrop) {
	Mat nativeImage = bitmapToMat(image);
	Mat croppedImageResized = cropAndResize(nativeImage, faceCrop);

	double confidence;
	int label;
	(*faceRecognizer)->predict(croppedImageResized, label, confidence); 
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
		Mat croppedImageResized = cropAndResize(image, faceCrop);

		nativeImages.push_back(croppedImageResized);
		int id = labels[i];
		nativeLabels.push_back(id);
	}

	(*faceRecognizer)->train(nativeImages, nativeLabels);
}

void FaceRecognizerBridge::Preview(Bitmap^ image, System::Windows::Rect faceCrop) {
	Mat nativeImage = bitmapToMat(image);
	imwrite("preview.jpg", cropAndResize(nativeImage, faceCrop));
}

void FaceRecognizerBridge::Save() {
	(*faceRecognizer)->save("faceDB.txt");
}

void FaceRecognizerBridge::Load() {
	(*faceRecognizer)->load("faceDB.txt");
}