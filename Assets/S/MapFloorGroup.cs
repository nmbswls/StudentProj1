


using UnityEngine;

public class MapFloorGroup : MonoBehaviour
{
    public string FloorGroupName;

    public void ShowFloorGroup()
    {
        for(int i=0;i<transform.childCount;i++)
        {
            var c = transform.GetChild(i);
            c.gameObject.SetActive(true);
        }
    }
}