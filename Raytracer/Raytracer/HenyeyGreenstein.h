#pragma once
#include "math.h"
#include "OrthoNormalBasis.h"

class HenyeyGreenstein {
public:
	float eval(const Vec3f& wo, const Vec3f& wi, float c)
	{
		const float k = 1.0f + (g * g) - (2.0f * g * Dot(wi, wo));
		return  (c / (4.0f * M_PI)) * ((1.0f - (g * g)) / (k * sqrtf(k)));
	}
	float sample(float s1, float s2, const Vec3f& wo, Vec3f& wi, float& _pdf, float c)
	{
		float costheta;
		if (g < EPSILON)
		{
			costheta = 1.0f - (2.0f * s1);
		}
		else
		{
			costheta = (1.0f - (g * g)) / (1.0f - g + (2.0f * g * s1));
			costheta = (1.0f + (g * g) - (costheta * costheta)) / (2.0f * g);
		}
		float sintheta;
		sintheta = sqrtf(1.0f - (costheta * costheta));
		float phi;
		phi = s2 * (2.0 * M_PI);
		wi = Vec3f(sintheta * cos(phi), sintheta * sin(phi), costheta);
		OrthoNormalBasis onb;
		SF_Vector3 onb_wo = SF_Vector3(wo.x, wo.y, wo.z);
		SF_Vector3 onb_wi = SF_Vector3(wi.x, wi.y, wi.z);
		onb = OrthoNormalBasis::makeFromW(onb_wo); // create a frame around wo
		onb_wi = onb.transform(onb_wi);
		wi = Vec3f(onb_wi.x, onb_wi.y, onb_wi.z); // align wi to frame
		_pdf = pdf(wo, wi);
		return eval(wo, wi, c);
	}
	float pdf(const Vec3f& wo, const Vec3f& wi)
	{
		float pdf;
		const float k = 1.0f + (g * g) - (2.0f * g * Dot(wi, wo));
		pdf = (1.0f / (4.0f * M_PI)) * ((1.0f - (g * g)) / (k * sqrtf(k)));
		return pdf;
	}

private:
	float g = 0.8f; //[-1 to 1]
	float EPSILON = 0.0f; //DUNNO WHAT THIS SHOULD BE
};