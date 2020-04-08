#include "math.h"

/* 
A partial conversion of some functionality from SunFlow
Based on: https://code.google.com/archive/p/sunflowsharp/
*/

class SF_Vector3
{
public:
    static std::vector<float> COS_THETA;
    static std::vector<float> SIN_THETA;
    static std::vector<float> COS_PHI;
    static std::vector<float> SIN_PHI;

    float x, y, z;

    SF_Vector3()
    {
        if (COS_THETA.size() == 0)
        {
            COS_THETA.reserve(256);
            SIN_THETA.reserve(256);
            COS_PHI.reserve(256);
            SIN_PHI.reserve(256);
            for (int i = 0; i < 256; i++)
            {
                double angle = (i * M_PI) / 256.0;
                COS_THETA[i] = (float)cos(angle);
                SIN_THETA[i] = (float)sin(angle);
                COS_PHI[i] = (float)cos(2 * angle);
                SIN_PHI[i] = (float)sin(2 * angle);
            }
        }
    }

    SF_Vector3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    static SF_Vector3 decode(short n, SF_Vector3 dest) {
        int t = (int)((unsigned int)(n & 0xFF00) >> 8);//>>>
        int p = n & 0xFF;
        dest.x = SIN_THETA[t] * COS_PHI[p];
        dest.y = SIN_THETA[t] * SIN_PHI[p];
        dest.z = COS_THETA[t];
        return dest;
    }

    static SF_Vector3 decode(short n)
    {
        return decode(n, SF_Vector3());
    }

    short encode()
    {
        int theta = (int)(acos(z) * (256.0 / M_PI));
        if (theta > 255)
            theta = 255;
        int phi = (int)(atan2(y, x) * (128.0 / M_PI));
        if (phi < 0)
            phi += 256;
        else if (phi > 255)
            phi = 255;
        return (short)(((theta & 0xFF) << 8) | (phi & 0xFF));
    }

    float get(int i)
    {
        switch (i)
        {
            case 0:
                return x;
            case 1:
                return y;
            default:
                return z;
        }
    }

    float Length()
    {
        return (float)sqrt((x * x) + (y * y) + (z * z));
    }

    float LengthSquared()
    {
        return (x * x) + (y * y) + (z * z);
    }

    SF_Vector3 negate()
    {
        x = -x;
        y = -y;
        z = -z;
        return *this;
    }

    SF_Vector3 negate(SF_Vector3 dest)
    {
        dest.x = -x;
        dest.y = -y;
        dest.z = -z;
        return dest;
    }

    SF_Vector3 mul(float s)
    {
        x *= s;
        y *= s;
        z *= s;
        return *this;
    }

    SF_Vector3 mul(float s, SF_Vector3 dest)
    {
        dest.x = x * s;
        dest.y = y * s;
        dest.z = z * s;
        return dest;
    }

    SF_Vector3 div(float d)
    {
        x /= d;
        y /= d;
        z /= d;
        return *this;
    }

    SF_Vector3 div(float d, SF_Vector3 dest)
    {
        dest.x = x / d;
        dest.y = y / d;
        dest.z = z / d;
        return dest;
    }

    float normalizeLength()
    {
        float n = (float)sqrt(x * x + y * y + z * z);
        float inf = 1.0f / n;
        x *= inf;
        y *= inf;
        z *= inf;
        return n;
    }

    SF_Vector3 normalize()
    {
        float inf = 1.0f / (float)sqrt((x * x) + (y * y) + (z * z));
        x *= inf;
        y *= inf;
        z *= inf;
        return *this;
    }

    SF_Vector3 normalize(SF_Vector3 dest)
    {
        float inf = 1.0f / (float)sqrt((x * x) + (y * y) + (z * z));
        dest.x = x * inf;
        dest.y = y * inf;
        dest.z = z * inf;
        return dest;
    }

    SF_Vector3 set(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
        return *this;
    }

