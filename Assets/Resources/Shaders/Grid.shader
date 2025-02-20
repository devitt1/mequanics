Shader "Custom/Grid" 
{
	Properties {
		_PosFocus("Focus Position", Vector) = (0, 0, 0, 1)
//		_RadiusMaxGrid("Max Grid Radius", Float) = 20.0
	}
	SubShader {
        Tags {"Queue" = "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha 
		
	    Pass 
	    {
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vp
			#pragma fragment fp
			
			uniform float4 _PosFocus;
			uniform float _RadiusMaxGrid;
				
			//////////////////////////////
			
			struct vert
			{
			    float4 vertex : POSITION0;
			};
			
			struct v2f {
			    float4 posScreen : POSITION0;
			    float3 vecFocus : TEXCOORD0;
			};

			struct frag
			{
			    float4 color : COLOR;
			};
			
			//////////////////////////////
			
			v2f vp (vert IN)
			{
			    v2f OUT;
			    OUT.posScreen = mul(UNITY_MATRIX_MVP, IN.vertex);
				float4 posView = mul(UNITY_MATRIX_MV, IN.vertex);				
				OUT.vecFocus = (posView - _PosFocus).xyz;
			    return OUT;
			}
			
			//////////////////////////////
			
			frag fp (v2f IN)
			{
				frag OUT;
				float alphaCoeff = max(_RadiusMaxGrid - length(IN.vecFocus), 0)/_RadiusMaxGrid;
				OUT.color = float4(1, 1, 1, 0.3 * alphaCoeff);
			    return OUT;
			}
			
			ENDCG
	    }
	}
	
	Fallback "Diffuse"
} 
