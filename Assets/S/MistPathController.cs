using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static PlayerState;

[RequireComponent(typeof(LineRenderer))]
public class MistPathController : MonoBehaviour
{
    [Header("基础设置")]
    public Transform targetTrans;       // 目标物体（比坐标更灵活）
    public Transform fromTrans;         // 起始物体
    public float pathHeight = 1.5f;     // 悬浮高度（建议比之前高一点）
    public float refreshRate = 0.2f;    // 寻路刷新频率

    [Header("迷雾形态 (关键)")]
    [Range(2, 50)]
    public int smoothness = 10;         // 平滑度（设高一点，因为我们要扭曲它）
    public float windingFrequency = 2f; // 蜿蜒的频率（数值越大，弯弯绕绕越多）
    public float windingAmplitude = 0.5f;// 蜿蜒的幅度（数值越大，偏离主路径越远）
    public float flowSpeed = 1.0f;      // 材质流动速度

    [Header("外观控制")]
    public float startWidth = 0.3f;     // 起始宽度（细）
    public float endWidth = 1.2f;       // 结束宽度（散开）

    private LineRenderer lineRenderer;
    private float timer;
    private NavMeshPath navPath;
    private Material mistMat;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        navPath = new NavMeshPath();

        // --- 1. 强制初始化 LineRenderer 的视觉属性 ---
        lineRenderer.useWorldSpace = true;
        lineRenderer.textureMode = LineTextureMode.Tile; // 必须是 Tile
        lineRenderer.alignment = LineAlignment.View;     // 始终面朝相机
        lineRenderer.numCapVertices = 5;                 // 端点圆滑
        lineRenderer.numCornerVertices = 5;              // 拐角圆滑

        // 设置宽度曲线：近处细实，远处宽虚
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0, startWidth);
        widthCurve.AddKey(1, endWidth);
        lineRenderer.widthCurve = widthCurve;

        // 设置颜色渐变：两头淡出（防止硬切断）
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.black, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.8f), new GradientAlphaKey(1.0f, 0.1f), new GradientAlphaKey(1.0f, 0.8f), new GradientAlphaKey(0.8f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;

        // 获取材质以便流动
        mistMat = lineRenderer.material;

        // 注册事件 (保留你原有的逻辑)
        if (PlayerState.Instance != null)
            PlayerState.Instance.EventOnDeepModeChange += OnPlayerSanityModeChange;

        OnPlayerSanityModeChange(ESanityState.Noemal);
    }

    private const float MinPointDistance = 0.5f;

    private void OnPlayerSanityModeChange(ESanityState state)
    {
        lineRenderer.enabled = (state == ESanityState.Deep);
    }

    private void OnDestroy()
    {
        if (PlayerState.Instance != null)
            PlayerState.Instance.EventOnDeepModeChange -= OnPlayerSanityModeChange;
    }

    void Update()
    {
        if (fromTrans == null || targetTrans == null) return;

        // --- 2. 材质纹理滚动 (制造流动感) ---
        if (lineRenderer.enabled && mistMat != null)
        {
            // 负数让雾气从目标流向玩家，或者反过来，看需求
            float offset = Time.time * -flowSpeed;
            mistMat.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        }

        // --- 3. 路径计算 ---
        timer += Time.deltaTime;
        if (timer >= refreshRate)
        {
            CalculateAndDrawPath();
            timer = 0;
        }
    }

    void CalculateAndDrawPath()
    {
        Vector3 startPos = fromTrans.position;
        Vector3 endPos = targetTrans.position;

        if (NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, navPath))
        {
            if (navPath.corners.Length < 2)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            // --- 优化步骤 A: 预处理路径点 ---
            List<Vector3> rawPoints = new List<Vector3>();
            Vector3 lastPoint = navPath.corners[0] + Vector3.up * pathHeight;
            rawPoints.Add(lastPoint);

            for (int i = 1; i < navPath.corners.Length; i++)
            {
                Vector3 currentPoint = navPath.corners[i] + Vector3.up * pathHeight;
                // 只有当新点和上一个点距离足够远时才添加，避免急转弯处点堆积
                if (Vector3.Distance(currentPoint, lastPoint) > MinPointDistance)
                {
                    rawPoints.Add(currentPoint);
                    lastPoint = currentPoint;
                }
            }

            // 如果过滤后点太少，就直接用原始的
            if (rawPoints.Count < 2) rawPoints = new List<Vector3>(navPath.corners);

            // --- 优化步骤 B: 生成路径 ---
            Vector3[] finalPoints = GenerateWindingPath(rawPoints, smoothness);

            lineRenderer.positionCount = finalPoints.Length;
            lineRenderer.SetPositions(finalPoints);
        }
    }

    // --- 核心修改：在平滑插值中注入噪声 ---
    private Vector3[] GenerateWindingPath(List<Vector3> points, int segments)
    {
        if (points.Count < 2) return points.ToArray();

        // 依然需要补点
        List<Vector3> controlPoints = new List<Vector3>(points);
        controlPoints.Insert(0, points[0] + (points[0] - points[1])); // 更好的起点延伸：向反方向延伸
        controlPoints.Add(points[points.Count - 1] + (points[points.Count - 1] - points[points.Count - 2])); // 更好的终点延伸

        List<Vector3> resultPath = new List<Vector3>();

        for (int i = 0; i < controlPoints.Count - 3; i++)
        {
            Vector3 p0 = controlPoints[i];
            Vector3 p1 = controlPoints[i + 1];
            Vector3 p2 = controlPoints[i + 2];
            Vector3 p3 = controlPoints[i + 3];

            // 限制 Alpha (向心参数)，这里用简单的标准插值，但加入张力控制
            // 或者直接限制插值点的生成

            for (int j = 0; j < segments; j++)
            {
                float t = j / (float)segments;

                // 使用标准 Catmull-Rom
                Vector3 basePos = GetCatmullRomPosition(t, p0, p1, p2, p3);

                // --- 优化步骤 C: 更加智能的噪波 ---
                // 获取当前路径的切线方向（前进方向）
                Vector3 tangent = (GetCatmullRomPosition(t + 0.01f, p0, p1, p2, p3) - basePos).normalized;
                // 计算右侧向量
                Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

                // 只在左右方向（Right）上施加噪波，绝不在前后方向（Tangent）施加
                // 这样能有效防止路径“倒退”打圈
                float noiseVal = Mathf.PerlinNoise(basePos.x * windingFrequency, basePos.z * windingFrequency);
                float offsetVal = (noiseVal * 2 - 1) * windingAmplitude;

                Vector3 offset = right * offsetVal;

                resultPath.Add(basePos + offset);
            }
        }

        resultPath.Add(points[points.Count - 1] + Vector3.up * pathHeight);
        return resultPath.ToArray();
    }

    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }
}
