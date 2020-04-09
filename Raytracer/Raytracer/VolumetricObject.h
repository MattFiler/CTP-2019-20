#pragma once
#include "VDBLoader.h"
#include "Object.h"

class VolumetricObject : public Object
{
public:
	VolumetricObject(const std::string filename, const Vec3f _colour) : Object(_colour) {
		thisVDB = new VDBLoader(filename);
		bounds[0] = thisVDB->GetBounds().min();
		bounds[1] = thisVDB->GetBounds().max();
		center = (bounds[1] - bounds[0]) + bounds[0];
	}
	~VolumetricObject() {
		delete thisVDB;
	}

	bool intersect(const Vec3f& orig, const Vec3f& dir, float& t) const;
	void getSurfaceData(const Vec3f& Phit, Vec3f& Nhit, Vec2f& tex) const;

private:
	VDBLoader* thisVDB = nullptr;
	Vec3f bounds[2];
	Vec3f center;
};