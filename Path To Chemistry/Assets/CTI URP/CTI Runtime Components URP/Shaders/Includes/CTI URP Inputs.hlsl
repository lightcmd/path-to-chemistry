#ifndef INPUT_SURFACE_CTI_INCLUDED
#define INPUT_SURFACE_CTI_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

//  Bark and Billbords use the built in lighting function
#if defined(CTIBARK)
// || defined (CTIBILLBOARD)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif


CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half3 _BaseWindMultipliers;
half3 _SpecColor;
half4 _HueVariation;
half _Smoothness;
half _BackfaceSmoothness;

//  Leaf specific inputs
half _Cutoff;
half3 _Translucency;
half _AmbientReflection;
half _TumbleStrength;
half _TumbleFrequency;
half _LeafTurbulence;
half _EdgeFlutterInfluence;

//  Billboard specific inputs
half _AlphaLeak;
half _OcclusionStrength;
half _BumpScale;
half _WindStrength;

half _Wrap;

//  Fix for old bug
half _BillboardScale;

CBUFFER_END

//  Custom packed Map for Leaf and Billboard shader
#if !defined(CTIBARK)
        TEXTURE2D(_BumpSpecMap); SAMPLER(sampler_BumpSpecMap);
#else
TEXTURE2D(_BumpOcclusionMap);
SAMPLER(sampler_BumpOcclusionMap);
#endif

#if defined(CTIBARKARRAY)
        TEXTURE2D_ARRAY(_BaseMapArray); SAMPLER(sampler_BaseMapArray);
        TEXTURE2D_ARRAY(_BumpOcclusionMapArray); SAMPLER(sampler_BumpOcclusionMapArray);
#endif


struct CTIVertexInput
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    #if !defined(SHADOWSONLYPASS) && !defined(DEPTHONLYPASS) && !defined(DEPTHNORMALPASS)
            float4 tangentOS                : TANGENT;
    #endif
    float2 texcoord : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
    #if defined(CTILEAVES)
            float3 texcoord2                : TEXCOORD2; // Pivot
    #endif
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct CTIVertexOutput
{
    float4 positionCS : SV_POSITION;
    #if defined(CTIBARK)
    #if !defined(SHADOWSONLYPASS) && !defined(DEPTHONLYPASS) && !defined(DEPTHNORMALPASS)
                float2 uv                       : TEXCOORD0;
    #endif
    #else
            float2 uv                           : TEXCOORD0;
    #endif

    #if !defined(SHADOWSONLYPASS) && !defined(DEPTHONLYPASS)
    //DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
    half3 normalWS : TEXCOORD1;
    #ifdef _NORMALMAP
                half4 tangentWS                 : TEXCOORD2;
    #endif

    #if !defined(DEPTHNORMALPASS)
                //  Due to the order of our includes we have to check for both defines? 
                //#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR) || !defined(_RECEIVE_SHADOWS_OFF)
                    float3 positionWS           : TEXCOORD3;
                //#endif
                half3 vertexSH                  : TEXCOORD4;
                half4 fogFactorAndVertexLight   : TEXCOORD5; // x: fogFactor, yzw: vertex light
            //  Due to the order of our includes we have to check for both defines  
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) || !defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                    float4 shadowCoord          : TEXCOORD6;
    #endif
    #if defined(CTIBARKARRAY)
                    half3 occlusionVariation    : TEXCOORD7;
    #else
                    half2 occlusionVariation    : TEXCOORD7;
    #endif
    #endif
    #endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

//  Surface functions ---------------------------

struct SurfaceDescriptionLeaves
{
    float3 albedo;
    float alpha;
    float3 normalTS;
    float3 emission;
    float metallic;
    float3 specular;
    float smoothness;
    float occlusion;
    //float AlphaClipThreshold;
    float translucency;
};

//  Leaves ---------------------

#if !defined(CTIBARK) && !defined(CTIBILLBOARD)
        inline void InitializeLeavesLitSurfaceData(half colorVariation, float2 uv, out SurfaceDescriptionLeaves outSurfaceData)
        {
            
            half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
            outSurfaceData.alpha = Alpha(albedoAlpha.a, 1, _Cutoff);

        //  Add Color Variation
            albedoAlpha.rgb = lerp(albedoAlpha.rgb, (albedoAlpha.rgb + _HueVariation.rgb) * 0.5, colorVariation * _HueVariation.a);
            
            outSurfaceData.albedo = albedoAlpha.rgb;
            outSurfaceData.metallic = 0;
            outSurfaceData.specular = _SpecColor;
            //outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
#if defined (_NORMALMAP)
                half4 sampleNormal = SAMPLE_TEXTURE2D(_BumpSpecMap, sampler_BumpSpecMap, uv);
                half3 tangentNormal;
                tangentNormal.xy = sampleNormal.ag * 2 - 1;
                tangentNormal.z = sqrt(1.0 - dot(tangentNormal.xy, tangentNormal.xy));  
                outSurfaceData.normalTS = tangentNormal;
                outSurfaceData.smoothness = sampleNormal.b * _Smoothness;
                outSurfaceData.translucency = sampleNormal.r;
#else
                outSurfaceData.normalTS = half3(0, 0, 1);
                outSurfaceData.smoothness = _Smoothness;
                outSurfaceData.translucency = 1;
#endif
            outSurfaceData.occlusion = 1;
            outSurfaceData.emission = 0;

        }

    //  This is needed by the Meta pass
        inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
        {
            half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));

        //  Zero initialize outSurfaceData as URP 10 might expect clear coat here.
            outSurfaceData = (SurfaceData)0;

            outSurfaceData.alpha = Alpha(albedoAlpha.a, 1, _Cutoff);
            outSurfaceData.albedo = albedoAlpha.rgb;
            outSurfaceData.metallic = 0;
            outSurfaceData.specular = _SpecColor;
            outSurfaceData.smoothness = .5; //_Gloss;
            outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
            outSurfaceData.occlusion = 1;
            outSurfaceData.emission = 0;
        }

