#pragma once

#include "Object.h"

class Sphere : public Object
{
public:
	Sphere(const Vec3f &c, const float &r, const Vec3f _colour) : Object(_colour) {
		radius = r;
		radius2 = r * r;
		center = c;
	}

	bool intersect(const Vec3f &orig, const Vec3f &dir, float &t);
	void getSurfaceData(const Vec3f &Phit, Vec3f &Nhit, Vec2f &tex);

private:
	float radius, radius2;
	Vec3f center;
};