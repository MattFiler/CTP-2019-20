#pragma once
#define _USE_MATH_DEFINES
#include <cstdio>
#include <cstdlib>
#include <memory>
#include <vector>
#include <utility>
#include <cstdint>
#include <cmath>
#include <iostream>
#include <fstream>
#include <limits>
#include <random>

#include "geometry.h"
#include "ArHosekSkyModel.h"
#include "spectrum.h"

/*
	Maths functions, nabbed from PBRT and Scratchapixel - thanks!
*/

inline
float clamp(const float &lo, const float &hi, const float &v)
{
	return std::max(lo, std::min(hi, v));
}

inline
float deg2rad(const float &deg)
{
	return deg * M_PI / 180;
}

inline
Vec3f mix(const Vec3f &a, const Vec3f& b, const float &mixValue)
{
	return a * (1 - mixValue) + b * mixValue;
}

inline
float Radians(float deg) { return (M_PI / 180) * deg; }

template <typename T>
T Dot(const Vec3<T> &v1, const Vec3<T> &v2) {
	return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
}

template <typename T, typename U, typename V>
inline T Clamp(T val, U low, V high) {
	if (val < low)
		return low;
	else if (val > high)
		return high;
	else
		return val;
}

inline
bool solveQuadratic(const float &a, const float &b, const float &c, float &x0, float &x1)
{
	float discr = b * b - 4 * a * c;
	if (discr < 0) return false;
	else if (discr == 0) {
		x0 = x1 = -0.5 * b / a;
	}
	else {
		float q = (b > 0) ?
			-0.5 * (b + sqrt(discr)) :
			-0.5 * (b - sqrt(discr));
		x0 = q / a;
		x1 = c / q;
	}

	return true;
}