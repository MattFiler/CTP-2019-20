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
	VDBLoader(const std::string filename, bool normalizeSize = false, float densityScale = 1.0f, IntegrationMethod integrationMethod = IntegrationMethod::ExactLinear, SampleMethod sampleMethod = SampleMethod::ExactLinear, int supergridSubsample = 10);

    float density(Vec3f p) const;

    float GetMinDensity() { return minDensity; }
    float GetMaxDensity() { return maxDensity; }
    Vec3i GetMinPos() { return minP; }
    Vec3i GetMaxPos() { return maxP; }
    Vec3f GetDiag() { return diag; }
    Vec3f GetCenter() { return center; }
    Box3f GetBounds() { return _bounds; }
    float GetScale() { return scale; }

private:
    void GenerateSuperGrid();

    float _densityScale;
    bool _normalizeSize;
    IntegrationMethod _integrationMethod;
    SampleMethod _sampleMethod;
    int _supergridSubsample;

    openvdb::FloatGrid::Ptr _densityGrid;

    float minDensity;
    float maxDensity;

    Vec3i minP;
    Vec3i maxP;
    Vec3f diag;
    Vec3f center;
    Box3f _bounds;
    float scale;
};