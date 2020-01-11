#include <iostream>
#include <fstream>
#include "rgbe.h"

#pragma warning (disable : 4996)

int main(int argc, char** argv) 
{
	float* image;
	int imageWidth, imageHeight;

	//Parse HDR
	FILE* hdrFile = fopen("streetview.hdr", "rb");
	RGBE_ReadHeader(hdrFile, &imageWidth, &imageHeight, NULL);
	image = (float *)malloc(sizeof(float) * 3 * imageWidth*imageHeight);
	RGBE_ReadPixels_RLE(hdrFile, image, imageWidth, imageHeight);
	fclose(hdrFile);

	//Re-write
	std::ofstream hdrDump;
	hdrDump.open("streetview.bin", std::ios::out | std::ios::binary);
	for (int i = 0; i < imageWidth * imageHeight; i++) {
		hdrDump.write(reinterpret_cast<const char*>(&image[i]), sizeof(float));
	}
	hdrDump.close();

	return 0;
}