#ifndef LIGHTWEIGHT_SS3D_FORWARD_LIT_PASS_INCLUDED
#define LIGHTWEIGHT_SS3D_FORWARD_LIT_PASS_INCLUDED

#include "Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitForwardPass.hlsl"

// Used in Standard (Physically Based) shader
half4 LitPassFragmentHalfPBR(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

    half4 color = LightweightFragmentHalfPBR(inputData, surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion, surfaceData.emission, surfaceData.alpha);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}

#endif
