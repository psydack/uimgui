Shader "DearImGui/Mesh"
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
                "com.unity.render-pipelines.universal" : "10.0"
                "unity" : "2020.1"
            }
            Name "DEARIMGUI URP"

            HLSLPROGRAM
            #pragma vertex ImGuiPassVertex
            #pragma fragment ImGuiPassFrag
            #include "./PassesUniversal.hlsl"
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
            Name "DEARIMGUI BUILTIN"

            CGPROGRAM
            #pragma vertex ImGuiPassVertex
            #pragma fragment ImGuiPassFrag
            #include "./PassesBuiltin.hlsl"
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
