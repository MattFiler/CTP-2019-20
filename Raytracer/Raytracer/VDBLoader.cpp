#include "VDBLoader.h"

VDBLoader::VDBLoader(const std::string filename, bool normalizeSize, float densityScale, IntegrationMethod integrationMethod, SampleMethod sampleMethod, int supergridSubsample)
{
    _normalizeSize = normalizeSize;
    _densityScale = densityScale;
    _integrationMethod = integrationMethod;
    _sampleMethod = sampleMethod;
    _supergridSubsample = supergridSubsample;

    openvdb::initialize();

    //Read the density grid from our VDB file
    openvdb::io::File file(filename);
    file.open();
    openvdb::GridBase::Ptr ptr;
    for (openvdb::io::File::NameIterator nameIter = file.beginName(); nameIter != file.endName(); ++nameIter)
    {
        if (nameIter.gridName() == "density") {
            ptr = file.readGrid(nameIter.gridName());
        }
    }
    file.close();

    _densityGrid = openvdb::gridPtrCast<openvdb::FloatGrid>(ptr);

    auto accessor = _densityGrid->getAccessor();
    for (openvdb::FloatGrid::ValueOnIter iter = _densityGrid->beginValueOn(); iter.test(); ++iter)
        iter.setValue((*iter) * _densityScale);

    _densityGrid->evalMinMax(minDensity, maxDensity);

    Vec3d densityCenter(*ptr->transform().indexToWorld(openvdb::Vec3d(0, 0, 0)).asPointer());
    Vec3d densitySpacing(*ptr->transform().indexToWorld(openvdb::Vec3d(1, 1, 1)).asPointer());
    densitySpacing = densitySpacing - densityCenter;

    openvdb::CoordBBox bbox = _densityGrid->evalActiveVoxelBoundingBox();
    minP = Vec3i(bbox.min().x(), bbox.min().y(), bbox.min().z());
    maxP = Vec3i(bbox.max().x(), bbox.max().y(), bbox.max().z()) + 1;
    diag = Vec3f(maxP.x, maxP.y, maxP.z) - Vec3f(minP.x, minP.y, minP.z);

    if (_normalizeSize) {
        scale = 1.0f / diag.max();
        diag *= scale;
        center = Vec3f(minP.x, minP.y, minP.z) * scale + Vec3f(diag.x, 0.0f, diag.z) * 0.5f;
    }
    else {
        scale = densitySpacing.min();
        center = -Vec3f(densityCenter.x, densityCenter.y, densityCenter.z);
    }

    if (_integrationMethod == IntegrationMethod::ResidualRatio)
        GenerateSuperGrid();

    //_transform = Mat4f::translate(-center) * Mat4f::scale(Vec3f(scale));
    //_invTransform = Mat4f::scale(Vec3f(1.0f / scale)) * Mat4f::translate(center);
    _bounds = Box3f(Vec3f(minP.x, minP.y, minP.z), Vec3f(maxP.x, maxP.y, maxP.z));

    if (_sampleMethod == SampleMethod::ExactLinear || _integrationMethod == IntegrationMethod::ExactLinear) {
        auto accessor = _densityGrid->getAccessor();
        for (openvdb::FloatGrid::ValueOnCIter iter = _densityGrid->cbeginValueOn(); iter.test(); ++iter) {
            if (*iter != 0.0f)
                for (int z = -1; z <= 1; ++z)
                    for (int y = -1; y <= 1; ++y)
                        for (int x = -1; x <= 1; ++x)
                            accessor.setValueOn(iter.getCoord() + openvdb::Coord(x, y, z));
            _bounds = Box3f(Vec3f(minP.x, minP.y, minP.z) - 1, Vec3f(maxP.x, maxP.y, maxP.z) + 1);
        }
    }

    std::string test = "";

    //_invConfigTransform = _configTransform.invert();
}

void VDBLoader::GenerateSuperGrid()
{
    const int offset = _supergridSubsample / 2;
    auto divideCoord = [&](const openvdb::Coord& a)
    {
        return openvdb::Coord(
            roundDown(a.x() + offset, _supergridSubsample),
            roundDown(a.y() + offset, _supergridSubsample),
            roundDown(a.z() + offset, _supergridSubsample));
    };

    Vec2fGrid::Ptr _superGrid = Vec2fGrid::create(openvdb::Vec2s(0.0f));
    auto accessor = _superGrid->getAccessor();

    Vec2fGrid::Ptr minMaxGrid = Vec2fGrid::create(openvdb::Vec2s(1e30f, 0.0f));
    auto minMaxAccessor = minMaxGrid->getAccessor();

    for (openvdb::FloatGrid::ValueOnCIter iter = _densityGrid->cbeginValueOn(); iter.test(); ++iter) {
        openvdb::Coord coord = divideCoord(iter.getCoord());
        float d = *iter;
        accessor.setValue(coord, openvdb::Vec2s(accessor.getValue(coord).x() + d, 0.0f));

        openvdb::Vec2s minMax = minMaxAccessor.getValue(coord);
        minMaxAccessor.setValue(coord, openvdb::Vec2s(min(minMax.x(), d), max(minMax.y(), d)));
    }

    float normalize = 1.0f / cube(_supergridSubsample);
    const float Gamma = 2.0f;
    const float D = std::sqrt(3.0f) * _supergridSubsample;
    for (Vec2fGrid::ValueOnIter iter = _superGrid->beginValueOn(); iter.test(); ++iter) {
        openvdb::Vec2s minMax = minMaxAccessor.getValue(iter.getCoord());

        float muMin = minMax.x();
        float muMax = minMax.y();
        float muAvg = iter->x() * normalize;
        float muR = muMax - muMin;
        float muC = clamp(muMin + muR * (std::pow(Gamma, 1.0f / (D * muR)) - 1.0f), muMin, muAvg);
        iter.setValue(openvdb::Vec2s(muC, 0.0f));
    }

    for (openvdb::FloatGrid::ValueOnCIter iter = _densityGrid->cbeginValueOn(); iter.test(); ++iter) {
        openvdb::Coord coord = divideCoord(iter.getCoord());
        openvdb::Vec2s v = accessor.getValue(coord);
        float residual = max(v.y(), std::abs(*iter - v.x()));
        accessor.setValue(coord, openvdb::Vec2s(v.x(), residual));
    }
}

template<typename TreeT>
static inline float gridAt(TreeT& acc, Vec3f p)
{
    return openvdb::tools::BoxSampler::sample(acc, openvdb::Vec3R(p.x, p.y, p.z));
}

float VDBLoader::density(Vec3f p) const
{
    return gridAt(_densityGrid->tree(), p);
}