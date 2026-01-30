using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTeleporter : SceneInteractableBasic
{

    public string SceneName;

    public override void TriggerInteract(int selectionId)
    {
        if (string.IsNullOrWhiteSpace(SceneName))
        {
            MainGameManager.Instance.LoadWorld(SceneName, true, (ret) =>
            {

            });
        }
    }

}