//  Bark ---------------------

#else
#if !defined(CTIBILLBOARD)
inline void InitializeStandardLitSurfaceData(
    #if !defined(BARKMETA)
    #if defined(CTIBARKARRAY)
                        half2 colorVariation,
    #else
    half colorVariation,
    #endif
    #endif
    float2 uv, out SurfaceData outSurfaceData)
{
    #if defined(CTIBARKARRAY)
                    half4 albedoAlpha = SAMPLE_TEXTURE2D_ARRAY(_BaseMapArray, sampler_BaseMapArray, uv, colorVariation.y); 
    #else
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    #endif
    outSurfaceData.alpha = 1;

    //  Add Color Variation
    #if !defined(BARKMETA)
    albedoAlpha.rgb = lerp(albedoAlpha.rgb, (albedoAlpha.rgb + _HueVariation.rgb) * 0.5h,
                           colorVariation.x * _HueVariation.a);
    #endif

    //  Zero initialize outSurfaceData as URP 10 might expect clear coat here.
    outSurfaceData = (SurfaceData)0;

    outSurfaceData.albedo = albedoAlpha.rgb;
    outSurfaceData.metallic = 0;
    outSurfaceData.specular = _SpecColor;
    outSurfaceData.smoothness = albedoAlpha.a * _Smoothness;
    //outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    #if defined (_NORMALMAP)
    #if defined(CTIBARKARRAY)
                        half4 sampleNormal = SAMPLE_TEXTURE2D_ARRAY(_BumpOcclusionMapArray, sampler_BumpOcclusionMapArray, uv, colorVariation.y); 
    #else
                        half4 sampleNormal = SAMPLE_TEXTURE2D(_BumpOcclusionMap, sampler_BumpOcclusionMap, uv);
    #endif
                    
                    half3 tangentNormal;
                    tangentNormal.xy = sampleNormal.ag * 2.0h - 1.0h;
                    tangentNormal.z = sqrt(1.0h - dot(tangentNormal.xy, tangentNormal.xy));  
                    outSurfaceData.normalTS = tangentNormal;
                    outSurfaceData.occlusion = sampleNormal.b;
    #else
    outSurfaceData.normalTS = half3(0, 0, 1);
    outSurfaceData.occlusion = 1;
    #endif
    outSurfaceData.emission = 0;
}

//  Billboard --------------------
#else
            inline void InitializeStandardLitSurfaceData(half colorVariation, float2 uv, out SurfaceDescriptionLeaves outSurfaceData)
            {
                half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
                outSurfaceData.alpha = Alpha(albedoAlpha.a, 1, _Cutoff);

            //  Add Color Variation
                albedoAlpha.rgb = lerp(albedoAlpha.rgb, (albedoAlpha.rgb + _HueVariation.rgb) * 0.5h, colorVariation * _HueVariation.a);
                outSurfaceData.albedo = albedoAlpha.rgb;
                outSurfaceData.metallic = 0;
                outSurfaceData.specular = _SpecColor;
                //outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
#if defined (_NORMALMAP)
                    half4 sampleNormal = SAMPLE_TEXTURE2D(_BumpSpecMap, sampler_BumpSpecMap, uv);
                    half3 tangentNormal;
                    
                    tangentNormal.xy = sampleNormal.ag * 2.0h - 1.0h;
                //  Scale!
                //  tangentNormal.xy *= _BumpScale;
                //  URP 11 needs reversed order here or goes crazy
                    tangentNormal.z = max(1.0e-16, sqrt(1.0 - saturate(dot(tangentNormal.xy, tangentNormal.xy))));
                    tangentNormal.xy *= _BumpScale;
                //  It will get normalized after TBN transformation.
                //  tangentNormal = normalize(tangentNormal); 
                    
                    outSurfaceData.normalTS = tangentNormal;
                    outSurfaceData.smoothness = sampleNormal.b * _Smoothness;
                    outSurfaceData.translucency = sampleNormal.r;
#else
                    outSurfaceData.normalTS = half3(0, 0, 1);
                    outSurfaceData.smoothness = _Smoothness;
                    outSurfaceData.translucency = 0;
#endif

                outSurfaceData.occlusion = albedoAlpha.a;
                outSurfaceData.emission = 0;
            }

#endif
#endif
#endif
