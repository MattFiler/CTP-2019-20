#pragma warning(disable:4996)
#include "Raytracer.h"
#include <openvdb/openvdb.h>
#include <openvdb/tools/ChangeBackground.h>
#include "OrthoNormalBasis.h"

/* Initialise the raytracer, generate a scene, and render */
int main(int argc, char **argv)
{
	openvdb::initialize();

    float _densityScale = 1.0f;
    bool _normalizeSize = false;

	//Read the density grid from our VDB file
	openvdb::io::File file("D:\\wdas_cloud\\wdas_cloud_sixteenth.vdb");
	file.open();
	openvdb::GridBase::Ptr ptr;
	for (openvdb::io::File::NameIterator nameIter = file.beginName(); nameIter != file.endName(); ++nameIter)
	{
		if (nameIter.gridName() == "density") {
			ptr = file.readGrid(nameIter.gridName());
		}
	}
	file.close();

	openvdb::FloatGrid::Ptr _densityGrid = openvdb::gridPtrCast<openvdb::FloatGrid>(ptr);

    auto accessor = _densityGrid->getAccessor();
    for (openvdb::FloatGrid::ValueOnIter iter = _densityGrid->beginValueOn(); iter.test(); ++iter)
        iter.setValue((*iter) * _densityScale);

    Vec3d densityCenter(*ptr->transform().indexToWorld(openvdb::Vec3d(0, 0, 0)).asPointer());
    Vec3d densitySpacing(*ptr->transform().indexToWorld(openvdb::Vec3d(1, 1, 1)).asPointer());
    densitySpacing = densitySpacing - densityCenter;

    openvdb::CoordBBox bbox = _densityGrid->evalActiveVoxelBoundingBox();
    Vec3i minP = Vec3i(bbox.min().x(), bbox.min().y(), bbox.min().z());
    Vec3i maxP = Vec3i(bbox.max().x(), bbox.max().y(), bbox.max().z()) + 1;
    Vec3f diag = Vec3f(maxP.x, maxP.y, maxP.z) - Vec3f(minP.x, minP.y, minP.z);

    //Work out scale to use
    float scale;
    Vec3f center;
    if (_normalizeSize) {
        scale = 1.0f / diag.max();
        diag *= scale;
        center = Vec3f(minP.x, minP.y, minP.z) * scale + Vec3f(diag.x, 0.0f, diag.z) * 0.5f;
    }
    else {
        scale = densitySpacing.min();
        center = -Vec3f(densityCenter.x, densityCenter.y, densityCenter.z);
    }

    /*
    if (_integrationMethod == IntegrationMethod::ResidualRatio)
        generateSuperGrid();

    _transform = Mat4f::translate(-center) * Mat4f::scale(Vec3f(scale));
    _invTransform = Mat4f::scale(Vec3f(1.0f / scale)) * Mat4f::translate(center);
    _bounds = Box3f(Vec3f(minP), Vec3f(maxP));

    if (_sampleMethod == SampleMethod::ExactLinear || _integrationMethod == IntegrationMethod::ExactLinear) {
        auto accessor = _densityGrid->getAccessor();
        for (openvdb::FloatGrid::ValueOnCIter iter = _densityGrid->cbeginValueOn(); iter.test(); ++iter) {
            if (*iter != 0.0f)
                for (int z = -1; z <= 1; ++z)
                    for (int y = -1; y <= 1; ++y)
                        for (int x = -1; x <= 1; ++x)
                            accessor.setValueOn(iter.getCoord() + openvdb::Coord(x, y, z));
            _bounds = Box3f(Vec3f(minP - 1), Vec3f(maxP + 1));
        }
    }

    _invConfigTransform = _configTransform.invert();
    */

	std::string test;
	std::cin >> test;

	/*
	//Set up the scene rendering parameters
	Options options;
	options.width = 640;
	options.height = 480;
	options.fov = 51.52;
	options.cameraToWorld = Matrix44f(0.945519, 0, -0.325569, 0, -0.179534, 0.834209, -0.521403, 0, 0.271593, 0.551447, 0.78876, 0, 4.208271, 8.374532, 17.932925, 1);

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
    for (uint32_t i = 0; i < numSpheres + numBoxes; ++i) {
        Vec3f randPos((0.5 - dis(gen)) * 10, (0.5 - dis(gen)) * 10, (0.5 + dis(gen) * 10));
        float randRadius = (0.5 + dis(gen) * 0.5);
		if (i >= numSpheres) objects.push_back(std::unique_ptr<Object>(new Box(randPos, (randPos + randRadius), Vec3f(dis(gen), dis(gen), dis(gen)))));
		else objects.push_back(std::unique_ptr<Object>(new Sphere(randPos, randRadius, Vec3f(dis(gen), dis(gen), dis(gen)))));
    }

	//Render the scene
    thisTracer->render(objects);
    return 0;
	*/
}
