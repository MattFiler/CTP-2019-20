#pragma once
#include "math.h"
#include "Object.h"
#include "SphereObject.h"
#include "BoxObject.h"
#include "VolumetricObject.h"

struct Options
{
	uint32_t width;
	uint32_t height;
	float fov;
	Matrix44f cameraToWorld;
};

class Raytracer {
public:
	Raytracer() = delete;
	Raytracer(const Options &_options) {
		options = _options;
	}
	~Raytracer() = default;

	void render(const std::vector<std::unique_ptr<Object>> &objects);

private:
	bool trace(const Vec3f &orig, const Vec3f &dir, const std::vector<std::unique_ptr<Object>> &objects, float &tNear, const Object *&hitObject);
	Vec3f castRay(const Vec3f &orig, const Vec3f &dir, const std::vector<std::unique_ptr<Object>> &objects, bool &hit);

	const float kInfinity = std::numeric_limits<float>::max();
	std::random_device rd;

	Options options;

	float albedo = 0.5;
	float turbidity = 3.;
	float elevation = Radians(10);
	int resolution = 2048;
};