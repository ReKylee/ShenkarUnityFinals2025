Shader "TextMeshPro/Distance Field (Surface)"
{

    Properties
    {
        _FaceTex ("Fill Texture", 2D) = "white" {}
        _FaceUVSpeedX ("Face UV Speed X", Range(-5, 5)) = 0.0
        _FaceUVSpeedY ("Face UV Speed Y", Range(-5, 5)) = 0.0
        _FaceColor ("Fill Color", Color) = (1,1,1,1)
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineTex ("Outline Texture", 2D) = "white" {}
        _OutlineUVSpeedX ("Outline UV Speed X", Range(-5, 5)) = 0.0
        _OutlineUVSpeedY ("Outline UV Speed Y", Range(-5, 5)) = 0.0
        _OutlineWidth ("Outline Thickness", Range(0, 1)) = 0
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0

        _Bevel ("Bevel", Range(0,1)) = 0.5
        _BevelOffset ("Bevel Offset", Range(-0.5,0.5)) = 0
        _BevelWidth ("Bevel Width", Range(-.5,0.5)) = 0
        _BevelClamp ("Bevel Clamp", Range(0,1)) = 0
        _BevelRoundness ("Bevel Roundness", Range(0,1)) = 0

        _BumpMap ("Normalmap", 2D) = "bump" {}
        _BumpOutline ("Bump Outline", Range(0,1)) = 0.5
        _BumpFace ("Bump Face", Range(0,1)) = 0.5

        _ReflectFaceColor ("Face Color", Color) = (0,0,0,1)
        _ReflectOutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Cube ("Reflection Cubemap", Cube) = "black" { /* TexGen CubeReflect */ }
        _EnvMatrixRotation ("Texture Rotation", vector) = (0, 0, 0, 0)
        _SpecColor ("Specular Color", Color) = (0,0,0,1)

        _FaceShininess ("Face Shininess", Range(0,1)) = 0
        _OutlineShininess ("Outline Shininess", Range(0,1)) = 0

        _GlowColor ("Color", Color) = (0, 1, 0, 0.5)
        _GlowOffset ("Offset", Range(-1,1)) = 0
        _GlowInner ("Inner", Range(0,1)) = 0.05
        _GlowOuter ("Outer", Range(0,1)) = 0.05
        _GlowPower ("Falloff", Range(1, 0)) = 0.75

        _WeightNormal ("Weight Normal", float) = 0
        _WeightBold ("Weight Bold", float) = 0.5

        // Should not be directly exposed to the user
        _ShaderFlags ("Flags", float) = 0
        _ScaleRatioA ("Scale RatioA", float) = 1
        _ScaleRatioB ("Scale RatioB", float) = 1
        _ScaleRatioC ("Scale RatioC", float) = 1

        _MainTex ("Font Atlas", 2D) = "white" {}
        _TextureWidth ("Texture Width", float) = 512
        _TextureHeight ("Texture Height", float) = 512
        _GradientScale ("Gradient Scale", float) = 5.0
        _ScaleX ("Scale X", float) = 1.0
        _ScaleY ("Scale Y", float) = 1.0
        _PerspectiveFilter ("Perspective Correction", Range(0, 1)) = 0.875
        _Sharpness ("Sharpness", Range(-1,1)) = 0

        _VertexOffsetX ("Vertex OffsetX", float) = 0
        _VertexOffsetY ("Vertex OffsetY", float) = 0

        _CullMode ("Cull Mode", Float) = 0
        //_MaskCoord		("Mask Coords", vector) = (0,0,0,0)
        //_MaskSoftness		("Mask Softness", float) = 0
    }

    SubShader
    {

        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }

        LOD 300
        Cull [_CullMode]

        CGPROGRAM
        #pragma surface PixShader BlinnPhong alpha:blend vertex:VertShader nolightmap nodirlightmap
        #pragma target 3.0
        #pragma shader_feature __ GLOW_ON
        #pragma glsl

        #include "TMPro_Properties.cginc"
        #include "TMPro.cginc"

        half _FaceShininess;
        half _OutlineShininess;

        struct Input
        {
            fixed4 color : COLOR;
            float2 uv_MainTex;
            float2 uv2_FaceTex;
            float2 uv2_OutlineTex;
            float2 param;						// Weight, Scale
            float3 viewDirEnv;
        };


        #define BEVEL_ON 1
        #include "TMPro_Surface.cginc"
        ENDCG

        // Pass to render object as a shadow caster
        Pass
        {
            Name "Caster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            Offset 1, 1

            Fog
            {
                Mode Off
            }
            ZWrite On
            ZTest LEqual
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
                float2 uv2 : TEXCOORD3;
                float  alphaClip : TEXCOORD2;
            };

            uniform float4 _MainTex_ST;
            uniform float4 _OutlineTex_ST;
            float          _OutlineWidth;
            float          _FaceDilate;
            float          _ScaleRatioA;

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER(o)
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.texcoord, _OutlineTex);
                o.alphaClip = (1.0 - _OutlineWidth * _ScaleRatioA - _FaceDilate * _ScaleRatioA) / 2;
                return o;
            }

            uniform sampler2D _MainTex;

            float4 frag(v2f i) : COLOR
            {
                fixed4 texcol = tex2D(_MainTex, i.uv).a;
                clip(texcol.a - i.alphaClip);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUI"
}