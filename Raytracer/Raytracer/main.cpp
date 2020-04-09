#pragma warning(disable:4996)
#include "Raytracer.h"
#include "OrthoNormalBasis.h"
#include "VDBLoader.h"
#include "HenyeyGreenstein.h"

/* Initialise the raytracer, generate a scene, and render */
int main(int argc, char **argv)
{
	//Set up the scene rendering parameters
	Options options;
	options.width = 640;
	options.height = 480;
	options.fov = 51.52;
	options.cameraToWorld = Matrix44f(-0.949972, 0, -0.312335, 0, -0.200507, 0.766736, 0.609847, 0, 0.239478, 0.641963, -0.728377, 0, 92.824173, 248.831252, -282.326327, 1);
	//0.945519, 0, -0.325569, 0, -0.179534, 0.834209, -0.521403, 0, 0.271593, 0.551447, 0.78876, 0, 4.208271, 8.374532, 17.932925, 1
	//0.707107, 0, - 0.707107, 0, - 0.331295, 0.883452, - 0.331295, 0, 0.624695, 0.468521, 0.624695, 0, 372.77259, 279.579442, 372.77259, 1
	//-0.949972, 0, -0.312335, 0, -0.200507, 0.766736, 0.609847, 0, 0.239478, 0.641963, -0.728377, 0, 92.824173, 248.831252, -282.326327, 1

	//Initialise random
	std::random_device rd;
	std::mt19937 gen(rd());
	std::uniform_real_distribution<> dis(0, 1);

    //Create a random scene of objects
	Raytracer* thisTracer = new Raytracer(options);
    std::vector<std::unique_ptr<Object>> objects;
    uint32_t numSpheres = 10;
	uint32_t numBoxes = 20;
    gen.seed(0);
    //for (uint32_t i = 0; i < numSpheres + numBoxes; ++i) {
        //Vec3f randPos((0.5 - dis(gen)) * 10, (0.5 - dis(gen)) * 10, (0.5 + dis(gen) * 10));
        //float randRadius = (0.5 + dis(gen) * 0.5);
		objects.push_back(std::unique_ptr<Object>(new VolumetricObject("D:\\wdas_cloud\\wdas_cloud_sixteenth.vdb", Vec3f(0, 0, 0))));
		//if (i >= numSpheres) objects.push_back(std::unique_ptr<Object>(new BoxObject(randPos, (randPos + randRadius), Vec3f(dis(gen), dis(gen), dis(gen)))));
		//else objects.push_back(std::unique_ptr<Object>(new Sphere(randPos, randRadius, Vec3f(dis(gen), dis(gen), dis(gen)))));
    //}

	//Render the scene
    thisTracer->render(objects);
    return 0;
}

