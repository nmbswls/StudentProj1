using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMagicLenCtrl : MonoBehaviour
{
    public float TargetScale = 6;
    public GameObject MainLen;

    private void Awake()
    {
        MainLen.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public  void ShowFade()
    {
        MainLen.SetActive(true);
        MainLen.transform.localScale = new(0.5f, 0.5f);
        MainLen.transform.DOScale(TargetScale, 0.5f);
    }
}
