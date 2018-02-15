
Shader "Custom/Chaperone" {
    Properties{
        _MainColor("Color", Color) = (1,1,1)
        _MainTex("Texture", 2D) = "white" {}
        _BumpMap("Bumpmap", 2D) = "bump" {}
        _PlayerPos("Player Position", Vector) = (0.0, 0.0, 0.0, 1.0)
		_Dist("Distance", Float) = 5.0
    }
    SubShader{
        Tags{ "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert
       struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float4 color : COLOR;
            float3 worldPos;
        };
        float4 _MainColor;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        float4 _PlayerPos;
		float _Dist;
       void surf(Input IN, inout SurfaceOutput o) {
            float4 c = tex2D(_MainTex, IN.uv_MainTex) * _MainColor;
            float dist = distance(IN.worldPos, _PlayerPos);
           if (dist < _Dist) {
                o.Albedo = c.rgb;
                o.Alpha = c.a;
                o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            }
            else {
                clip(-1);
            }
       }
        ENDCG
    }
    Fallback "Diffuse"
}