

using Animancer;
using HighlightPlus;
using System.Collections.Generic;
using UnityEngine;
using static PlayerState;

public class SimpleDangerPivotObj : MonoBehaviour, ISceneInteractable
{
    public long Id => 0;

    public AnimancerComponent AnimancerComponent;

    public string ShowName => "Ãªµã";
    public HighlightEffect[] Effects;

    public Vector2 Pos => transform.position;

    public bool Used = false;

    private void Start()
    {
        PlayerState.Instance.EventOnDeepModeChange += OnPlayerSanityModeChange;
    }

    public void Update()
    {
        if(PlayerState.Instance != null)
        {
            if(PlayerState.Instance.SanityState == ESanityState.Danger)
            {
                
            }
            else
            {

            }
        }
    }
    private void OnDestroy()
    {
        PlayerState.Instance.EventOnDeepModeChange -= OnPlayerSanityModeChange;
    }

    private void OnPlayerSanityModeChange(ESanityState state)
    {
        if (state == ESanityState.Danger)
        {
            Used = false;
        }

        if (Effects != null)
        {
            if (PlayerState.Instance.SanityState == ESanityState.Danger)
            {
                foreach (var effect in Effects)
                {
                    effect.highlighted = true;
                }
            }
            else
            {
                foreach (var effect in Effects)
                {
                    effect.highlighted = false;
                }
            }
        }

        

            
    }

    public bool CanInteractEnable(float dist)
    {
        if(Used)
        {
            return false;
        }
        if(PlayerState.Instance == null)
        {
            return false;
        }

        if(PlayerState.Instance.SanityState != PlayerState.ESanityState.Danger)
        {
            return false;
        }

        return true;
    }

    public Vector3 GetHintAnchorPosition()
    {
        return transform.position + Vector3.up * 0.2f;
    }

    public List<SceneInteractSelection> GetInteractSelections(float dist)
    {
        List<SceneInteractSelection> ret = new();
        ret.Add(new SceneInteractSelection()
        {
            SelectId = 1,
            SelectContent = "»¥¶¯",
            Selectable = true,
        });
        return ret;
    }

    public bool IsAutoInteract()
    {
        return false;
    }

    public void TriggerInteract(int selectionId)
    {
        
        if(Used)
        {
            return;
        }

        Used = true;
        PlayerState.Instance.AddCLearTimes();
    }
}