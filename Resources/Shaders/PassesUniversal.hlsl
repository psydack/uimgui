#ifndef DEARIMGUI_UNIVERSAL_INCLUDED
#define DEARIMGUI_UNIVERSAL_INCLUDED

#include "./Common.hlsl"

#if UNITY_2020_1_OR_NEWER

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#ifndef UNITY_COLORSPACE_GAMMA
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#endif


TEXTURE2D(_Texture);
SAMPLER(sampler_Texture);

half4 unpack_color(uint c)
{
    half4 color = half4(
        (c      ) & 0xff,
        (c >>  8) & 0xff,
        (c >> 16) & 0xff,
        (c >> 24) & 0xff
    ) / 255;
#ifndef UNITY_COLORSPACE_GAMMA
    color.rgb = FastSRGBToLinear(color.rgb);
#endif
    return color;
}

Varyings ImGuiPassVertex(ImVert input)
{
    Varyings output  = (Varyings)0;
    output.vertex    = TransformWorldToHClip(TransformObjectToWorld(float3(input.vertex, 0.0)));
    output.uv        = float2(input.uv.x, 1 - input.uv.y);
    output.color     = unpack_color(input.color);
    return output;
}

half4 ImGuiPassFrag(Varyings input) : SV_Target
{
    return input.color * SAMPLE_TEXTURE2D(_Texture, sampler_Texture, input.uv);
}

#else
// TODO: Do a better way to compile only on unity_2020+.
// The code below is only to editor compile doesn't throw exceptions.
Varyings ImGuiPassVertex(ImVert input) { return (Varyings)0; }
half4 ImGuiPassFrag(Varyings input) : SV_Target{ return input.color; }
#endif
#endif
