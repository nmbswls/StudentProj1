using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricFogAndMist2;
using static PlayerState;

public class SceneFireEffectProvider : MonoBehaviour
{

    public Material targetMaterial;
    public float TargetBurnProgress = 0;
    private float currBurnProgress = 0;

    private void Start()
    {
        

        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.EventOnDeepModeChange += OnPlayerSanityModeChange;
        }
        //OnPlayerSanityModeChange(ESanityState.Noemal);
    }


    private void OnPlayerSanityModeChange(ESanityState state)
    {
        if(state == ESanityState.Noemal)
        {
            TargetBurnProgress = 0;
        }
        else
        {
            TargetBurnProgress = 1f;
        }

        UpdateView();
    }


    private void OnDestroy()
    {
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.EventOnDeepModeChange -= OnPlayerSanityModeChange;
        }
    }

    private void Update()
    {
        UpdateBurnProgress();
    }

    private float velocity = 0f; // 必须定义在类成员变量里，不能在 Update 里定义
    private float smoothTime = 0.25f; // 多少秒到达目标
    private void UpdateBurnProgress()
    {
        if(Mathf.Abs(TargetBurnProgress - currBurnProgress) < 1e-2)
        {
            currBurnProgress = TargetBurnProgress;
            UpdateView();
            return;
        }
        currBurnProgress = Mathf.SmoothDamp(currBurnProgress, TargetBurnProgress, ref velocity, smoothTime);

        UpdateView();
    }


    public void UpdateView()
    {
        targetMaterial.SetFloat("_BurnProgress", currBurnProgress);
    }

}
