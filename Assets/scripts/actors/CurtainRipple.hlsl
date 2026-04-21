#ifndef CURTAIN_RIPPLE_INCLUDED
#define CURTAIN_RIPPLE_INCLUDED

// Matches MAX_RIPPLES in CurtainRipple.cs
#define CURTAIN_MAX_RIPPLES 8

// Populated by MaterialPropertyBlock from CurtainRipple.cs
float4 _RippleOrigins[CURTAIN_MAX_RIPPLES]; // xyz = world pos, w = 1 if active
float4 _RippleParams[CURTAIN_MAX_RIPPLES];  // x=birth_time, y=amplitude, z=lifetime, w=active
float  _RippleSpeed;
float  _RippleWavelength;
int    _RippleCount;

// Computes a UV offset to simulate ripples propagating from world-space impact points.
// WorldPos : per-pixel world position of the curtain surface
// UVScale  : how strongly the ripple pushes UVs (tune per material)
// Returns  : 2D UV offset to add to the sampling UV
void CurtainRipple_Offset_float(float3 WorldPos, float UVScale, out float2 Offset)
{
    float2 total = float2(0.0, 0.0);
    float t_now = _Time.y; // seconds since level load

    int count = min(_RippleCount, CURTAIN_MAX_RIPPLES);

    [unroll(CURTAIN_MAX_RIPPLES)]
    for (int i = 0; i < CURTAIN_MAX_RIPPLES; i++)
    {
        if (i >= count) break;

        float4 origin = _RippleOrigins[i];
        float4 params = _RippleParams[i];

        // Skip inactive slots
        if (params.w < 0.5) continue;

        float age = t_now - params.x;
        float lifetime = max(params.z, 0.0001);

        if (age < 0.0 || age > lifetime) continue;

        float3 delta = WorldPos - origin.xyz;
        float dist = length(delta);

        // Ring that expands outward over time
        float ring_radius = age * _RippleSpeed;

        // Gaussian-ish ring falloff around current radius
        float ring_width = max(_RippleWavelength, 0.0001);
        float ring = exp(-pow((dist - ring_radius) / ring_width, 2.0));

        // Oscillation (cos so peak is at the ring crest)
        float phase = (dist - ring_radius) * (6.2831853 / ring_width);
        float wave = cos(phase) * ring;

        // Temporal fade (1 at birth, 0 at death)
        float life_fade = 1.0 - saturate(age / lifetime);

        // Direction of distortion : radial in the curtain's plane
        // Use delta.xy as approximation of local surface (curtain roughly vertical)
        float2 dir = normalize(delta.xy + float2(1e-5, 1e-5));

        total += dir * wave * params.y * life_fade * UVScale;
    }

    Offset = total;
}

#endif // CURTAIN_RIPPLE_INCLUDED
