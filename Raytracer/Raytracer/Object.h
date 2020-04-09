#pragma once

#include "math.h"

class Object
{
public:
	Object() = delete;
	Object(Vec3f _colour) {
		colour = _colour;
	}
	virtual ~Object() {}

	virtual bool intersect(const Vec3f &, const Vec3f &, float &) = 0;
	virtual void getSurfaceData(const Vec3f &, Vec3f &, Vec2f &) = 0;

	Vec3f colour;
};