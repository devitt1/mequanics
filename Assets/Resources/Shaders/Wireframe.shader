Shader "Custom/Wireframe" 
{ 
	Properties {
	}
	SubShader 
	{ 
		Pass 
		{ 
			BindChannels { Bind "Color", color }
		
			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite Off 
			Cull Front 
			Fog 
			{ 
				Mode Off 
			} 
		} 
	} 
	
	FallBack "Transparent/Diffuse"
}