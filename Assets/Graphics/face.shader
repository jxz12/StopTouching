Shader "Face"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
 
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite On
        ZTest Less
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Lambert approxview halfasview noforwardadd alpha:auto
 
        sampler2D _MainTex;
        sampler2D _MainTex2;
        sampler2D _MainTex3;
        sampler2D _MainTex4;
 
        struct Input
        {
            float2 uv_MainTex;
        };
 
        void surf (Input IN, inout SurfaceOutput o)
        {
            half4 c1 = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c1;
            o.Alpha = c1.a;
        }
        ENDCG
    }
}