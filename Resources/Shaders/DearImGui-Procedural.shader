Shader "DearImGui/Procedural"
{
    // shader for Universal render pipeline
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "PreviewType" = "Plane" }
        LOD 100

        Lighting Off
        Cull Off ZWrite On ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        
        Pass
        {
            PackageRequirements
            {
                "com.unity.render-pipelines.universal"
                "unity" : "2020.1"
            }
            Name "DEARIMGUI PROCEDURAL URP"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment ImGuiPassFrag
            #include "./PassesUniversal.hlsl"

            StructuredBuffer<ImVert> _Vertices;
            int _BaseVertex;

            Varyings vert(uint id : SV_VertexID)
            {
#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE)
                // BaseVertexLocation is not automatically added to SV_VertexID
                id += _BaseVertex;
#endif
                ImVert v = _Vertices[id];
                return ImGuiPassVertex(v);
            }
            ENDHLSL
        }
    }

    // shader for builtin rendering
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        LOD 100

        Lighting Off
        Cull Off ZWrite On ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "DEARIMGUI PROCEDURAL BUILTIN"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment ImGuiPassFrag
            #include "./PassesBuiltin.hlsl"

            StructuredBuffer<ImVert> _Vertices;
            int _BaseVertex;

            Varyings vert(uint id : SV_VertexID)
            {
#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE)
                // BaseVertexLocation is not automatically added to SV_VertexID
                id += _BaseVertex;
#endif
                ImVert v = _Vertices[id];
                return ImGuiPassVertex(v);
            }
            ENDCG
        }
    }

    // shader for HD render pipeline
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "HDRenderPipeline" "PreviewType" = "Plane" }
        LOD 100

        Lighting Off
        Cull Off ZWrite On ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        
        Pass
        {
            PackageRequirements
            {
                "com.unity.render-pipelines.high-definition"
                "unity" : "2020.1"
            }
            Name "DEARIMGUI HDRP"

            HLSLPROGRAM
            #pragma vertex ImGuiPassVertex
            #pragma fragment ImGuiPassFrag
            #include "./PassesHD.hlsl"
            ENDHLSL
        }
    }
}
