Shader "Custom/DiffuseCollPoints" 
{
	Properties {
		// solid pass
		_ColorBlock("Block Color", Color) = (1, 1, 1, 1)
		_ColCollision ("Collision Color", Color) = (1, 0, 0, 1) 
		
		// wireframe pass
	    _LineColor ("Line Color", Color) = (1,0,0,1)
	    _LineWidth ("Line Width", float) = 0.0
	}
	SubShader {
	    Pass 
	    {
			CGPROGRAM
			#pragma target 3.0
			#pragma exclude_renderers xbox360
			#pragma vertex vp
			#pragma fragment fp
			
			uniform float4 _ColCollision;
			uniform float4 _ColorBlock;
			uniform fixed _nCollPoints;
			uniform float _fMoveOffset;
			
			uniform float4 _posColl0;
			uniform float4 _posColl1;
			uniform float4 _posColl2;
			uniform float4 _posColl3;
			uniform float4 _posColl4;
			uniform float4 _posColl5;
			uniform float4 _posColl6;
			uniform float4 _posColl7;
			uniform float4 _posColl8;
			uniform float4 _posColl9;
			
		    uniform float4 _LineColor;
//		    uniform float4 _GridColor;
		    uniform float _LineWidth;
				
			//////////////////////////////
			
			struct vert
			{
			    float4 vertex : POSITION0;
			    float4 normal : NORMAL;
//			    float4 texcoord1 : TEXCOORD0;
//			    float4 color : COLOR;
			};
			
			struct v2f {
			    float4 posScreen : POSITION0;
			    float3 norView : TEXCOORD0;
			    float4 posView : TEXCOORD1;
			    float3 dirLight : TEXCOORD2;
			    
//			    float4 texcoord1 : TEXCOORD3;
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
			    
//			    OUT.texcoord1 = IN.texcoord1;
			    return OUT;
			}
			
			//////////////////////////////
			
			frag fp (v2f IN)
			{
				int nCollPoints = (int)_nCollPoints;
				
//				float3 dirLight = float3(0, 0, 10);
				
				frag OUT;
				float3 colAmbient = _ColorBlock.rgb * 0.0;
				float3 colDiffuse = _ColorBlock.rgb * 1.0;
				
				float cos = max(dot(normalize(IN.dirLight).xyz, normalize(IN.norView).xyz), 0);
				colDiffuse = colDiffuse * cos;

				float4 colLit = float4(colAmbient + colDiffuse, _ColorBlock.a);
				float4 posCollView;
				// collision visualization
				float blendFactor = 1;
				if (0 < nCollPoints){
					posCollView = _posColl0;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				if (1 < nCollPoints){
					posCollView = _posColl1;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				if (2 < nCollPoints){
					posCollView = _posColl2;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				if (3 < nCollPoints){
					posCollView = _posColl3;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				if (4 < nCollPoints){
					posCollView = _posColl4;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				if (5 < nCollPoints){
					posCollView = _posColl5;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				if (6 < nCollPoints){
					posCollView = _posColl6;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}	
				if (7 < nCollPoints){
					posCollView = _posColl7;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				if (8 < nCollPoints){
					posCollView = _posColl8;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				if (9 < nCollPoints){
					posCollView = _posColl9;
					blendFactor = min(distance(IN.posView.xyz, posCollView.xyz)+(1-2*_fMoveOffset), blendFactor);
				}
				
				OUT.color0 = lerp(float4(1,0,0,1), colLit, float4(blendFactor, blendFactor, blendFactor, blendFactor));
				
//			    float lx = step(_LineWidth, IN.texcoord1.x);
//			    float ly = step(_LineWidth, IN.texcoord1.y);
//			    float hx = step(IN.texcoord1.x, 1.0 - _LineWidth);
//			    float hy = step(IN.texcoord1.y, 1.0 - _LineWidth);
//			     
//			    OUT.color0 = lerp(_LineColor, OUT.color0, lx*ly*hx*hy);
			    
			    return OUT;
			}
			
			ENDCG
	    }
	}
	
	Fallback "Diffuse"
} 
