Shader "Custom/VignetteMask"
{
    Properties
    {
        [Header(Base Config)]
        _Color ("Base Color", Color) = (0,0,0,1) // 遮罩底色(通常是黑)
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 1.5)) = 0.3
        _Softness ("Softness", Range(0.01, 1)) = 0.2

        [Header(Vein Effect)]
        _VeinTex ("Vein Texture", 2D) = "black" {} // 拖入血管贴图
        _VeinColor ("Vein Color", Color) = (0.8, 0, 0, 1) // 血管颜色(深红)
        _VeinPower ("Vein Intensity", Range(0, 5)) = 1.0 // 血管有多亮
        _Tiling ("Texture Tiling", Float) = 1.0 // 纹理重复次数
        _Distortion ("Distortion", Range(0, 0.1)) = 0.02 // 扭曲程度(模拟充血)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            float4 _Color;
            float2 _Center;
            float _Radius;
            float _Softness;

            sampler2D _VeinTex;
            float4 _VeinTex_ST;
            float4 _VeinColor;
            float _VeinPower;
            float _Tiling;
            float _Distortion;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 计算基础圆形的遮罩 (和之前一样)
                // 这里为了让血管边缘不那么圆润，我们加一点基于纹理的偏移
                float2 distUV = i.uv;
                
                // 采样纹理用于扭曲 (让圆圈看起来不规则，像是有机物)
                float noise = tex2D(_VeinTex, i.uv * 0.5 + _Time.x * 0.05).r; 
                float dist = distance(distUV + noise * _Distortion, _Center);

                float maskAlpha = smoothstep(_Radius, _Radius + _Softness, dist);

                // 2. 采样血管纹理
                // 技巧：让UV随时间极缓慢移动，模拟血液流动感(可选)
                // 1. 获取屏幕长宽比
				// _ScreenParams 是 Unity 内置变量
				// x 是宽度, y 是高度, z = 1 + 1/width, w = 1 + 1/height
				float aspect = _ScreenParams.x / _ScreenParams.y;

				// 2. 修正 UV
				// 我们保持 Y 轴不变，把 X 轴乘以长宽比
				// 这样 X 轴的 0~1 范围会变成 0~1.77 (对于 16:9 屏幕)
				float2 correctedUV = i.uv;
				correctedUV.x *= aspect; 

				// 3. 加上 Tiling (重复次数)
				float2 veinUV = correctedUV * _Tiling;

				// 4. 采样
				fixed4 veinSample = tex2D(_VeinTex, veinUV);

                // 3. 颜色混合
                // 如果 alpha 是 0 (中心)，完全显示场景
                // 如果 alpha 是 1 (边缘)，显示 黑色底 + 红色血管
                
                // 计算血管层的颜色：底色 lerp 到 血管色，根据纹理亮度决定
                float3 finalColor = lerp(_Color.rgb, _VeinColor.rgb, veinSample.r * _VeinPower);

                // 4. 应用透明度
                // 只有在 maskAlpha 大于 0 的地方才显示颜色
                return float4(finalColor, maskAlpha * _Color.a);
            }
            ENDCG
        }
    }
}