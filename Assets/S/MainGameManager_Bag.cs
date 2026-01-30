using Item;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Interaact
{
    
}

namespace Item
{
    [Serializable]
    public abstract class ItemInstanceInfo
    {

    }


    [Serializable]
    public class ItemStack
    {
        public string ItemID;
        public long Count;
        public long ItemInstanceId;
        public ItemInstanceInfo InstanceInfo;

        public ItemStack(string id, long count)
        {
            ItemID = id;
            Count = count;
        }

        public bool CanStackWith(ItemStack other)
        {
            if (other == null) return false;
            return other.ItemID == ItemID;
        }

        public long AddToStack(long amount, long maxStack)
        {
            long canAdd = Math.Max(0, maxStack - Count);
            long added = Math.Min(canAdd, amount);
            Count += added;
            return added;
        }

        public long RemoveFromStack(long amount)
        {
            long removed = Math.Min(amount, Count);
            Count -= removed;
            return removed;
        }

        public bool IsEmpty => string.IsNullOrEmpty(ItemID) || Count <= 0;
    }
}

public class MyBag
{
    public List<ItemStack> BagSlots = new List<ItemStack>();

    // 给道具
    public long TryGiveItem(string itemId, long amount)
    {
        long remaining = amount;
        //// 找普通格子空位继续放
        //for (int i = 0; i < BagSlots.Count && remaining > 0; i++)
        //{
        //    if (BagSlots[i] == null || BagSlots[i].IsEmpty)
        //    {
        //        //var put = Math.Min(maxStack, remaining);
        //        //BagSlots[i] = FakeItemDatabase.CreateItemStack(itemId, put);
        //        remaining -= put;
        //    }
        //}

        //if (remaining <= 0)
        //{
        //    return count;
        //}

        //// 额外还有空位
        //while (ExtraSlots.Count < MaxExtraCapacity && remaining > 0)
        //{
        //    var put = Math.Min(maxStack, remaining);
        //    var newItem = FakeItemDatabase.CreateItemStack(itemId, put);
        //    ExtraSlots.Add(newItem);
        //    remaining -= put;
        //}
        return 0;
    }


    public long TryCostItem(string itemId, long costItem)
    {
        long leftCount = costItem;
        if (leftCount > 0)
        {
            foreach (var slot in BagSlots)
            {
                if (slot == null) continue;
                if (slot.ItemID != itemId) { continue; }

                if (slot.Count > leftCount)
                {
                    slot.Count -= leftCount;
                    leftCount = 0;
                }
                else
                {
                    leftCount -= slot.Count;
                    slot.Count = 0;
                }

                if (leftCount <= 0)
                {
                    break;
                }
            }
        }

        

        ClearEmptyItems();

        return leftCount;
    }

    public long GetItemCount(string itemId)
    {
        long totalNum = 0;

        foreach (var slot in BagSlots)
        {
            if (slot == null) continue;
            if (slot.ItemID != itemId) { continue; }

            totalNum += slot.Count;
        }

        return totalNum;
    }


    public void ClearEmptyItems()
    {
        for (int i = BagSlots.Count - 1; i >= 0; i--)
        {
            if (BagSlots[i] == null || BagSlots[i].Count <= 0)
            {
                BagSlots.RemoveAt(i);
            }
        }
    }
}




public partial class MainGameManager : MonoBehaviour
{
    public MyBag MyBag;

    
}
