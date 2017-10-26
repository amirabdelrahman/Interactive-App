Shader "Struct/structShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float2 uv3 : TEXCOORD2;
				float2 uv4 : TEXCOORD3;
			};

			struct v2f
			{
				float3 p:TEXCOORD0;
				float3 n:TEXCOORD1;


				float  vonMises : TEXCOORD3;
				float2 ps : TEXCOORD4;
				float3 def : TEXCOORD5;

				float4 vertex : SV_POSITION;
			};

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
			
				o.vonMises = v.uv.x;
				o.ps = v.uv2;
				o.def = float3(v.uv3.y, v.uv4.x, v.uv4.y);
				o.p = v.vertex;
				o.n = v.normal;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float3 p = i.p;
				float3 n = normalize(i.n);
				// sample the texture
				float4 col = float4(n.x, n.y, sin(p.y*200.0),1.0);

				float vms = i.vonMises;
				float r = sin(vms *  3.1415926  * 0.5);
				float g = 0.4 + sin(vms *  3.1415926);
				float b = 0.5 + cos(vms * 3.1415926  * 0.5);

				col= float4(r, g, b, 1.0);

				return col;
			}
			ENDCG
		}
	}
}
