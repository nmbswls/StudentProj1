using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisionMaskController : MonoBehaviour
{
    [Header("组件")]
    public Image maskImage;

    [Header("半径设置")]
    private float innerRaduis = 0.25f;
    private float outerRadius = 0.65f;

    [Header("血管效果")]
    public float minVeinPower = 0.5f; // 放松时血管暗淡
    public float maxVeinPower = 2.0f; // 紧张/遮挡时血管鲜红

    [Header("呼吸参数")]
    private float breathSpeed = 12.0f;
    private float breathAmplitude = 0.03f;

    [Header("平滑过渡")]
    private float smoothTime = 0.2f;

    private Material m_Mat;
    private float m_CurrentBaseRadius;
    private float m_Velocity;

    // Shader 属性 ID
    private int m_RadiusID;
    private int m_VeinPowerID;

    public float FocusRate;

    void Start()
    {
        m_Mat = Instantiate(maskImage.material);
        maskImage.material = m_Mat;
        m_CurrentBaseRadius = outerRadius;

        m_RadiusID = Shader.PropertyToID("_Radius");
        m_VeinPowerID = Shader.PropertyToID("_VeinPower");

        // 5. 设置 Shader
        m_Mat.SetFloat(m_RadiusID, outerRadius);
        m_Mat.SetFloat(m_VeinPowerID, Mathf.Max(0, 0)); // 保护一下不小于0
    }

    void Update()
    {
        if(PlayerState.Instance != null)
        {
            if(PlayerState.Instance.SanityState == PlayerState.ESanityState.Noemal)
            {
                FocusRate = 0;
            }
            else if (PlayerState.Instance.SanityState == PlayerState.ESanityState.Deep)
            {
                FocusRate = Mathf.Clamp((Time.time - PlayerState.Instance.ChangeStateTimer) / 3.0f, 0, 1);
            }
            else
            {
                FocusRate = 1;
            }
        }

        FadeFocusMode();
    }


    void FadeFocusMode()
    {
        // 1. 输入逻辑
        float targetRadius = Mathf.Lerp(outerRadius, innerRaduis, FocusRate);


        // 2. 平滑半径
        m_CurrentBaseRadius = Mathf.SmoothDamp(m_CurrentBaseRadius, targetRadius, ref m_Velocity, smoothTime);

        // 3. 呼吸计算
        float noise = 0; // -1 到 1
        if(FocusRate >= 1)
        {
            noise = Mathf.Sin(Time.time * breathSpeed);
        }
        float breathOffset = noise * breathAmplitude;
        float finalRadius = Mathf.Max(0, m_CurrentBaseRadius + breathOffset);

        // 4. 血管充血逻辑 (修正版)

        // t 代表 "视野张开的进度"
        // 0 = 视野最小 (最黑, minRadius)
        // 1 = 视野最大 (最清, maxRadius)
        float t = Mathf.InverseLerp(outerRadius, innerRaduis, m_CurrentBaseRadius);

        // 我们希望：
        // t = 0 时 (最黑) -> 血管最强 (maxVeinPower)
        // t = 1 时 (最亮) -> 血管最弱 (minVeinPower)

        // 方法：直接用 Lerp，但是明确参数位置
        // Lerp(当t=0时的值, 当t=1时的值, t)
        float baseVeinPower = Mathf.Lerp(minVeinPower, maxVeinPower, t);

        // 呼吸闪烁：
        // 当 noise 为正(呼吸吸气)时，血管稍微亮一点；呼气时暗一点
        // 这里的 0.2f 系数可以根据需要调整，确保不会减成负数
        float finalVeinPower = baseVeinPower - noise * 0.1f;

        // 5. 设置 Shader
        m_Mat.SetFloat(m_RadiusID, finalRadius);
        m_Mat.SetFloat(m_VeinPowerID, Mathf.Max(0, finalVeinPower)); // 保护一下不小于0
    }
}