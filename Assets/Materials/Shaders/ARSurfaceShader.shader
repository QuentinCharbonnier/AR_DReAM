Shader "ARDReAM/SurfaceShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GridColor ("Grid Color", Color) = (1.0, 1.0, 0.0, 1.0)
        _PlaneNormal ("Plane Normal", Vector) = (0.0, 0.0, 0.0)
        _UvRotation ("UV Rotation", float) = 30
    }

    SubShader
    {
        Pass
        {

            // We add the LightMode at ForwardBase
            Tags { "Queue"="Transparent" "RenderType"="Transparent" "LightMode" = "ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest on
            ZWrite off // Turn on if you don't want to render a hole through the floor.
 
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Add multi_compile_fwdbase
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            // Reference the Unity library that includes all the lighting shadow macros
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                
                //The LIGHTING_COORDS macro (defined in AutoLight.cginc) defines the parameters needed to sample the shadow map
                LIGHTING_COORDS(0,1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GridColor;
            float3 _PlaneNormal;
            fixed _UvRotation;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);

                fixed cosr = cos(_UvRotation);
                fixed sinr = sin(_UvRotation);
                fixed2x2 uvrotation = fixed2x2(cosr, -sinr, sinr, cosr);

                // Construct two vectors that are orthogonal to the normal.
                // This arbitrary choice is not co-linear with either horizontal
                // or vertical plane normals.
                const float3 arbitrary = float3(1.0, 1.0, 0.0);
                float3 vec_u = normalize(cross(_PlaneNormal, arbitrary));
                float3 vec_v = normalize(cross(_PlaneNormal, vec_u));

                // Project vertices in world frame onto vec_u and vec_v.
                float2 plane_uv = float2(dot(v.vertex.xyz, vec_u), dot(v.vertex.xyz, vec_v));
                float2 uv = plane_uv * _MainTex_ST.xy;
                o.uv = mul(uvrotation, uv);
                o.color = v.color;
                
                // Transfer information from vertex shader to fragment shader
                TRANSFER_VERTEX_TO_FRAGMENT(o);  
                return o;
            }

            // Herit of COLOR, not SV_Target
            fixed4 frag(v2f i) : COLOR
            {
                // This considers whether this fragment is lit or in shadow.
                float attenuation = LIGHT_ATTENUATION(i);
                fixed4 col = tex2D(_MainTex, i.uv);
                return fixed4(_GridColor.rgb, col.r * i.color.a) * attenuation;
            }

            ENDCG
        }
    }
}