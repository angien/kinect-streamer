// This is the main DLL file.

#include "stdafx.h"

#include "windows.h"
#include "FaceRecognition.h"
#include <string>
#include <fstream>      // std::ifstream

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
	// This is where the prediction happens


		(*faceRecognizer)->predict(croppedImageResized, label, confidence);
		FaceRecognitionResult^ result = gcnew FaceRecognitionResult();
		result->confidence = confidence;
		result->label = label;
	return result;
}

void FaceRecognizerBridge::Update(array<Bitmap^>^ images, array<int>^ labels, array<System::Windows::Rect>^ faceCrops) {
	vector<Mat> nativeImages;
	vector<int> nativeLabels;

	for (int i = 0; i < images->Length; i++)
	{
		System::Windows::Rect faceCrop = faceCrops[i];

		Mat image = bitmapToMat(images[i]);
		Mat croppedImageResized = cropAndResize(image, faceCrop);

		string directory = "C:\\Test\\" + to_string(labels[i]) + "\\";
		string filename = to_string(i) + ".jpg";
		bool check = imwrite(directory + filename, croppedImageResized);
		if (check == false) {
			string folderCreateCommand = "mkdir " + directory;
			system(folderCreateCommand.c_str());
			bool check2 = imwrite(directory + filename, croppedImageResized);

		}
		nativeImages.push_back(croppedImageResized);
		int id = labels[i];
		nativeLabels.push_back(id);

	}

	(*faceRecognizer)->update(nativeImages, nativeLabels);
	Save();
}

void FaceRecognizerBridge::Train(array<Bitmap^>^ images, array<int>^ labels, array<System::Windows::Rect>^ faceCrops) {
	vector<Mat> nativeImages;
	vector<int> nativeLabels;

	for (int i = 0; i < images->Length; i++)
	{
		System::Windows::Rect faceCrop = faceCrops[i];

		Mat image = bitmapToMat(images[i]);
		Mat croppedImageResized = cropAndResize(image, faceCrop);

		string directory = "C:\\Test\\" + to_string(labels[i]) + "\\";
		string filename = to_string(i)+".jpg";
		bool check = imwrite(directory + filename, croppedImageResized);
		if (check == false) {
			string folderCreateCommand = "mkdir " + directory;
			system(folderCreateCommand.c_str());
			bool check2 =imwrite(directory + filename, croppedImageResized);

		}
		nativeImages.push_back(croppedImageResized);
		int id = labels[i];
		nativeLabels.push_back(id);

	}

	(*faceRecognizer)->train(nativeImages, nativeLabels);
	(*faceRecognizer)->update(nativeImages, nativeLabels);
	Save();
}

void FaceRecognizerBridge::Preview(Bitmap^ image, System::Windows::Rect faceCrop) {
	Mat nativeImage = bitmapToMat(image);
	imwrite("C:/Test/preview.jpg", cropAndResize(nativeImage, faceCrop));
}

void FaceRecognizerBridge::Save() {
	(*faceRecognizer)->save("C:/Test/faceDB.txt");
}

void FaceRecognizerBridge::Load() {
		(*faceRecognizer)->load("C:/Test/faceDB.txt");
}