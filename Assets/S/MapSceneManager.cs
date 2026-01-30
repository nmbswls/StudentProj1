using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapSceneManager : MonoBehaviour
{

    // todo 移动到scene管理器
    public float MoveRangeMin;
    public float MoveRangeMax;

    public Transform BornPos;
    public Transform VCamRoot;

    public SceneFogProvider FogProvider;
    public Dictionary<long, SceneInteractableBasic> SceneInteractables = new();

    public Dictionary<string, MapFloorGroup> SceneFloorGroups = new();
    public void InitScene(GameObject sceneRoot)
    {
        BornPos = null;

        InitAndCollectSceneInteracts(sceneRoot);

        var floorGroups = sceneRoot.GetComponentsInChildren<MapFloorGroup>();
        foreach(var fg in floorGroups)
        {
            SceneFloorGroups[fg.FloorGroupName] = fg;
        }

        BornPos = sceneRoot.transform.Find("BornPos");
        VCamRoot = sceneRoot.transform.Find("VCamRoot");

        if(VCamRoot != null)
        {
            for (int i = 0; i < VCamRoot.childCount; i++)
            {
                var c = VCamRoot.GetChild(i);
                var vCam = c.GetComponent<CinemachineVirtualCamera>();
                if (vCam == null)
                {
                    continue;
                }

                vCam.Follow = MainGameManager.Instance.PlayerPresenter.EyePos;

                //vCam.FollowTargetAsVcam
            }
        }

        FogProvider = sceneRoot.transform.GetComponent<SceneFogProvider>();
        FogProvider.TargetDensity = 3.0f;
    }

    public void InitFogs(GameObject sceneRoot)
    {
    }

    public void InitAndCollectSceneInteracts(GameObject sceneRoot)
    {
        SceneInteractables.Clear();
        var interacts = sceneRoot.GetComponentsInChildren<SceneInteractableBasic>();
        ;
        foreach(var interact in interacts)
        {
            SceneInteractables[interact.Id] = interact;
        }
    }


    public void ShowTmpFloorGroup(string fgName)
    {
        SceneFloorGroups.TryGetValue(fgName, out var group);
        group.ShowFloorGroup();
    }

    public void SwitchLayer()
    {

    }
}
