using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerState;
using static SceneInteractSystem;

public class AllInOneUIManager : MonoBehaviour
{
    public static AllInOneUIManager Instance { get; set; }

    public Canvas RootCanvas;
    public Camera UICamera;

    public VisionMaskController VisionMaskController;

    public TextMeshProUGUI DangerLeftTex;
    public RectTransform DeadMask;
    public Button DeadMaskClickArea;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        RootCanvas = GetComponent<Canvas>();

        if (InteractOne != null)
        {
            InteractOne.gameObject.SetActive(false);
        }

        DeadMaskClickArea.onClick.RemoveAllListeners();
        DeadMaskClickArea.onClick.AddListener(() =>
        {
            MainGameManager.Instance.DoPlayerSpawn();
        });
    }

    private void Start()
    {
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.EventOnDeepModeChange += OnPlayerSanityModeChange;
            PlayerState.Instance.EventOnPlayerDead += OnPlayerDead;
        }

        OnPlayerSanityModeChange(ESanityState.Noemal);
    }

    private void OnDestroy()
    {
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.EventOnDeepModeChange -= OnPlayerSanityModeChange;
            PlayerState.Instance.EventOnPlayerDead -= OnPlayerDead;
        }
    }

    private void OnPlayerSanityModeChange(ESanityState state)
    {
        if(state == ESanityState.Noemal)
        {
            //AllInOneUIManager.Instance.VisionMaskController.IsFocus = false;
            //AllInOneUIManager.Instance.VisionMaskController.IsFocusss = false;
        }
        else if(state == ESanityState.Deep)
        {
            //AllInOneUIManager.Instance.VisionMaskController.IsFocus = true;
            //AllInOneUIManager.Instance.VisionMaskController.IsFocusss = false;
        }
        else
        {
            //AllInOneUIManager.Instance.VisionMaskController.IsFocus = true;
            //AllInOneUIManager.Instance.VisionMaskController.IsFocusss = true;
        }

        if(state == ESanityState.Danger)
        {
            Camera.main.cullingMask = (Camera.main.cullingMask | (1 << LayerMask.NameToLayer("SceneLayer2")));
        }
        else
        {
            Camera.main.cullingMask = (Camera.main.cullingMask) & ~(1 << LayerMask.NameToLayer("SceneLayer2"));
        }
    }

    private void OnPlayerDead()
    {
        DeadMask.gameObject.SetActive(true);
    }

    void Update()
    {
        UpdateInteractShow();

        if(Input.GetKeyDown(KeyCode.F))
        {
            OnConfirm();
        }

        if (PlayerState.Instance != null)
        {
            if(PlayerState.Instance.SanityState == ESanityState.Danger)
            {
                float timePassed = Time.time - PlayerState.Instance.ChangeStateTimer;
                DangerLeftTex.text = (((int)(timePassed * 10) / 10f)).ToString();
            }
            else
            {
                DangerLeftTex.text = "";
            }
        }
    }

    public InteractOne InteractOne;


    public bool InteractShowStatus;
    public List<IntResultItem> CurrInteractPoint = new();
    public ISceneInteractable? currBindPoint = null;


    public bool OnConfirm()
    {
        // 在物体界面选择 进入详细交互
        if (InteractShowStatus)
        {
            if (currBindPoint == null)
            {
                Debug.LogError("nooooo bind interact");
                return false;
            }
            currBindPoint.TriggerInteract(0);
        }
        
        return true;
    }


    public void UpdateInteractShow()
    {
        if(!this.InteractShowStatus)
        {
            return;
        }

        var selections = currBindPoint.GetInteractSelections(0);
        var hintPos = currBindPoint.GetHintAnchorPosition();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(hintPos);
        // 如果是 Screen Space - Camera 或 World Space，用 RectTransformUtility：
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            RootCanvas.transform as RectTransform,
            screenPos,
            UICamera,   // Screen Space - Camera 用摄像机；Overlay 模式传 null
            out Vector2 localPos
        );

        InteractOne.transform.localPosition = localPos;
    }
    /// <summary>
    /// 刷新交互物
    /// </summary>
    /// <param name="interactPoints"></param>
    public void RefreshInteractObjs(List<IntResultItem> interactPoints)
    {
        this.CurrInteractPoint.Clear();
        if (interactPoints.Count > 0)
        {
            var firstPoint = interactPoints[0];
            this.CurrInteractPoint.Add(firstPoint);


            //for (int i = 1; i < interactPoints.Count; i++)
            //{
            //    if ((interactPoints[i].pos - firstPoint.pos).sqrMagnitude < 0.3f * 0.3f)
            //    {
            //        this.CurrInteractPoint.AddRange(interactPoints);
            //    }
            //}
        }

        // 无可交互物 全部隐藏
        // 无可交互物 全部隐藏
        if (CurrInteractPoint.Count == 0)
        {
            InteractShowStatus = false;
            InteractOne.gameObject.SetActive(false);
        }
        else
        {
            ShowDirectInteractMenuOnObj(CurrInteractPoint.First().interactable, CurrInteractPoint.First().distance);
        }
    }

    /// <summary>
    /// 刷新详细交互小界面
    /// </summary>
    /// <param name="interactObj"></param>
    private void ShowDirectInteractMenuOnObj(ISceneInteractable interactObj, float dist)
    {
        InteractOne.gameObject.SetActive(true);

        this.InteractShowStatus = true;
        this.currBindPoint = interactObj;

        var selections = interactObj.GetInteractSelections(dist);

        var hintPos = interactObj.GetHintAnchorPosition();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(hintPos);

        // 如果是 Screen Space - Camera 或 World Space，用 RectTransformUtility：
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            RootCanvas.transform as RectTransform,
            screenPos,
            UICamera,   // Screen Space - Camera 用摄像机；Overlay 模式传 null
            out Vector2 localPos
        );

        InteractOne.transform.localPosition = localPos;

        var innerList = new List<(long, string, bool)>();
        foreach (var one in selections)
        {
            innerList.Add(new(one.SelectId, one.SelectContent, one.Selectable));
        }
        InteractOne.SetData(innerList);
    }

    public void AddShowBottomText(string content)
    {

    }
}
