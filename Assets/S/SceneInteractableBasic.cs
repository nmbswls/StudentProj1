using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;
using static UnityEngine.UI.GridLayoutGroup;

[Serializable]
public class LogicInteractOutput
{
    public enum EOutputType
    {
        Invalid,
        ChangeSelfStatus,
        FinishTask,
        GiveItems,
        CostItems,
        Teleport,
        OpenPanel,

        SpecialMoveTo,
        ShowAnim,

        UpdateMoveRange,
        TriggerTargetInteract,

        SetSwitch,

        ShowFloorGroup,
        ShowMagicLen,
    }

    public EOutputType OutputType;
    public long Param1;
    public long Param2;
    public string Param3;
    public string Param4;
    public float Param5;
    public float Param6;

    public float DelayTime;
}

[Serializable]
public class InteractCheckCond
{
    public enum ECheckType
    {
        None,
        AlwaysFalse,
        NotHide,
    }

    public ECheckType CheckType;
    public long Param1;
    public long Param2;
    public string Param3;
    public string Param4;
}


/// <summary>
/// 所有条件归一化
/// </summary>
public enum ECommonCheckType
{
    None,
    TaskFinish,
    SwitchSet,
    SwitchNoSet,

    OwnItem, // p5 itemid p1 count
}


[Serializable]
public class CommonCheckCond
{
    public ECommonCheckType Type;
    public long Param1;
    public long Param2;
    public long Param3;
    public long Param4;
    public string Param5;
    public string Param6;
}

[Serializable]
public class MapInteractInfo
{
    // 所有交互放一起
    public int InteractId;
    public string Label; // 选项
    public string UnLabel; // 灰色选项
    public bool HideWhenFail = true;
    public float NeedDist = 0.4f;

    public List<CommonCheckCond> CheckCommonCond = new();
    public List<InteractCheckCond> CheckInteractCond = new();
    public List<LogicInteractOutput> Outputs = new();

}



[Serializable]
public class InteractableConfig
{
    public long Id;

    [Serializable]
    public class StatusInfo
    {
        public int StatusId;

        public List<MapInteractInfo> InteractInfos = new();

        public bool HasBlock = false;
        public bool ShowView = false;

        public bool AutoTrigger;
    }

    [Serializable]
    public class StatusChangeRule
    {
        public int FromStatus;
        public List<CommonCheckCond> CommonConds = new();
        public List<string> NeedSelfFlag = new();
        public int ToStatus;
    }


    public StatusInfo MainStatusInfo;
    public List<StatusInfo> ExtraStatusInfos;


    /// <summary>
    /// 状态切换规则
    /// </summary>
    public List<StatusChangeRule> StateChangeRules = new();
}



public class SceneInteractableBasic : MonoBehaviour, ISceneInteractable
{

    public string showName;
    public Transform HintPivot;
    public Animator mainAnimator;

    public GameObject ViewRoot;

    public InteractableConfig Config;

    public long Id { get { return Config.Id; } }

    public int CurrStatusId = 0;
    public event Action<int> OnStatusChange;


    private float _lowFreqCheckStatusTimer = 0;
    private List<MapInteractInfo> interactInfos = new();

    void Awake()
    {
        if(mainAnimator == null)
        {
            mainAnimator = GetComponent<Animator>();
        }
    }

    void Start()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        CurrStatusId = 0;

        var curState = GetCurrentStatusInfo();
        if (curState != null)
        {
            interactInfos.Clear();
            interactInfos.AddRange(curState.InteractInfos);
        }

        CheckStatusCondition();

