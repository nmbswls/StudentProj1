using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] // 方便在编辑器里看效果
public class DualWorldController : MonoBehaviour
{
    public Material worldRevealMat; // 拖入刚才做的材质 Mat_WorldReveal
    public Camera mainCam;

    // 对应 Shader Graph 里的 Reference Name
    private int posID = Shader.PropertyToID("_PlayerScreenPos");
    private int ratioID = Shader.PropertyToID("_ScreenAspectRatio"); // 如果你的圆变椭圆了，需要修宽高比

    void Update()
    {
        if (worldRevealMat == null || mainCam == null) return;

        // 1. 获取主角在屏幕上的位置 (0-1 范围)
        Vector3 screenPos = mainCam.WorldToViewportPoint(transform.position);

        // 2. 传给 Shader
        worldRevealMat.SetVector(posID, new Vector2(screenPos.x, screenPos.y));

        // *可选：修复宽高比导致的椭圆问题
        // float aspect = (float)Screen.width / Screen.height;
        // worldRevealMat.SetFloat(ratioID, aspect);
        // (需要在 Shader 里把 UV.x 乘上 aspect 再算 Distance)
    }
}