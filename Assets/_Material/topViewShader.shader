Shader "Unlit/topViewShader"
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 p:TEXCOORD0;
			};

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.p = v.vertex;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				const float miny = -1.0;
				const float maxy = 1.0;

				float ny = (i.p.y - miny) / (maxy - miny);
			
				// sample the texture
				fixed4 col =fixed4(ny, ny, ny, 1.0);

				return col;
			}
			ENDCG
		}
	}
}
