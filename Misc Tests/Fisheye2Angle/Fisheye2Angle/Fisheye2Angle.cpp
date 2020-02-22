// Fisheye2Angle.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include <iostream>

#define M_PI       3.14159265358979323846   // pi

int fisheye2angle(int fe_width, int fe_height, int fe_w, int fe_h, float *angles, float fisheyeradius) {
	float w = ((float)fe_w - ((float)fe_width) * 0.5f) / fisheyeradius;
	float h = ((float)fe_h - ((float)fe_height) * 0.5f) / fisheyeradius;
	if (sqrtf(w*w + h * h) > 1.f) {
		return -1;
	}
	float theta = asinf(sqrtf(w*w + h * h));
	float phi = atan2f(h, w);
	if (phi < 0)
		phi += 2 * M_PI;

	angles[0] = theta;
	angles[1] = phi;
	return 0;
}

int main()
{
	float angle[2] = { 0.0f, 0.0f };
	std::cout << fisheye2angle(1024, 1024, 10, 10, angle, 1024) << std::endl;
    std::cout << angle[0] << std::endl;
	std::cout << angle[1] << std::endl;
	int d;
	std::cin >> d;
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
