/*
1. alpha clip for grass
2. shaking depends on z axis in model-space
*/
Shader "Unlit/grass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CutOff("CutOff", Range(0,1)) = 0.5
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CutOff;
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
	            v.vertex.x += step( v.vertex.y - _YOffset,1) * x * _SwayMax;// apply the movement if the vertex's y above the YOffset
	            v.vertex.z += step( v.vertex.y - _YOffset,1) * z * _SwayMax;

                o.vertex = mul(unity_MatrixMVP, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                clip(col.a - _CutOff);
                return col;
            }
            ENDCG
        }
    }
}
