Shader "Hide/GrabDepth"
{
	Properties {
		_MainTex ("Main", 2D) = "white" {}
	}

	SubShader 
	{
		Tags{ "RenderType" = "GrabAlpha"  }

		Pass 
		{
			ColorMask 0
			ZWrite On 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 color = tex2D(_MainTex,i.uv);
				if (color.a <= 0)
					discard;
				return color;
			}
			ENDCG
		}
	}

	SubShader 
	{
		Tags{ "RenderType" = "Opaque"  }

		Pass 
		{
			ColorMask 0
			ZWrite On 
		}
	}
	FallBack Off
}