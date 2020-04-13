#pragma once
#include "VDBLoader.h"
#include "Object.h"
#include "globals.h"


//todo: add the ability to transform this object's position (shift all queries by a given transform)
class VolumetricObject : public Object
{
public:
	VolumetricObject(const std::string filename, const Vec3f _colour) : Object(_colour) {
		thisVDB = new VDBLoader(filename);

		bounds[0] = thisVDB->GetBounds().min();
		bounds[1] = thisVDB->GetBounds().max();
		center = (bounds[1] - bounds[0]) + bounds[0];

		maxDensity = thisVDB->GetMaxDensity();
		invMaxDensity = 1 / maxDensity;
	}
	~VolumetricObject() {
		delete thisVDB;
	}

	bool intersect(const Vec3f& orig, const Vec3f& dir, float& t);
	void getSurfaceData(const Vec3f& Phit, Vec3f& Nhit, Vec2f& tex) {} //not in use atm
	float density(const Vec3f& orig, const Vec3f& dir, float t);

private:
	float densityToSun(const Vec3f& orig) {
		float t = 0.0f;
		float totalD = 0.0f;
		float thisD = 0.0f;
		while (true) {
			thisD = thisVDB->density(orig + (Globals::sunDirection * t));
			if (thisD == 0.0f) break;
			totalD += thisD;
			t++;
		}
		return totalD;
	}

	VDBLoader* thisVDB = nullptr;
	Vec3f bounds[2];
	Vec3f center;

	float maxDensity;
	float invMaxDensity;

	float tmin = 0.0f;
	float tmax = 0.0f;
	float tymin = 0.0f;
	float tymax = 0.0f;
	float tzmin = 0.0f;
	float tzmax = 0.0f;
};