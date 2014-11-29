// This is the main DLL file.

#include "stdafx.h"

#include "FaceRecognition.h"

void FaceRecognition::FaceRecognizerBridge::Predict(Bitmap^ image) {
	ImageConverter converter;
	array<Byte>^ temp = gcnew array<Byte>(1);
	array<Byte>^ data = (array<Byte>^) converter.ConvertTo(image, temp->GetType());

	int size = data->Length;
	int rows = image->Height;

	pin_ptr<unsigned char> pointer = &data[0];
	unsigned char* rawData = (unsigned char*)pointer;

	vector<char> buffer(rawData, rawData + size);
	Mat result = imdecode(buffer, CV_LOAD_IMAGE_GRAYSCALE);
	imwrite("out.bmp", result);
}