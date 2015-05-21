// This is the main DLL file.

#include "stdafx.h"
#include <direct.h>
#include "windows.h"
#include "FaceRecognition.h"
#include <string>
#include <fstream>      // std::ifstream

using namespace FaceRecognition;

//string filepath = "\\\\NARENDRAN-PC\\Users\\Narendran\\Documents\\eyehome\\Test\\";
string filepath = "C:\\Test\\";

Mat bitmapToMat(Bitmap^ image) {
	ImageConverter converter;
	array<Byte>^ temp = gcnew array<Byte>(1);
	array<Byte>^ data = (array<Byte>^) converter.ConvertTo(image, temp->GetType());

	int size = data->Length;
	int rows = image->Height;

	pin_ptr<unsigned char> pointer = &data[0];
	unsigned char* rawData = (unsigned char*)pointer;

	vector<unsigned char> buffer(rawData, rawData + size);
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

	double confidence = 0.0;
	int label = -1;
	// This is where the prediction happens

	// added this to see what would happen and it's just returning -1 this time
	//(*faceRecognizer)->set("threshold", 100.0);

	//while (confidence < 100) { // keep predicting until we get a confidence that is over 100?

	cv::Size s = nativeImage.size();
	int rows = s.height;
	int cols = s.width;

	cv::Size ts = croppedImageResized.size();
	int srows = ts.height;
	int scols = ts.width;
	if (scols == 0 || srows == 0 || rows == 0 || cols == 0) {

		cerr << "IMAGES WERE NULL" << endl;
	}

	try {

		(*faceRecognizer)->predict(croppedImageResized, label, confidence);
	}
	catch (...) {
		cerr << "hello" << endl;
		label = -1;
	}
	//	cerr << "ID:  " << label << " Confidence: " << confidence << "\n";
	//}
		FaceRecognitionResult^ result = gcnew FaceRecognitionResult();
		result->confidence = confidence;
		result->label = label;
		cerr << "Final ID:  " << label << " Confidence: " << confidence << "\n";

	return result;
}

void FaceRecognizerBridge::Update(array<Bitmap^>^ images, array<int>^ labels, array<System::Windows::Rect>^ faceCrops) {
	/*vector<Mat> nativeImages;
	vector<int> nativeLabels;

	for (int i = 0; i < images->Length; i++)
	{
		System::Windows::Rect faceCrop = faceCrops[i];

		Mat image = bitmapToMat(images[i]);
		Mat croppedImageResized = cropAndResize(image, faceCrop);

		string directory = filepath + to_string(labels[i]) ;
		string filename = to_string(i) + ".jpg";
		bool check = imwrite(directory +"\\"+ filename, croppedImageResized);
		if (check == false) {
			mkdir(directory.c_str());
			bool check2 = imwrite(directory+ "\\"+filename, croppedImageResized);
		}
		nativeImages.push_back(croppedImageResized);
		int id = labels[i];
		nativeLabels.push_back(id);

	}

	(*faceRecognizer)->update(nativeImages, nativeLabels);
	Save();*/

	vector<Mat> nativeImages;
	vector<int> nativeLabels;

	for (int i = 0; i < images->Length; i++)
	{
		System::Windows::Rect faceCrop = faceCrops[i];

		Mat image = bitmapToMat(images[i]);
		Mat croppedImageResized = cropAndResize(image, faceCrop);

		// save the image into the feed folder
		string directory = filepath + to_string(labels[i]);
		string filename = to_string(i) + ".jpg";
		bool check = imwrite(directory + "\\" + filename, croppedImageResized);
		if (check == false) {

			mkdir(directory.c_str());
			bool check2 = imwrite(directory + "\\" + filename, croppedImageResized);

		}
		nativeImages.push_back(croppedImageResized);
		int id = labels[i];
		cerr << "training id:" << id << " native size: " << nativeImages.size() << "\n";
		nativeLabels.push_back(id);

	}

	//(*faceRecognizer)->train(nativeImages, nativeLabels);
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

		// save the image into the feed folder
		string directory = filepath + to_string(labels[i]);
		string filename = to_string(i)+".jpg";
		bool check = imwrite(directory +"\\"+ filename, croppedImageResized);
		if (check == false) {
			
			mkdir(directory.c_str());
			bool check2 = imwrite(directory + "\\" + filename, croppedImageResized);

		}
		nativeImages.push_back(croppedImageResized);
		int id = labels[i];
		cerr << "training id:" << id << " native size: " << nativeImages.size() << "\n";
		nativeLabels.push_back(id);

	}

	(*faceRecognizer)->train(nativeImages, nativeLabels);
	//(*faceRecognizer)->update(nativeImages, nativeLabels);
	Save();
}

void FaceRecognizerBridge::Preview(Bitmap^ image, System::Windows::Rect faceCrop) {
	Mat nativeImage = bitmapToMat(image);
	imwrite(filepath+ "preview.jpg", cropAndResize(nativeImage, faceCrop));
}

void FaceRecognizerBridge::Save() {
	(*faceRecognizer)->save(filepath + "faceDB.txt");
}

void FaceRecognizerBridge::Load() {
		(*faceRecognizer)->load(filepath+ "faceDB.txt");
}