Shader "Custom/mask" 
{
	Properties {
		_ColMasked("Masked Color", Color) = (0, 0, 0, 0.5)
		_ColUnmasked("Unmasked Color", Color) = (0, 0, 0, 0.0)
		_posCenter("Pos Center", Vector) = (0, 0, 0, 1)
//		_posTest("Pos Test", Vector) = (960, 600, 0, 1)
		_fRadius("Radius", float) = 1.0
//		_fRadiusTest("Radius Test", float) = 100.0
	}
	SubShader {
        Tags {"Queue" = "Transparent"}
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha 
	    Pass 
	    {
			CGPROGRAM
			#pragma target 3.0
			#pragma exclude_renderers xbox360
			#pragma vertex vp
			#pragma fragment fp
			
			
			uniform float4 _ColMasked;
			uniform float4 _ColUnmasked;
			
			uniform float4 _posCenter;
//			uniform float4 _posTest;
			uniform float _fRadius;
//			uniform float _fRadiusTest;
				
			//////////////////////////////
			
			struct vert
			{
			    float4 vertex : POSITION0;
			};
			
			struct v2f {
			    float4 posScreen : POSITION0;
			    float4 posView : TEXCOORD0;	
			    float4 posScreenC : TEXCOORD1;		
			};

			struct frag
			{
			    float4 color0 : COLOR0;
			};
			
			//////////////////////////////
			
			v2f vp (vert IN)
			{
			    v2f OUT;
			    OUT.posScreen = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.posScreenC = OUT.posScreen;
			    return OUT;
			}
			
			//////////////////////////////
			
			frag fp (v2f IN)
			{
				frag OUT;
				float4 posPixel = IN.posScreenC;
//				posPixel.x = (posPixel.x);
//				posPixel.y = (posPixel.y);
//				float4 posPixel = (IN.posScreenC + float4(1, 1, 0, 0)) * float4(0.5, 0.5, 1, 1) * float4(960, 600, 1, 1);
//				posPixel = (posPixel + float4(2, 2, 0, 0)) * float4(0.25, 0.25, 1, 1) * float4(960, 600, 1, 1);
				posPixel = (posPixel + float4(2, 2, 0, 0)) * float4(0.25, 0.25, 1, 1) * _ScreenParams;
//				float4 posPixel = IN.posScreenC * float4(960, 600, 1, 1);
//				
				float4 diff = posPixel - _posCenter; 
//				float4 diff = posPixel - _posTest; 
				if (_fRadius * _fRadius > dot(diff.xy, diff.xy))
//				if (_fRadiusTest * _fRadiusTest > dot(diff.xy, diff.xy))
				{
					OUT.color0 = float4(0,0,0,0);
				} else {
					OUT.color0 = _ColMasked;
				}
				return OUT;
			}
			
			ENDCG
	    }
	}
	
	Fallback "Diffuse"
} 
