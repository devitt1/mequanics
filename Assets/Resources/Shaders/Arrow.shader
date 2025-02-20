Shader "Custom/Arrow" 
{
	Properties {
		_ColorMain("Main Color", Color) = (1, 1, 1, 0.0)
	}
	SubShader {
		Tags {"Queue" = "Transparent"}
		
		ZTest Always
		ZWrite false
		Blend SrcAlpha OneMinusSrcAlpha 
		
	    Pass 
	    {
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vp
			#pragma fragment fp
			
			uniform float4 _ColorMain;
				
			//////////////////////////////
			
			struct vert
			{
			    float4 vertex : POSITION0;
			    float4 normal : NORMAL;
			};
			
			struct v2f {
			    float4 posScreen : POSITION0;
			    float3 normalView : TEXCOORD0;
			    float3 dirLight : TEXCOORD1;
			};

			struct frag
			{
			    float4 color : COLOR;
			};
			
			//////////////////////////////
			
			v2f vp (vert IN)
			{
				float4 posLight = float4(0, 0, 10, 1);
				
			    v2f OUT;
			    OUT.posScreen = mul(UNITY_MATRIX_MVP, IN.vertex);
			    OUT.normalView = mul(UNITY_MATRIX_MV, float4(IN.normal.xyz, 0)).xyz;
			    float4 posView = mul(UNITY_MATRIX_MV, IN.vertex);
			    OUT.dirLight = (posLight.xyz - posView.xyz).xyz;
			    return OUT;
			}
			
			//////////////////////////////
			
			frag fp (v2f IN)
			{
				frag OUT;
				float fAmb = 0.2;
				float fDiff = 0.8;
				float diffCoef = max(dot(normalize(IN.normalView), normalize(IN.dirLight)), 0);
	
				OUT.color = _ColorMain * (fAmb + fDiff * diffCoef);
				
			    return OUT;
			}
			
			ENDCG
	    }
	}
	
	Fallback "Transparent/Diffuse"
} 
