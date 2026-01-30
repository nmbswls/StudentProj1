using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricFogAndMist2;
using static PlayerState;

public class SceneFogProvider : MonoBehaviour
{

    public VolumetricFog[] Fogs;

    public float TargetDensity;
    private float currDensity;

    private void Start()
    {
        if (Fogs != null)
        {
            foreach (var fog in Fogs)
            {
                VolumetricFogProfile instanceProfile = Instantiate(fog.profile);

                // 2. 将副本赋值回去
                // 这一步之后，fogManager 用的就是你独享的 Profile 了
                fog.profile = instanceProfile;
            }
        }

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
            TargetDensity = 3.0f;
        }
        else
        {
            TargetDensity = 0f;
        }

        UpdateAllFogs();
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
        UpdateDensity();
    }

    private float velocity = 0f; // 必须定义在类成员变量里，不能在 Update 里定义
    public float smoothTime = 0.3f; // 多少秒到达目标
    private void UpdateDensity()
    {
        if(Mathf.Abs(TargetDensity - currDensity) < 1e-2)
        {
            currDensity = TargetDensity;
            UpdateAllFogs();
            return;
        }
        currDensity = Mathf.SmoothDamp(currDensity, TargetDensity, ref velocity, smoothTime);

        UpdateAllFogs();
    }


    public void UpdateAllFogs()
    {
        if(Fogs != null)
        {
            foreach(var fog in Fogs)
            {
                fog.profile.density = currDensity;
                fog.UpdateMaterialProperties();
            }
        }
    }

}
