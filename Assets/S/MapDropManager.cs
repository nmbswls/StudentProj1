using System.Collections.Generic;
using UnityEngine;

public class MapDropManager : MonoBehaviour
{
    private Dictionary<long, SceneDropItem> _sceneDroppedItems = new Dictionary<long, SceneDropItem>();
    private Queue<SceneDropItem> _innerPool = new();
    public GameObject interactablePrefab;

    public static long DropIdInstCounter = 100;

    public void CreateDrop(string itemId, long amount, Vector2 position, bool autoPick, Vector2? sourcePos)
    {
        var dropData = new DropData(DropIdInstCounter++, itemId, (int)amount, position, createTime: Time.time, autoPick);
        //EvOnDropAdd?.Invoke(dropData, sourcePos);

        if (_sceneDroppedItems.ContainsKey(dropData.Id)) return;

        // 生成交互物
        var go = SpawnInteractable(dropData, sourcePos, dropData.AutoPick);
        _sceneDroppedItems[dropData.Id] = go;
    }

    private SceneDropItem SpawnInteractable(DropData dropData, Vector3? srcPos, bool autoPick)
    {
        SceneDropItem interactObj = null;
        if (_innerPool.Count > 0)
        {
            interactObj = _innerPool.Dequeue();
            interactObj.gameObject.SetActive(true);
        }
        else
        {
            // 对象池可替换 Instantiate
            var go = Instantiate(interactablePrefab, dropData.Position, Quaternion.identity, transform);
            interactObj = go.GetComponent<SceneDropItem>();
            interactObj.gameObject.SetActive(true);
        }

        if (interactObj == null)
        {
            return null;
        }

        interactObj.InitFromDrop(dropData, srcPos, autoPick);
        return interactObj;
    }


    public void PickDrop(long id)
    {
        _sceneDroppedItems.TryGetValue(id, out var dropData);
        if (dropData != null)
        {
            RemoveDrop(id, isRecycle: false);

            Debug.Log("PickDrop " + id);
            //logicManager.playerDataManager.TryGiveItem(dropData.ItemId, dropData.Amount, 0);
        }
    }

    public void RemoveDrop(long id, bool isRecycle)
    {
        if (_sceneDroppedItems.TryGetValue(id, out var spawnedInteract))
        {
            _sceneDroppedItems.Remove(id);
            if (_innerPool.Count < 20)
            {
                spawnedInteract.DoRecycle();
                _innerPool.Enqueue(spawnedInteract);
            }
            else
            {
                GameObject.Destroy(spawnedInteract.gameObject);
            }
        }
    }
}