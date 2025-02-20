Shader "Custom/Ghost" 
{
	Properties {
	}
	SubShader {
        Tags {"Queue" = "Transparent"}
		
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha 
		
	    Pass 
	    {
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vp
			#pragma fragment fp
			
			uniform float4 _ColorBlock;
				
			//////////////////////////////
			
			struct vert
			{
			    float4 vertex : POSITION0;
			    float4 normal : NORMAL;
			};
			
			struct v2f {
			    float4 posScreen : POSITION0;
			    float3 norView : TEXCOORD0;
			    float4 posView : TEXCOORD1;
			    float3 dirLight : TEXCOORD2;
			};

			struct frag
			{
			    float4 color0 : COLOR0;
			};
			
			//////////////////////////////
			
			v2f vp (vert IN)
			{
				float4 posLight = float4(0, 0, 10, 1);
				
			    v2f OUT;
			    OUT.posScreen = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.norView = mul(UNITY_MATRIX_IT_MV, float4(IN.normal.xyz, 0)).xyz;
				OUT.posView = mul(UNITY_MATRIX_MV, IN.vertex);				
			    OUT.dirLight = (posLight.xyz - OUT.posView.xyz).xyz;
			    return OUT;
			}
			
			//////////////////////////////
			
			frag fp (v2f IN)
			{
				frag OUT;
				float3 colAmbient = _ColorBlock.rgb * 0.0;
				float3 colDiffuse = _ColorBlock.rgb * 1.0;
				
				float cos = max(dot(normalize(IN.dirLight).xyz, normalize(IN.norView).xyz), 0);
				colDiffuse = colDiffuse * cos;

				float4 colLit = float4(colAmbient + colDiffuse, _ColorBlock.a);
			
				OUT.color0 = colLit;
			    return OUT;
			}
			
			ENDCG
	    }
	}
	
	Fallback "Diffuse"
} 
