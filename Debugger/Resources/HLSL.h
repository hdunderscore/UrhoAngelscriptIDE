// GLSL API header intended to be 'force included' in IDE for GLSL content assist / code completion

// Classes
class bool2
{
// Properties:
bool x;
bool y;
};

class bool3
{
// Properties:
bool x;
bool y;
bool z;
};

class bool4
{
// Properties:
bool x;
bool y;
bool z;
bool w;
};

class int2
{
// Properties:
int x;
int y;
};

class int3
{
// Properties:
int x;
int y;
int z;
};

class int4
{
// Properties:
int x;
int y;
int z;
int w;
};

class float2
{
// Properties:
float x;
float y;
};

class float3
{
// Properties:
float x;
float y;
float z;
};

class float4
{
// Properties:
float x;
float y;
float z;
float w;
};

// array
class mat2
{

};

// array
class float3x3
{

};

// array
class float4x4
{

};

class sampler
{

};

class sampler1D : sampler
{

};

class sampler2D : sampler
{

};

class sampler3D : sampler
{

};

class samplerCUBE : sampler
{

};

class Texture2D
{
};

class Texture3D
{
};

class TextureCube
{
};

// Global functions
float radians(float degrees);
float2 radians(float2 degrees);
float3 radians(float3 degrees);
float4 radians(float4 degrees);
float degrees(float radians);
float2 degrees(float2 radians);
float3 degrees(float3 radians);
float4 degrees(float4 radians);
float sin(float angle);
float2 sin(float2 angle);
float3 sin(float3 angle);
float4 sin(float4 angle);
float cos(float angle);
float2 cos(float2 angle);
float3 cos(float3 angle);
float4 cos(float4 angle);
float tan(float angle);
float2 tan(float2 angle);
float3 tan(float3 angle);
float4 tan(float4 angle);
float asin(float x);
float2 asin(float2 x);
float3 asin(float3 x);
float4 asin(float4 x);
float acos(float x);
float2 acos(float2 x);
float3 acos(float3 x);
float4 acos(float4 x);
float atan(float y_over_x);
float2 atan(float2 y_over_x);
float3 atan(float3 y_over_x);
float4 atan(float4 y_over_x);
float atan2(float y, float x);
float2 atan2(float2 y, float2 x);
float3 atan2(float3 y, float3 x);
float4 atan2(float4 y, float4 x);
float pow(float x, float y);
float2 pow(float2 x, float2 y);
float3 pow(float3 x, float3 y);
float4 pow(float4 x, float4 y);
float exp(float x);
float2 exp(float2 x);
float3 exp(float3 x);
float4 exp(float4 x);
float log(float x);
float2 log(float2 x);
float3 log(float3 x);
float4 log(float4 x);
float exp2(float x);
float2 exp2(float2 x);
float3 exp2(float3 x);
float4 exp2(float4 x);
float log2(float x);
float2 log2(float2 x);
float3 log2(float3 x);
float4 log2(float4 x);
float sqrt(float x);
float2 sqrt(float2 x);
float3 sqrt(float3 x);
float4 sqrt(float4 x);
float inversesqrt(float x);
float2 inversesqrt(float2 x);
float3 inversesqrt(float3 x);
float4 inversesqrt(float4 x);
float abs(float x);
float2 abs(float2 x);
float3 abs(float3 x);
float4 abs(float4 x);
float sign(float x);
float2 sign(float2 x);
float3 sign(float3 x);
float4 sign(float4 x);
float floor(float x);
float2 floor(float2 x);
float3 floor(float3 x);
float4 floor(float4 x);
float ceil(float x);
float2 ceil(float2 x);
float3 ceil(float3 x);
float4 ceil(float4 x);
float frac(float x);
float2 frac(float2 x);
float3 frac(float3 x);
float4 frac(float4 x);
float fmod(float x, float y);
float2 fmod(float2 x, float2 y);
float3 fmod(float3 x, float3 y);
float4 fmod(float4 x, float4 y);
float2 fmod(float2 x, float y);
float3 fmod(float3 x, float y);
float4 fmod(float4 x, float y);
float min(float x, float y);
float2 min(float2 x, float2 y);
float3 min(float3 x, float3 y);
float4 min(float4 x, float4 y);
float2 min(float2 x, float y);
float3 min(float3 x, float y);
float4 min(float4 x, float y);
float max(float x, float y);
float2 max(float2 x, float2 y);
float3 max(float3 x, float3 y);
float4 max(float4 x, float4 y);
float2 max(float2 x, float y);
float3 max(float3 x, float y);
float4 max(float4 x, float y);
float clamp(float x, float minVal, float maxVal);
float2 clamp(float2 x, float2 minVal, float2 maxVal);
float3 clamp(float3 x, float3 minVal, float3 maxVal);
float4 clamp(float4 x, float4 minVal, float4 maxVal);
float2 clamp(float2 x, float minVal, float maxVal);
float3 clamp(float3 x, float minVal, float maxVal);
float4 clamp(float4 x, flfloat minVal, float maxVal);
float lerp(float x, float y, float a);
float2 lerp(float2 x, float2 y, float2 a);
float3 lerp(float3 x, float3 y, float3 a);
float4 lerp(float4 x, float4 y, float4 a);
float2 lerp(float2 x, float2 y, float a);
float3 lerp(float3 x, float3 y, float a);
float4 lerp(float4 x, float4 y, float a);
float step(float edge, float x);
float2 step(float2 edge, float2 x);
float3 step(float3 edge, float3 x);
float4 step(float4 edge, float4 x);
float2 step(float edge, float2 x);
float3 step(float edge, float3 x);
float4 step(float edge, float4 x);
float smoothstep(float edge0, float edge1, float x);
float2 smoothstep(float2 edge0, float2 edge1, float2 x);
float3 smoothstep(float3 edge0, float3 edge1, float3 x);
float4 smoothstep(float4 edge0, float4 edge1, float4 x);
float2 smoothstep(float edge0, float edge1, float2 x);
float3 smoothstep(float edge0, float edge1, float3 x);
float4 smoothstep(float edge0, float edge1, float4 x);
float length(float x);
float length(float2 x);
float length(float3 x);
float length(float4 x);
float distance(float p0, float p1);
float distance(float2 p0, float2 p1);
float distance(float3 p0, float3 p1);
float distance(float4 p0, float4 p1);
float2 dst(float2, float2);
float3 dst(float3, float3);
float4 dst(float4, float4);
float dot(float x, float y);
float dot(float2 x, float2 y);
float dot(float3 x, float3 y);
float dot(float4 x, float4 y);
float3 cross(float3 x, float3 y);
float normalize(float x);
float2 normalize(float2 x);
float3 normalize(float3 x);
float4 normalize(float4 x);
float faceforward(float N, float I, float Nref);
float2 faceforward(float2 N, float2 I, float2 Nref);
float3 faceforward(float3 N, float3 I, float3 Nref);
float4 faceforward(float4 N, float4 I, float4 Nref);
float reflect(float I, float N);
float2 reflect(float2 I, float2 N);
float3 reflect(float3 I, float3 N);
float4 reflect(float4 I, float4 N);
float refract(float I, float N, float eta);
float2 refract(float2 I, float2 N, float eta);
float3 refract(float3 I, float3 N, float eta);
float4 refract(float4 I, float4 N, float eta);
bool any(bool2 x);
bool any(bool3 x);
bool any(bool4 x);
bool all(bool2 x);
bool all(bool3 x);
bool all(bool4 x);
void clip(float);
void clip(float2);
void clip(float3);
void clip(float4);
float ddx(float);
float ddx(float2);
float ddx(float3);
float ddx(float4);
float ddx_coarse(float);
float ddx_coarse(float2);
float ddx_coarse(float3);
float ddx_coarse(float4);
float ddx_fine(float);
float ddx_fine(float2);
float ddx_fine(float3);
float ddx_fine(float4);
float ddy(float);
float ddy(float2);
float ddy(float3);
float ddy(float4);
float ddy_coarse(float);
float ddy_coarse(float2);
float ddy_coarse(float3);
float ddy_coarse(float4);
float ddy_fine(float);
float ddy_fine(float2);
float ddy_fine(float3);
float ddy_fine(float4);
bool isinfinite(float);
bool isinfinite(float2);
bool isinfinite(float3);
bool isinfinite(float4);
bool isinf(float);
bool isinf(float2);
bool isinf(float3);
bool isinf(float4);
bool isnan(float);
bool isnan(float2);
bool isnan(float3);
bool isnan(float4);
float saturate(float);
float2 saturate(float2);
float3 saturate(float3);
float4 saturate(float4);
float4 tex1D(sampler1D, float);
float4 tex2D(sampler2D, float2);
float4 tex3D(sampler3D, float3);
float4 texCUBE(samplerCUBE, float3);
float4 tex1Dbias(sampler1D, float4);
float4 tex2Dbias(sampler2D, float4);
float4 tex3Dbias(sampler3D, float4);
float4 texCUBEbias(samplerCUBE, float4);
float4 tex1Dgrad(sampler1D, float, float ddx, float ddy);
float4 tex2Dgrad(sampler2D, float2, float ddx, float ddy);
float4 tex3Dgrad(sampler3D, float3, float ddx, float ddy);
float4 texCUBEgrad(samplerCUBE, float2, float ddx, float ddy);
float4 tex1Dlod(sampler1D, float4);
float4 tex2Dlod(sampler2D, float4);
float4 tex3Dlod(sampler3D, float4);
float4 texCUBElod(samplerCUBE, float4);
float4 tex1Dproj(sampler1D, float4);
float4 tex2Dproj(sampler2D, float4);
float4 tex3Dproj(sampler3D, float4);
float4 texCUBEproj(samplerCUBE, float4);

// Global properties

// Global constants
