#include "Raytracer.h"

/* 
	Major thanks to Scratchapixel for the tutorial:
	https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection
*/

/* Trace a ray */
bool Raytracer::trace(const Vec3f & orig, const Vec3f & dir, const std::vector<std::unique_ptr<Object>>& objects, float & tNear, Object *& hitObject)
{
	tNear = kInfinity;
	std::vector<std::unique_ptr<Object>>::const_iterator iter = objects.begin();
	for (; iter != objects.end(); ++iter) {
		float t = kInfinity;
		if ((*iter)->intersect(orig, dir, t) && t < tNear) {
			hitObject = iter->get();
			tNear = t;
		}
	}

	return (hitObject != nullptr);
}

/* Cast a ray to the scene */
Vec3f Raytracer::castRaySolid(const Vec3f & orig, const Vec3f & dir, const std::vector<std::unique_ptr<Object>>& objects, bool & hit)
{
	hit = false;
	Vec3f hitColor = 0;
	Object *hitObject = nullptr; // this is a pointer to the hit object
	float t; // this is the intersection distance from the ray origin to the hit point
	if (trace(orig, dir, objects, t, hitObject)) {
		if (!dynamic_cast<VolumetricObject*>(hitObject)) {
			Vec3f Phit = orig + dir * t;
			Vec3f Nhit;
			Vec2f tex;
			hitObject->getSurfaceData(Phit, Nhit, tex);
			float scale = 4;
			float pattern = (fmodf(tex.x * scale, 1) > 0.5) ^ (fmodf(tex.y * scale, 1) > 0.5);
			hitColor = std::max(0.f, Nhit.dotProduct(-dir)) * mix(hitObject->colour, hitObject->colour * 0.8, pattern);

			hit = true;
		}
	}

	return hitColor;
}
Vec3f Raytracer::castRayAdditive(const Vec3f& orig, const Vec3f& dir, const std::vector<std::unique_ptr<Object>>& objects, bool& hit)
{
	hit = false;
	Vec3f hitColor = 0;
	Object* hitObject = nullptr; // this is a pointer to the hit object
	float t; // this is the intersection distance from the ray origin to the hit point
	if (trace(orig, dir, objects, t, hitObject)) {
		if (dynamic_cast<VolumetricObject*>(hitObject)) {
			//Volumetric shape
			VolumetricObject* obj = static_cast<VolumetricObject*>(hitObject);
			float tempColour = obj->density(orig, dir, t) / 40; //magic number to make test set work with hosek-wilkie brightness
			hitColor = Vec3f(tempColour, tempColour, tempColour); //temp debug output
			hit = (hitColor.x != 0.0f); //temp
		}
	}

	return hitColor;
}

