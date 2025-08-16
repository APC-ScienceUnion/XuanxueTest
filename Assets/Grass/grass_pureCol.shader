/*
1. alpha clip for grass
2. shaking depends on z axis in model-space
*/
Shader "Unlit/grass_pureCol"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        _Speed("Speed", float) = 10
        _SwayMax("SwayMax", float) = 0.005
        _YOffset("YOffset",float) = 0.5
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

            v2f vert (appdata v)
            {
                v2f o;
                // world position
                float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
	            float x = sin(wpos.x + (_Time.x * _Speed)) *(v.vertex.y - _YOffset) * 5;// x axis movements
	            float z = sin(wpos.z + (_Time.x * _Speed)) *(v.vertex.y - _YOffset) * 5;// z axis movements
	            v.vertex.x += smoothstep( 0, v.vertex.y - _YOffset, 0.5) * x * _SwayMax;// apply the movement if the vertex's y above the YOffset
	            v.vertex.z += smoothstep( 0, v.vertex.y - _YOffset, 0.5) * z * _SwayMax;

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
