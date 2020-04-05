#pragma warning(disable:4996)
#include "Raytracer.h"
#include <openvdb/openvdb.h>
#include <openvdb/tools/ChangeBackground.h>
#include "OrthoNormalBasis.h"

/* Initialise the raytracer, generate a scene, and render */
int main(int argc, char **argv)
{
	openvdb::initialize();
	// Create a VDB file object.
	openvdb::io::File file("C:\\Users\\mattf\\Downloads\\wdas_cloud\\wdas_cloud\\wdas_cloud.vdb");
	// Open the file.  This reads the file header, but not any grids.
	file.open();
	// Loop over all grids in the file and retrieve a shared pointer
	// to the one named "LevelSetSphere".  (This can also be done
	// more simply by calling file.readGrid("LevelSetSphere").)
	openvdb::GridBase::Ptr baseGrid;
	for (openvdb::io::File::NameIterator nameIter = file.beginName();
		nameIter != file.endName(); ++nameIter)
	{
		// Read in only the grid we are interested in.
		if (nameIter.gridName() == "density") {
			baseGrid = file.readGrid(nameIter.gridName());
		}
		else {
			std::cout << "skipping grid " << nameIter.gridName() << std::endl;
		}
	}
	file.close();
	// From the example above, "LevelSetSphere" is known to be a FloatGrid,
	// so cast the generic grid pointer to a FloatGrid pointer.
	openvdb::FloatGrid::Ptr grid = openvdb::gridPtrCast<openvdb::FloatGrid>(baseGrid);
	// Convert the level set sphere to a narrow-band fog volume, in which
	// interior voxels have value 1, exterior voxels have value 0, and
	// narrow-band voxels have values varying linearly from 0 to 1.
	const float outside = grid->background();
	const float width = 2.0 * outside;
	// Visit and update all of the grid's active values, which correspond to
	// voxels on the narrow band.
	for (openvdb::FloatGrid::ValueOnIter iter = grid->beginValueOn(); iter; ++iter) {
		float dist = iter.getValue();
		iter.setValue((outside - dist) / width);
	}
	// Visit all of the grid's inactive tile and voxel values and update the values
	// that correspond to the interior region.
	for (openvdb::FloatGrid::ValueOffIter iter = grid->beginValueOff(); iter; ++iter) {
		if (iter.getValue() < 0.0) {
			iter.setValue(1.0);
			iter.setValueOff();
		}
	}
	// Set exterior voxels to 0.
	openvdb::tools::changeBackground(grid->tree(), 0.0);

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