/* Render the scene */
void Raytracer::render(const std::vector<std::unique_ptr<Object>>& objects, bool asHDR)
{
	const int num_channels = 9;
	// Three wavelengths around red, three around green, and three around blue.
	double lambda[num_channels] = { 630, 680, 710, 500, 530, 560, 460, 480, 490 };

	ArHosekSkyModelState *skymodel_state[num_channels];
	for (int i = 0; i < num_channels; ++i) {
		skymodel_state[i] =
			arhosekskymodelstate_alloc_init(elevation, turbidity, albedo);
	}

	// Vector pointing at the sun. Note that elevation is measured from the
	// horizon--not the zenith, as it is elsewhere in pbrt.
	Vec3f sunDir(0., std::sin(elevation), std::cos(elevation));

	int nTheta = height*2, nPhi = 2 * nTheta; /*SETTING HEIGHT TO BE *2 SO WE DONT GET BLACK VALUES BELOW ZENITH IN FINAL*/
	std::vector<float> img(3 * nTheta * nPhi, 0.f);

	for (int t = 0; t < nTheta; t++) {
		float theta = float(t + 0.5) / nTheta * M_PI;
		if (theta > M_PI / 2.) continue;
		for (int p = 0; p < nPhi; ++p) {
			float phi = float(p + 0.5) / nPhi * 2. * M_PI;

			// Vector corresponding to the direction for this pixel.
			Vec3f v(std::cos(phi) * std::sin(theta), std::cos(theta),
				std::sin(phi) * std::sin(theta));
			// Compute the angle between the pixel's direction and the sun
			// direction.
			float gamma = std::acos(Clamp(Dot(v, sunDir), -1, 1));

			for (int c = 0; c < num_channels; ++c) {
				float val = arhosekskymodel_solar_radiance(
					skymodel_state[c], theta, gamma, lambda[c]);
				img[3 * (t * nPhi + p) + c / 3] += val / 3.f;
			}
		}
	}

	/*
	std::ifstream ofs0("image.bin", std::ios::in | std::ios::binary);
	std::vector<float> img_test = std::vector<float>();
	for (int x = 0; x < width; x++) {
		for (int y = 0; y < height; y++) {
			for (int i = 0; i < 3; i++) {
				float thisColour;
				ofs0.read(reinterpret_cast<char*>(&thisColour), sizeof(float));
				img_test.push_back(thisColour);
			}
		}
	}
	ofs0.close();
	*/

	Vec3f *framebuffer = new Vec3f[width * height];
	Vec3f *pix = framebuffer;
	float scale = tan(deg2rad(fov * 0.5));
	float imageAspectRatio = width / (float)height;
	Vec3f orig;
	cameraToWorld.multVecMatrix(Vec3f(0), orig);
	for (uint32_t j = 0; j < height; ++j) {
		for (uint32_t i = 0; i < width; ++i) {
			float x = (2 * (i + 0.5) / (float)width - 1) * imageAspectRatio * scale;
			float y = (1 - 2 * (j + 0.5) / (float)height) * scale;

			Vec3f dir;
			cameraToWorld.multDirMatrix(Vec3f(x, y, -1), dir);
			dir.normalize();

			bool hitSolid = false;
			bool hitAdditive = false;
			Vec3f col = castRaySolid(orig, dir, objects, hitSolid);
			col = col + castRayAdditive(orig, dir, objects, hitAdditive);
			if (!hitSolid) {
				//If we didn't hit a solid object, take the background sky colour - adds to our additive, or blank if hit none
				col = col + Vec3f(
					img[(3 * ((j * nPhi) + i)) + 0],
					img[(3 * ((j * nPhi) + i)) + 1],
					img[(3 * ((j * nPhi) + i)) + 2]);
				/*
				col = Vec3f(
					img_test[(3 * ((i * height) + j)) + 0],
					img_test[(3 * ((i * height) + j)) + 1],
					img_test[(3 * ((i * height) + j)) + 2]);
					*/
			}
			*(pix++) = col;
		}
	}

	//Output as LDR
	if (!asHDR) 
	{
		std::ofstream ofs("out.ppm", std::ios::out | std::ios::binary);
		ofs << "P6\n" << width << " " << height << "\n255\n";
		for (uint32_t i = 0; i < height * width; ++i) {
			char r = (char)(255 * clamp(0, 1, framebuffer[i].x));
			char g = (char)(255 * clamp(0, 1, framebuffer[i].y));
			char b = (char)(255 * clamp(0, 1, framebuffer[i].z));
			ofs << r << g << b;
		}
		ofs.close();
	}
	//Output as HDR
	else 
	{
		std::ofstream ofs("out.pfm", std::ios::out | std::ios::binary);
		ofs << "PF\n" << width << " " << height << "\n-1.0\n";
		for (uint32_t i = 0; i < height * width; ++i) {
			ofs.write(reinterpret_cast<const char*>(&framebuffer[i].x), sizeof(float));
			ofs.write(reinterpret_cast<const char*>(&framebuffer[i].y), sizeof(float));
			ofs.write(reinterpret_cast<const char*>(&framebuffer[i].z), sizeof(float));
		}
		ofs.close();
	}

	delete[] framebuffer;
}
