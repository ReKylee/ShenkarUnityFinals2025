// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'
// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.
// Upgrade NOTE: upgraded instancing buffer 'name' to new syntax.

Shader "TMPExtensions/BitmapShadow"
{
    Properties
    {
        _MainTex("Font Atlas", 2D) = "white" {}
        _FaceColor("Face Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (0,0,0,1)
        _ShadowOffset("Shadow Offset (px)", Vector) = (1,-1,0,0)
        _ShadowHorStrength("Horizontal Edge Intensity", Range(0,1)) = 1
        _ShadowVerStrength("Vertical Edge Intensity", Range(0,1)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _FaceColor;
            float4    _ShadowColor;
            float4    _ShadowOffset;       // x, y in pixels
            float4    _MainTex_TexelSize;  // declare texel size uniform

            float _ShadowHorStrength;
            float _ShadowVerStrength;

            struct app
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(app IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(IN.vertex);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                // Calculate atlas resolution
                float2 res = 1.0 / _MainTex_TexelSize.xy;

                // Snap original UVs to nearest pixel center
                float2 uv0 = floor(IN.uv * res + 0.5) / res;

                // Sample face alpha
                float faceAlpha = tex2D(_MainTex, uv0).a;

                // Detect edges on face (one-pixel neighbors)
                float2 one = 1.0 / res;
                float  alphaL = tex2D(_MainTex, uv0 - float2(one.x, 0)).a;
                float  alphaR = tex2D(_MainTex, uv0 + float2(one.x, 0)).a;
                float  alphaU = tex2D(_MainTex, uv0 + float2(0, one.y)).a;
                float  alphaD = tex2D(_MainTex, uv0 - float2(0, one.y)).a;
                float  verticalEdges = max(alphaL, alphaR);
                float  horizontalEdges = max(alphaU, alphaD);

                // Compute snapped face UV and constant UV offset in atlas space
                float2 faceUV = floor(IN.uv * res + 0.5) / res;
                float2 offsetUV = _ShadowOffset.xy * _MainTex_TexelSize.xy;
                // Apply offset to snapped UV for shadow sampling
                float2 uvHor = faceUV + float2(offsetUV.x, 0);
                float2 uvVer = faceUV + float2(0, offsetUV.y);

                // Sample shadow contributions (always offset from snapped faceUV)
                float shadowAlphaHor = verticalEdges * tex2D(_MainTex, uvHor).a * _ShadowHorStrength;
                float shadowAlphaVer = horizontalEdges * tex2D(_MainTex, uvVer).a * _ShadowVerStrength;
                float shadowAlpha = max(shadowAlphaHor, shadowAlphaVer);

                // Combine colors, drawing shadow behind face
                float3 col = _ShadowColor.rgb * shadowAlpha + _FaceColor.rgb * faceAlpha;
                float  alpha = max(shadowAlpha, faceAlpha);

                return float4(col, alpha);
            }
            ENDCG
        }
    }
}