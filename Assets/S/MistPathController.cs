using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static PlayerState;

[RequireComponent(typeof(LineRenderer))]
public class MistPathController : MonoBehaviour
{
    [Header("设置")]
    public Vector3 targetPos;            // 目标点
    public float pathHeight = 1.0f;     // 雾气悬浮高度
    public float refreshRate = 0.5f;    // 路径刷新频率（秒）

    [Header("平滑度设置")]
    [Range(2, 20)]
    public int smoothness = 6;          // 每两个路点之间插入多少个平滑点（越大越圆滑，性能开销越大）
    public float tension = 0.5f;        // 曲线张力（0.5 是 Catmull-Rom 标准）

    private LineRenderer lineRenderer;
    public Transform FromTrans;
    private float timer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // 优化 LineRenderer 设置，使其适合雾气效果
        lineRenderer.useWorldSpace = true;
        lineRenderer.textureMode = LineTextureMode.Tile; // 关键！设为 Tile 才能让材质贴图流动

        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.EventOnDeepModeChange += OnPlayerSanityModeChange;
        }
        OnPlayerSanityModeChange(ESanityState.Noemal);
    }

    private void OnPlayerSanityModeChange(ESanityState state)
    {
        if (state == ESanityState.Deep)
        {
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }


    private void OnDestroy()
    {
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.EventOnDeepModeChange -= OnPlayerSanityModeChange;
        }
    }

    void Update()
    {
        if (FromTrans == null)
        {
            //lineRenderer.enabled = false;
            return;
        }

        //lineRenderer.enabled = true;
        timer += Time.deltaTime;
        if (timer >= refreshRate)
        {
            DrawPath();
            timer = 0;
        }
    }

    void DrawPath()
    {
        NavMeshPath path = new NavMeshPath();

        // 计算路径
        // 如果物体本身有 NavMeshAgent，用 agent.nextPosition 会比 transform.position 更贴合网格
        Vector3 startPos = FromTrans.position;

        if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path))
        {
            if (path.corners.Length < 2)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            // 1. 获取原始路径点并加上高度
            List<Vector3> rawPoints = new List<Vector3>(path.corners);
            for (int i = 0; i < rawPoints.Count; i++)
            {
                // 抬高 Y 轴，让雾气不要贴地，防止 Z-Fighting
                rawPoints[i] += Vector3.up * pathHeight;
            }

            // 2. 生成平滑的曲线点
            Vector3[] smoothPoints = GenerateSmoothPath(rawPoints, smoothness);

            // 3. 赋值给 Line Renderer
            lineRenderer.positionCount = smoothPoints.Length;
            lineRenderer.SetPositions(smoothPoints);
        }
    }

    // --- Catmull-Rom 样条曲线算法 ---
    // 这个算法能让曲线平滑地穿过每一个控制点，非常适合路径引导
    private Vector3[] GenerateSmoothPath(List<Vector3> points, int segments)
    {
        if (points.Count < 2) return points.ToArray();

        // Catmull-Rom 算法需要首尾各补一个辅助点
        // 我们简单地重复第一个点和最后一个点
        List<Vector3> controlPoints = new List<Vector3>(points);
        controlPoints.Insert(0, points[0]);
        controlPoints.Add(points[points.Count - 1]);

        List<Vector3> finalPath = new List<Vector3>();

        // 遍历所有线段
        for (int i = 0; i < controlPoints.Count - 3; i++)
        {
            Vector3 p0 = controlPoints[i];
            Vector3 p1 = controlPoints[i + 1];
            Vector3 p2 = controlPoints[i + 2];
            Vector3 p3 = controlPoints[i + 3];

            // 在 P1 和 P2 之间进行细分插值
            for (int j = 0; j < segments; j++)
            {
                float t = j / (float)segments;
                finalPath.Add(GetCatmullRomPosition(t, p0, p1, p2, p3));
            }
        }

        // 别忘了加上终点
        finalPath.Add(points[points.Count - 1]);

        return finalPath.ToArray();
    }

    // 标准 Catmull-Rom 插值公式
    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }
}