    SF_Vector3 set(SF_Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
        return *this;
    }

    float dot(float vx, float vy, float vz)
    {
        return vx * x + vy * y + vz * z;
    }

    static float dot(SF_Vector3 v1, SF_Vector3 v2)
    {
        return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
    }

    static SF_Vector3 cross(SF_Vector3 v1, SF_Vector3 v2, SF_Vector3 dest)
    {
        dest.x = (v1.y * v2.z) - (v1.z * v2.y);
        dest.y = (v1.z * v2.x) - (v1.x * v2.z);
        dest.z = (v1.x * v2.y) - (v1.y * v2.x);
        return dest;
    }

    static SF_Vector3 add(SF_Vector3 v1, SF_Vector3 v2, SF_Vector3 dest)
    {
        dest.x = v1.x + v2.x;
        dest.y = v1.y + v2.y;
        dest.z = v1.z + v2.z;
        return dest;
    }

    static SF_Vector3 sub(SF_Vector3 v1, SF_Vector3 v2, SF_Vector3 dest)
    {
        dest.x = v1.x - v2.x;
        dest.y = v1.y - v2.y;
        dest.z = v1.z - v2.z;
        return dest;
    }
};

class OrthoNormalBasis
{
private:
    SF_Vector3 u;
    SF_Vector3 v;
    SF_Vector3 w;

public:
    OrthoNormalBasis()
    {
        u = SF_Vector3();
        v = SF_Vector3();
        w = SF_Vector3();
    }

    void flipU()
    {
        u.negate();
    }

    void flipV()
    {
        v.negate();
    }

    void flipW()
    {
        w.negate();
    }

    void swapUV()
    {
        SF_Vector3 t = u;
        u = v;
        v = t;
    }

    void swapVW()
    {
        SF_Vector3 t = v;
        v = w;
        w = t;
    }

    void swapWU()
    {
        SF_Vector3 t = w;
        w = u;
        u = t;
    }

    SF_Vector3 transform(SF_Vector3 a, SF_Vector3 dest)
    {
        dest.x = (a.x * u.x) + (a.y * v.x) + (a.z * w.x);
        dest.y = (a.x * u.y) + (a.y * v.y) + (a.z * w.y);
        dest.z = (a.x * u.z) + (a.y * v.z) + (a.z * w.z);
        return dest;
    }

    SF_Vector3 transform(SF_Vector3 a)
    {
        float x = (a.x * u.x) + (a.y * v.x) + (a.z * w.x);
        float y = (a.x * u.y) + (a.y * v.y) + (a.z * w.y);
        float z = (a.x * u.z) + (a.y * v.z) + (a.z * w.z);
        return a = SF_Vector3(x, y, z);
    }

    static OrthoNormalBasis makeFromW(SF_Vector3 w)
    {
        OrthoNormalBasis onb = OrthoNormalBasis();
        w.normalize(onb.w);
        if ((abs(onb.w.x) < abs(onb.w.y)) && (abs(onb.w.x) < abs(onb.w.z)))
        {
            onb.v.x = 0;
            onb.v.y = onb.w.z;
            onb.v.z = -onb.w.y;
        }
        else if (abs(onb.w.y) < abs(onb.w.z))
        {
            onb.v.x = onb.w.z;
            onb.v.y = 0;
            onb.v.z = -onb.w.x;
        }
        else
        {
            onb.v.x = onb.w.y;
            onb.v.y = -onb.w.x;
            onb.v.z = 0;
        }
        SF_Vector3::cross(onb.v.normalize(), onb.w, onb.u);
        return onb;
    }

    static OrthoNormalBasis makeFromWV(SF_Vector3 w, SF_Vector3 v)
    {
        OrthoNormalBasis onb = OrthoNormalBasis();
        w.normalize(onb.w);
        SF_Vector3::cross(v, onb.w, onb.u).normalize();
        SF_Vector3::cross(onb.w, onb.u, onb.v);
        return onb;
    }
};