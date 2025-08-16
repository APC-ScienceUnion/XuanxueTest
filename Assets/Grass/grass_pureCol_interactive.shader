/*
1. alpha clip for grass
2. shaking depends on z axis in model-space
*/
Shader "Unlit/grass_pureCol_interactive"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        _Speed("Speed", float) = 10
        _SwayMax("SwayMax", float) = 0.002
        _YOffset("YOffset",float) = 0
        
        _Radius("Interactive Radius", float) = 0.2
        _MaxWidth("Interactive Strength", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Speed;
            float _SwayMax;
            float _YOffset;
            float _Radius;
            float _MaxWidth;

            // postion array from the script
            uniform float3 _Positions[100];
            uniform float _PositionArray;

            v2f vert (appdata v)
            {
                v2f o;
                // world position
                float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
	            float x = sin(wpos.x + (_Time.x * _Speed)) *(v.vertex.y - _YOffset) * 5;// x axis movements
	            float z = sin(wpos.z + (_Time.x * _Speed)) *(v.vertex.y - _YOffset) * 5;// z axis movements
	            v.vertex.x += smoothstep( 0, v.vertex.y - _YOffset, 0.5) * x * _SwayMax;// apply the movement if the vertex's y above the YOffset
	            v.vertex.z += smoothstep( 0, v.vertex.y - _YOffset, 0.5) * z * _SwayMax;

                for(int i = 0; i < _PositionArray; i++)
                {
                    float3 dis = distance(_Positions[i], wpos); // distance for radius
                    float3 radius = 1 - saturate(dis /_Radius); // in world radius based on objects interaction radius
                    float3 sphereDisp = wpos - _Positions[i]; // position comparison
                    sphereDisp *= radius; // position multiplied by radius for falloff
                    v.vertex.xz += clamp(sphereDisp.xz * step(_YOffset, v.vertex.y), -_MaxWidth,_MaxWidth);// vertex movement based on falloff and clamped
                }

                o.vertex = mul(unity_MatrixMVP, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;

                return col;
            }
            ENDCG
        }
    }
}
