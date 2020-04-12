#pragma once
#include "math.h"
#include "Object.h"
#include "SphereObject.h"
#include "BoxObject.h"
#include "VolumetricObject.h"

class Raytracer {
public:
	Raytracer() = delete;
	Raytracer(float _width, float _height, float _fov, Matrix44f& _cameraToWorld, float groundAlbedo = 0.5f, float skyTurbidity = 3.0f, float sunElevation = Radians(10)) {
		width = _width;
		height = _height;
		fov = _fov;
		cameraToWorld = _cameraToWorld;
		albedo = groundAlbedo;
		turbidity = skyTurbidity;
		elevation = sunElevation;
	}
	~Raytracer() = default;

	void render(const std::vector<std::unique_ptr<Object>> &objects, bool asHDR = true);

private:
	bool trace(const Vec3f &orig, const Vec3f &dir, const std::vector<std::unique_ptr<Object>> &objects, float &tNear, Object *&hitObject);
	Vec3f castRay(const Vec3f &orig, const Vec3f &dir, const std::vector<std::unique_ptr<Object>> &objects, bool &hit);

	const float kInfinity = std::numeric_limits<float>::max();
	std::random_device rd;

	float width;
	float height;
	float fov;
	Matrix44f cameraToWorld;

	float albedo;
	float turbidity;
	float elevation;
};