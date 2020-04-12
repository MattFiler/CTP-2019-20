#pragma warning(disable:4996)
#include "Raytracer.h"
#include "OrthoNormalBasis.h"
#include "VDBLoader.h"
#include "HenyeyGreenstein.h"
#include <DirectXMath.h>
#include "nlohmann/json.hpp"
using json = nlohmann::json;

/* Initialise the raytracer, generate a scene, and render */
int main(int argc, char **argv)
{
	//Load custom config
	json config;
	std::fstream config_file("config.json");
	config_file >> config;

	//Global configs
	Globals::sunDirection = Vec3f(config["sun_direction"]["x"], config["sun_direction"]["y"], config["sun_direction"]["z"]); //todo: pull this together with sky model?

	//Work out camera to world matrix
	DirectX::XMFLOAT3 rotation = DirectX::XMFLOAT3(
		DirectX::XMConvertToRadians(config["camera_rotation"]["x"]), 
		DirectX::XMConvertToRadians(161.8),
		DirectX::XMConvertToRadians(0));
	DirectX::XMFLOAT3 position = DirectX::XMFLOAT3(
		config["camera_position"]["x"], 
		config["camera_position"]["y"], 
		config["camera_position"]["z"]);
	DirectX::XMMATRIX matrix = 
		DirectX::XMMatrixScaling(1, 1, 1) * 
		DirectX::XMMatrixRotationRollPitchYaw(rotation.x, rotation.y, rotation.z) * 
		DirectX::XMMatrixTranslation(position.x, position.y, position.z);
	DirectX::XMFLOAT4X4 matrix_f;
	DirectX::XMStoreFloat4x4(&matrix_f, matrix);

	Matrix44f cameraToWorld = Matrix44f(
		matrix_f.m[0][0], matrix_f.m[0][1], matrix_f.m[0][2], matrix_f.m[0][3],
		matrix_f.m[1][0], matrix_f.m[1][1], matrix_f.m[1][2], matrix_f.m[1][3],
		matrix_f.m[2][0], matrix_f.m[2][1], matrix_f.m[2][2], matrix_f.m[2][3],
		matrix_f.m[3][0], matrix_f.m[3][1], matrix_f.m[3][2], matrix_f.m[3][3]);

	//Create our raytracer object
	Raytracer* thisTracer = new Raytracer(
		(float)config["render_resolution"]["width"],
		(float)config["render_resolution"]["height"],
		(float)config["camera_fov"],
		cameraToWorld,
		(float)config["sky_model"]["ground_albedo"],
		(float)config["sky_model"]["turbidity"],
		Radians((float)config["sky_model"]["sun_elevation"]));

	//Initialise random
	std::random_device rd;
	std::mt19937 gen(rd());
	std::uniform_real_distribution<> dis(0, 1);

    //Create the scene (TODO: MAKE THIS TOTALLY DYNAMIC, WITH TRANSFORMS)
    std::vector<std::unique_ptr<Object>> objects;
    uint32_t numSpheres = 10;
	uint32_t numBoxes = 20;
    gen.seed(0);
    //for (uint32_t i = 0; i < numSpheres + numBoxes; ++i) {
        //Vec3f randPos((0.5 - dis(gen)) * 10, (0.5 - dis(gen)) * 10, (0.5 + dis(gen) * 10));
        //float randRadius = (0.5 + dis(gen) * 0.5);
		objects.push_back(std::unique_ptr<Object>(new VolumetricObject(config["temp_vdb"], Vec3f(0, 0, 0))));
		//if (i >= numSpheres) objects.push_back(std::unique_ptr<Object>(new BoxObject(randPos, (randPos + randRadius), Vec3f(dis(gen), dis(gen), dis(gen)))));
		//else objects.push_back(std::unique_ptr<Object>(new Sphere(randPos, randRadius, Vec3f(dis(gen), dis(gen), dis(gen)))));
    //}

	//Render the scene
    thisTracer->render(objects);
    return 0;
}

