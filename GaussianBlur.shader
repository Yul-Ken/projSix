Shader "Game/GaussianBlur" {
    Properties {
        _Size ("Size", Range(0, 5)) = 16
        _MainTex ("", 2D) = "" {}
    }
	CGINCLUDE
	#include "UnityCG.cginc"
	
    float _Size;
	sampler2D _MainTex;
    float2 _MainTex_TexelSize;
    
    v2f_img vert_amendUV(appdata_img v){
        v2f_img o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = float2(v.texcoord.x,1 - v.texcoord.y);//修正貼圖上下翻轉問題，Graphics.Blit有時會自動修正（非必要）
		return o;
    }

    half4 frag( v2f_img i) : SV_Target {
        return tex2D(_MainTex,i.uv);
    }

    half4 frag_h( v2f_img i ) : SV_Target {
        half4 sum = half4(0,0,0,0);        
        //weight为权重，offest为偏移的像素
        #define PROSSES_V(weight,offest) weight * tex2D(_MainTex, float2(i.uv.x + _MainTex_TexelSize.x * offest *_Size , i.uv.y))// ***_TexelSize指 1/图片宽(高)，即每像素的宽(高)
        sum += PROSSES_V(0.05, -4.0);
        sum += PROSSES_V(0.09, -3.0);
        sum += PROSSES_V(0.12, -2.0);
        sum += PROSSES_V(0.15, -1.0);
        sum += PROSSES_V(0.18,  0.0);
        sum += PROSSES_V(0.15, +1.0);
        sum += PROSSES_V(0.12, +2.0);
        sum += PROSSES_V(0.09, +3.0);
        sum += PROSSES_V(0.05, +4.0);
        
        return sum;
    }

    half4 frag_v( v2f_img i ) : SV_Target {
        half4 sum = half4(0,0,0,0);
        #define PROSSES_H(weight,offest) weight * tex2D( _MainTex, float2(i.uv.x , i.uv.y + _MainTex_TexelSize.y * offest *_Size))
        sum += PROSSES_H(0.05, -4.0);
        sum += PROSSES_H(0.09, -3.0);
        sum += PROSSES_H(0.12, -2.0);
        sum += PROSSES_H(0.15, -1.0);
        sum += PROSSES_H(0.18,  0.0);
        sum += PROSSES_H(0.15, +1.0);
        sum += PROSSES_H(0.12, +2.0);
        sum += PROSSES_H(0.09, +3.0);
        sum += PROSSES_H(0.05, +4.0);
        
        return sum;
    }
	ENDCG 
    
    Subshader {
        Pass {
            Cull Off ZTest Always ZWrite Off

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_v
            ENDCG
        }
        Pass{
            Cull Off ZTest Always ZWrite Off

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_h
            ENDCG
        }
        Pass{
            Cull Off ZTest Always ZWrite Off

            CGPROGRAM
            #pragma vertex vert_amendUV
            #pragma fragment frag
            ENDCG
        }
    }
    Fallback off
}