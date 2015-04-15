// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LodManagerCubic.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright> 
// <summary>
//   A shader used to highlight objects in the scene with a defined color and strength
// </summary>
// --------------------------------------------------------------------------------------------------------------------

Shader "QuickLod/Editor/RimHighlight" 
{
	Properties 
	{
		_Color ("Color (RGB) Transparency (A)", Color) = (1,0.5,0,1)
		_Strength ("Rim Strength", Range(0,1)) = 0.5
		_Offset ("Offset", float) = 0.003
	}
  
	SubShader 
	{  
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		LOD 200
   
		Pass 
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
    
			struct appdata 
			{
				float4 pos : POSITION;
				float3 normal : NORMAL;
			};
    
			struct v2f 
			{
				float4 pos : POSITION;
				float2 rim : TEXCOORD0;
			};
    
			fixed4 _Color;
			fixed _Strength;
			float _Offset;
    
			v2f vert (appdata v) 
			{
				v2f o;

				v.pos.xyz += v.normal * _Offset;
				o.pos = mul (UNITY_MATRIX_MVP, v.pos);
				o.rim = float2(1.0 - saturate(dot (normalize(ObjSpaceViewDir(v.pos)), v.normal)), 0);
     
				return o;
			}
         
			fixed4 frag(v2f i) : COLOR 
			{
				fixed4 hitColor;				
				hitColor.a = _Color.a * pow (i.rim.x, _Strength);
				hitColor.rgb = _Color.rgb * hitColor.a; 
				
				return hitColor;
			}  

			ENDCG
		}
	}
}