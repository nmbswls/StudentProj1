

using Animancer;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDoorController : MonoBehaviour, ISceneInteractable
{
    public long Id => 0;

    public Transform HandleObj;
    public bool IsOpen;
    public AnimancerComponent AnimancerComponent;

    public bool IsSwitching;
    public AnimationClip openClip;
    public AnimationClip closeClip;

    public string ShowName => "门";

    public Vector2 Pos => transform.position;

    public void Update()
    {
        if(IsSwitching)
        {
            if(AnimancerComponent == null)
            {
                IsSwitching = false;
                return;
            }
        }
    }

    public bool CanInteractEnable(float dist)
    {
        if(IsSwitching)
        {
            return false;
        }
        return true;
    }

    public Vector3 GetHintAnchorPosition()
    {
        return HandleObj.transform.position + Vector3.up * 0.1f;
    }

    public List<SceneInteractSelection> GetInteractSelections(float dist)
    {
        List<SceneInteractSelection> ret = new();
        ret.Add(new SceneInteractSelection()
        {
            SelectId = 1,
            SelectContent = IsOpen ? "关门" : "开门",
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
        if (IsSwitching)
        {
            return;
        }

        

        if(IsOpen)
        {
            IsOpen = false;
            if (AnimancerComponent != null && closeClip != null)
            {
                IsSwitching = true;
                var state = AnimancerComponent.Play(closeClip, 0);
                state.Events.OnEnd += () =>
                {
                    IsSwitching = false;
                };
            }
        }
        else
        {
            IsOpen = true;
            if (AnimancerComponent != null && openClip != null)
            {
                IsSwitching = true;
                var state = AnimancerComponent.Play(openClip, 0);

                state.Events.OnEnd += () =>
                {
                    IsSwitching = false;
                };
            }
        }

    }
}