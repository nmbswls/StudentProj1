using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VolumetricFogAndMist2;

public class MySceneInfo
{
    public string SceneName { get; set; }
}



public partial class MainGameManager : MonoBehaviour
{
    public static MainGameManager Instance { get; set; }

    public PlayerMagicLenCtrl PlayerMagicLen;
    public PlayerPresenter PlayerPresenter;

    public SceneInteractSystem InteractSystem;
    public MapSceneManager MapManager;

    public MistPathController MistController;
    // Start is called before the first frame update

    public VolumetricFogManager FogManager; 
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InteractSystem = new();
        MapManager = GetComponent<MapSceneManager>();

        PlayerPresenter.gameObject.SetActive(false);

        MainGameManager.Instance.LoadWorld("Scene_01", true, (ret) =>
        {

        });
    }

    // Update is called once per frame
    void Update()
    {
        InteractSystem.Tick(Time.deltaTime);
    }

    public HashSet<string> EnableSwitches = new();

    public bool CheckSwitch(string switchName)
    {
        if(EnableSwitches.Contains(switchName))
        {
            return true;
        }

        return false;
    }

    public void SetSwitch(string switchName)
    {
        EnableSwitches.Add(switchName);
    }

    /// <summary>
    /// 条件检查
    /// </summary>
    /// <param name="cond"></param>
    /// <returns></returns>
    public bool CheckCommonCond(CommonCheckCond cond)
    {
        switch (cond.Type)
        {
            case ECommonCheckType.None:
                {
                    return true;
                }
                break;
            case ECommonCheckType.SwitchSet:
                {
                    return MainGameManager.Instance.CheckSwitch(cond.Param5);
                }
                break;
            case ECommonCheckType.SwitchNoSet:
                {
                    return !MainGameManager.Instance.CheckSwitch(cond.Param5);
                }
                break;
                //case ECommonCheckType.OwnItem:
                //    {
                //        string itemId = cond.Param5;
                //        long itemCnt = cond.Param1;

                //        if (playerDataManager.CheckHaveItem(itemId, itemCnt))
                //        {
                //            return true;
                //        }
                //    }
                //    break;
        }
        return false;
    }



    #region 场景

    public MySceneInfo currentSceneInfo;
    public readonly List<Scene> loadedSubScenes = new List<Scene>();

    public event Action<string> OnWorldLoaded;
    public event Action<string> OnWorldUnloaded;
    public event Action<string, float> OnLoadingProgress; // 子场景名，进度0-1

    public void LoadWorld(string sceneName, bool setActive = true, Action<string>? onComplete = null)
    {
        if (onComplete != null)
        {
            OnWorldLoaded = onComplete;
        }
        StartCoroutine(CoLoadWorld(sceneName, setActive));
    }

    private IEnumerator CoLoadWorld(string sceneName, bool setActive)
    {
        // 先卸载旧的
        if (currentSceneInfo != null)
            yield return CoUnloadWorld(null);

        currentSceneInfo = new()
        {
            SceneName = sceneName,
        };
        loadedSubScenes.Clear();

        // 异步依次加载子场景（也可并行）
        do
        {
            if (!IsInBuildSettings(sceneName))
            {
                Debug.LogError($"SubSceneManager: scene '{sceneName}' not in Build Settings.");
                break;
            }
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null) { Debug.LogError($"LoadSceneAsync returned null for {sceneName}"); continue; }
            op.allowSceneActivation = true;

            while (!op.isDone)
            {
                OnLoadingProgress?.Invoke(sceneName, op.progress);
                yield return null;
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid()) loadedSubScenes.Add(scene);
            else Debug.LogError($"Loaded scene invalid: {sceneName}");
        }
        while (false);

        GameObject onlyRoot = null;
        // 设置激活场景（影响 Instantiate 的默认归属、Lighting、NavMesh 等）
        if (setActive)
        {
            var active = loadedSubScenes.FirstOrDefault(s => s.name == sceneName);
            if (active.IsValid())
            {
                SceneManager.SetActiveScene(active);
                var roots = active.GetRootGameObjects();
                onlyRoot = roots.FirstOrDefault();

                MapManager.InitScene(onlyRoot);
            }
            else
            {
                // 若指定的 activeSubScene未加载，默认设为第一个加载的
                if (loadedSubScenes.Count > 0)
                    SceneManager.SetActiveScene(loadedSubScenes[0]);
            }
        }

        PlayerPresenter.transform.position = MapManager.BornPos.position + Vector3.up;
        PlayerPresenter.gameObject.SetActive(true);


        OnWorldLoaded?.Invoke(sceneName);
        Debug.Log($"SubSceneManager: World '{sceneName}' loaded with {loadedSubScenes.Count} sub-scenes.");
    }

    private IEnumerator CoUnloadWorld(Action? onUnload)
    {
        // 逐个卸载
        for (int i = loadedSubScenes.Count - 1; i >= 0; --i)
        {
            var scene = loadedSubScenes[i];
            if (!scene.IsValid()) continue;

            var op = SceneManager.UnloadSceneAsync(scene);
            while (op != null && !op.isDone)
                yield return null;
        }
        loadedSubScenes.Clear();

        var last = currentSceneInfo;
        currentSceneInfo = null;
        OnWorldUnloaded?.Invoke(currentSceneInfo.SceneName);
        Debug.Log("SubSceneManager: world unloaded.");

        onUnload?.Invoke();
    }

    private bool IsInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }


    #endregion


    public void DoPlayerSpawn()
    {
        PlayerPresenter.transform.position = MapManager.BornPos.position;

        PlayerState.Instance.IsDead = true;
        PlayerState.Instance.SwitchSanityMode(PlayerState.ESanityState.Noemal);

        AllInOneUIManager.Instance.DeadMask.gameObject.SetActive(false);
    }
}
