#include "SphereObject.h"

/* Check to see if a ray intersects with us */
bool Sphere::intersect(const Vec3f & orig, const Vec3f & dir, float & t)
{
	float t0, t1;
	Vec3f L = orig - center;
	float a = dir.dotProduct(dir);
	float b = 2 * dir.dotProduct(L);
	float c = L.dotProduct(L) - radius2;
	if (!solveQuadratic(a, b, c, t0, t1)) return false;
	if (t0 > t1) std::swap(t0, t1);

	if (t0 < 0) {
		t0 = t1; // if t0 is negative, let's use t1 instead
		if (t0 < 0) return false; // both t0 and t1 are negative
	}

	t = t0;

	return true;
}

/* Get the sphere's surface data */
void Sphere::getSurfaceData(const Vec3f & Phit, Vec3f & Nhit, Vec2f & tex)
{
	Nhit = Phit - center;
	Nhit.normalize();
	//make a checkerboard effect
	tex.x = (1 + atan2(Nhit.z, Nhit.x) / M_PI) * 0.5; 
	tex.y = acosf(Nhit.y) / M_PI;
}
