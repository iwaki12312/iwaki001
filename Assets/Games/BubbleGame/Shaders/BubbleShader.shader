Shader "Custom/BubbleShader"
{
    Properties
    {
        [NoScaleOffset] [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        _HighlightIntensity ("Highlight Intensity", Range(0,2)) = 1.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { 
            "Queue"="Transparent+100" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "ForceNoShadowCasting"="True"
        }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        CGPROGRAM
        #pragma surface surf Standard alpha:premul
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _HighlightColor;
        float _HighlightIntensity;
        float _Cutoff;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            
            // 光沢効果
            float rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = _HighlightColor.rgb * pow(rim, 2) * _HighlightIntensity;
            o.Alpha = c.a;
            clip(o.Alpha - _Cutoff);
        }
        ENDCG
    }
    FallBack "Sprites/Default"
}
