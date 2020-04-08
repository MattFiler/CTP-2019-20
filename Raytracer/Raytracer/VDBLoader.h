#pragma once
#include "math.h"
#include "Box.h"

#include <string>
#include <openvdb/openvdb.h>
#include <openvdb/tools/Interpolation.h>

typedef openvdb::tree::Tree4<openvdb::Vec2s, 5, 4, 3>::Type Vec2fTree;
typedef openvdb::Grid<Vec2fTree> Vec2fGrid;

enum class IntegrationMethod
{
    ExactNearest,
    ExactLinear,
    Raymarching,
    ResidualRatio,
};
enum class SampleMethod
{
    ExactNearest,
    ExactLinear,
    Raymarching,
};

class VDBLoader {
public:
	VDBLoader(std::string filename);
    float density(Vec3f p) const;

private:
    void GenerateSuperGrid();

    float _densityScale = 1.0f;
    bool _normalizeSize = false;
    IntegrationMethod _integrationMethod = IntegrationMethod::ExactLinear;
    SampleMethod _sampleMethod = SampleMethod::ExactLinear;
    int _supergridSubsample = 10;

    openvdb::FloatGrid::Ptr _densityGrid;
    float scale;
};