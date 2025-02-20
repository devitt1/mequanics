Shader "Custom/test" 
{
	SubShader {
	    Pass 
	    {
			CGPROGRAM
			#pragma target 3.0
			#pragma exclude_renderers xbox360
			#pragma vertex vp
			#pragma fragment fp

			//////////////////////////////
			
			struct vert
			{
			    float4 vertex : POSITION0;
			    float4 normal : NORMAL;
			};
			
			struct v2f {
			    float4 posScreen : POSITION0;
			    float4 norView : TEXCOORD0;
			    float4 posView : TEXCOORD1;
			    float4 dirLight : TEXCOORD2;
			};

			struct frag
			{
			    float4 color0 : COLOR0;
			};
			
			//////////////////////////////
			
			v2f vp (vert IN)
			{
				float4 posLight = float4(0, 0, 10, 1);
//				posLight = mul(UNITY_MATRIX_MV, posLight);
				
			    v2f OUT;
			    OUT.posScreen = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.norView = float4(mul(UNITY_MATRIX_IT_MV, float4(IN.normal.xyz, 0)).xyz, 0);
				OUT.posView = mul(UNITY_MATRIX_MV, float4(IN.vertex.xyz, 1));				
			    OUT.dirLight = float4( posLight.xyz - OUT.posView.xyz, 0);
//			    OUT.dirLight = float4(0, 0, 10, 0);
			    return OUT;
			}
			
			//////////////////////////////
			
			frag fp (v2f IN)
			{
				float4 dirLight = float4(0, 0, 10, 0);
				
				frag OUT;
				float cos = dot(normalize(IN.dirLight.xyz).xyz, normalize(IN.norView.xyz).xyz);
	


				OUT.color0 = float4(float3(1,1,1) * cos, 1);
//				OUT.color0 = float4(IN.dirLight.xyz, 1);

			    return OUT;
			}
			
			ENDCG
	    }
	}
	
	Fallback "Diffuse"
} 
