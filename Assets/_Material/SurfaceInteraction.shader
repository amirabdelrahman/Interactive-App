Shader "Custom/SurfaceInteraction" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Contours("Contours", Range(0,1)) = 0.5
		_MinY("Minimum Y", Float) = -2.0
		_MaxY("Maximum Y", Float) = 2.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0


		struct Input {
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float _Contours;
		float _MinY;
		float _MaxY;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {

			

			float y = IN.worldPos.y;
			float dy = cos(_Contours*y*100.0);

			if (dy < -0.5) discard;

			float ny = (y - _MinY) / (_MaxY - _MinY);

			float2 p2 = IN.worldPos.xz;

			ny*sin(p2.x*200.0)*sin(p2.y*200.0);

			// Albedo comes from a texture tinted by color
			fixed4 c =  _Color*ny;
			o.Albedo =  c.rgb;

			o.Emission = float3(ny,ny,ny)*0.5;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
