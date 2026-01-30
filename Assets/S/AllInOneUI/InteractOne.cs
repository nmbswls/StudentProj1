using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractOne : MonoBehaviour
{

    public TextMeshProUGUI TextLabel;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 动态设置数据并重建
    public void SetData(List<(long, string, bool)> newData, int initialIndex = 0)
    {
        //data = newData ?? new List<(long, string, bool)>();
        //currentIndex = Mathf.Clamp(initialIndex, 0, Mathf.Max(0, data.Count - 1));
        //selectedIndex = -1;
        //listView.SetListItemCount(data.Count, false);
        //listView.RefreshAllShownItem();
        //ScrollToCenter(currentIndex);

        //this.viewport.sizeDelta = new(this.viewport.sizeDelta.x, data.Count * itemHeight);

        TextLabel.text = "使用";
    }

}
