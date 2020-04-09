#include "VolumetricObject.h"
#include "HenyeyGreenstein.h"

/* Check to see if a ray intersects with our bounding box */
bool VolumetricObject::intersect(const Vec3f& orig, const Vec3f& dir, float& t)
{
	Vec3f invdir = 1 / dir;
	int sign[3];
	sign[0] = (invdir.x < 0);
	sign[1] = (invdir.y < 0);
	sign[2] = (invdir.z < 0);

	tmin = (bounds[sign[0]].x - orig.x) * invdir.x;
	tmax = (bounds[1 - sign[0]].x - orig.x) * invdir.x;
	tymin = (bounds[sign[1]].y - orig.y) * invdir.y;
	tymax = (bounds[1 - sign[1]].y - orig.y) * invdir.y;

	if ((tmin > tymax) || (tymin > tmax))
		return false;

	if (tymin > tmin)
		tmin = tymin;
	if (tymax < tmax)
		tmax = tymax;

	tzmin = (bounds[sign[2]].z - orig.z) * invdir.z;
	tzmax = (bounds[1 - sign[2]].z - orig.z) * invdir.z;

	if ((tmin > tzmax) || (tzmin > tmax))
		return false;

	if (tzmin > tmin)
		tmin = tzmin;
	if (tzmax < tmax)
		tmax = tzmax;

	t = tmin;

	if (t < 0) {
		t = tmax;
		if (t < 0) return false;
	}

	return true;
}

/* Get the density along the given ray */
float VolumetricObject::density(const Vec3f& orig, const Vec3f& dir, float &t)
{
	float total_density = 0;
	HenyeyGreenstein greenstein = HenyeyGreenstein();
	float this_t = t;
	while (true) {
		Vec3f density_pos = orig + (dir * this_t);
		float density = thisVDB->density(density_pos);
		total_density += density;
		this_t++;
		if (this_t > tmax) {
			break;
		}
	}
	if (total_density != 0) {
		std::string fdgdfgf = "";
	}
	return total_density;
}