        RefreshStatusView();
    }



    public void Update()
    {
        LowFreqCheckStatusChange();

        TickRunningInteract();
    }


    /// <summary>
    /// 低频检查
    /// </summary>
    protected void LowFreqCheckStatusChange()
    {
        if (Time.time < _lowFreqCheckStatusTimer)
        {
            return;
        }

        _lowFreqCheckStatusTimer = Time.time + 2f;

        CheckStatusCondition();
    }


    public bool IsAutoInteract()
    {
        var status = GetCurrentStatusInfo();
        if (status == null) return false;
        return status.AutoTrigger;
    }


    public void ChangeSelfStatus(int newStatus)
    {
        int oldStat = CurrStatusId;
        CurrStatusId = newStatus;

        var curState = GetCurrentStatusInfo();
        if (curState != null)
        {
            this.interactInfos.Clear();
            this.interactInfos.AddRange(curState.InteractInfos);
        }
        else
        {
            this.interactInfos.Clear();
        }

        if(_currInteractCtx != null)
        {
            _currInteractCtx.Outputs = null;
        }

        RefreshStatusView();


        OnStatusChange?.Invoke(newStatus);
    }

    private void RefreshStatusView()
    {
        var curState = GetCurrentStatusInfo();
        if (curState != null && ViewRoot != null)
        {
            if (curState.ShowView)
            {
                ViewRoot.SetActive(true);
            }
            else
            {
                ViewRoot.SetActive(false);
            }
        }
    }

    public InteractableConfig.StatusInfo GetCurrentStatusInfo()
    {
        if (CurrStatusId == 0)
        {
            return Config.MainStatusInfo;
        }

        var findIt = Config.ExtraStatusInfos.Find((item) => item.StatusId == CurrStatusId);
        return findIt;

    }

    /// <summary>
    /// 检查状态切换
    /// </summary>
    public void CheckStatusCondition()
    {
        foreach (var rule in Config.StateChangeRules)
        {
            if (rule.FromStatus != CurrStatusId)
            {
                continue;
            }

            var poassed = true;
            foreach (var cond in rule.CommonConds)
            {
                if (!MainGameManager.Instance.CheckCommonCond(cond))
                {
                    poassed = false;
                    break;
                }
            }

            if (poassed)
            {
                ChangeSelfStatus(rule.ToStatus);
                break;
            }
        }
    }

    public string ShowName => showName;

    public Vector2 Pos => transform.position;


    public void TickRunningInteract()
    {
        if (_currInteractCtx == null)
        {
            return;
        }

        if(_currInteractCtx.Outputs == null)
        {
            _currInteractCtx = null;
            return;
        }

        if(_currInteractCtx.RunnintStepId >= _currInteractCtx.Outputs.Count)
        {
            Debug.Log("Finish ALl");
            return;
        }

        var output = _currInteractCtx.Outputs[_currInteractCtx.RunnintStepId];
        bool finished = false;
        switch (output.OutputType)
        {
            case LogicInteractOutput.EOutputType.ShowAnim:
                {
                    _currInteractCtx.RunnintStatus1 += Time.deltaTime;

                    float timer = _currInteractCtx.RunnintStatus1;
                    if(timer > output.Param5)
                    {
                        finished = true;
                        break;
                    }
                }
                break;
        }

        if(finished)
        {
            _currInteractCtx.RunnintStepId += 1;
            HandleInteractoutputs();
        }
    }

    public virtual bool CanInteractEnable(float dist)
    {
        if(_currInteractCtx != null)
        {
            return false;
        }

        var currState = GetCurrentStatusInfo();
        if (currState == null) return false;

        foreach (var i in interactInfos)
        {
            bool canInt = CheckTriggerInteract(i.InteractId);

            if (canInt) return true;
        }

        return false;
    }

    public virtual Vector3 GetHintAnchorPosition()
    {
        return HintPivot.position;
    }

    public virtual List<SceneInteractSelection> GetInteractSelections(float dist)
    {
        List<SceneInteractSelection> ret = new();
        if (_currInteractCtx != null)
        {
            return ret;
        }

        foreach (var i in interactInfos)
        {
            bool canInt = CheckTriggerInteract(i.InteractId);

            if (!canInt) continue;
            ret.Add(new SceneInteractSelection()
            {
                SelectId = i.InteractId,
                SelectContent = canInt ? i.Label : i.UnLabel,
                Selectable = canInt,
            });
        }

        return ret;
    }


    public bool CheckTriggerInteract(int interactId)
    {
        var interactItem = interactInfos.Find((item) => item.InteractId == interactId);
        if (interactItem == null)
        {
            return false;
        }

        var passed = true;
        foreach (var oneCond in interactItem.CheckCommonCond)
        {
            if (!MainGameManager.Instance.CheckCommonCond(oneCond))
            {
                passed = false;
                break;
            }
        }

        if (passed)
        {
            foreach (var oneCond in interactItem.CheckInteractCond)
            {
                switch (oneCond.CheckType)
                {
                    case InteractCheckCond.ECheckType.AlwaysFalse:
                        {
                            passed = false;
                        }
                        break;
                }
            }
        }


        return passed;
    }

    public virtual void TriggerInteract(int selectionId)
    {
        _currInteractCtx = new()
        {
            RunnintStepId = 0,
            Outputs = interactInfos[0].Outputs,
        };

        // 立即触发一次
        HandleInteractoutputs();
    }


    private void HandleInteractoutputs()
    {
        while(_currInteractCtx.Outputs != null && _currInteractCtx.RunnintStepId < _currInteractCtx.Outputs.Count)
        {
            var output = _currInteractCtx.Outputs[_currInteractCtx.RunnintStepId];
            bool pending = false;
            switch (output.OutputType)
            {
                case LogicInteractOutput.EOutputType.ChangeSelfStatus:
                    {
                        ChangeSelfStatus((int)output.Param1);
                    }
                    break;
                case LogicInteractOutput.EOutputType.TriggerTargetInteract:
                    {
                        var targetId = output.Param1;
                        MainGameManager.Instance.MapManager.SceneInteractables.TryGetValue(targetId, out var targetInteracble);
                        if(targetInteracble == null)
                        {
                            Debug.Log($"TriggerTargetInteract no target found {targetId}");
                            break;
                        }
                        targetInteracble.TriggerInteract((int)output.Param2);
                    }
                    break;
                case LogicInteractOutput.EOutputType.SetSwitch:
                    {
                        MainGameManager.Instance.SetSwitch(output.Param3);
                    }
                    break;
                case LogicInteractOutput.EOutputType.FinishTask:
                    {

                    }
                    break;
                case LogicInteractOutput.EOutputType.UpdateMoveRange:
                    {

                        MainGameManager.Instance.MapManager.MoveRangeMin = output.Param5;
                        MainGameManager.Instance.MapManager.MoveRangeMax = output.Param6;
                    }
                    break;
                case LogicInteractOutput.EOutputType.ShowAnim:
                    {
                        //
                        if(mainAnimator != null)
                        {
                            mainAnimator.SetTrigger(output.Param3);
                        }

                        _currInteractCtx.RunnintStatus1 = 0;
                        pending = true;
                    }
                    break;
                case LogicInteractOutput.EOutputType.ShowFloorGroup:
                    {
                        MainGameManager.Instance.MapManager.ShowTmpFloorGroup(output.Param3);
                    }
                    break;
                case LogicInteractOutput.EOutputType.ShowMagicLen:
                    {
                        MainGameManager.Instance.PlayerMagicLen.ShowFade();
                    }
                    break;
            }

            if (pending)
            {
                break;
            }

            _currInteractCtx.RunnintStepId += 1;
        }
        
        // 处理结束
        if(_currInteractCtx.Outputs == null || _currInteractCtx.RunnintStepId >= _currInteractCtx.Outputs.Count)
        {
            Debug.Log("interact finish");
            _currInteractCtx = null;
        }
    }

    public class InteractRunningCtx
    {
        public int RunnintStepId = 0;
        public List<LogicInteractOutput> Outputs = null;

        public float RunnintStatus1 = 0;
        public float RunnintStatus2 = 0;
        public float RunnintStatus3 = 0;
    }

    public InteractRunningCtx _currInteractCtx = null;
}
