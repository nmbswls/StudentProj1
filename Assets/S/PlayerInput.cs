using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlayerState;


public partial class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; set; }
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }


    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(PlayerState.Instance.SanityState == ESanityState.Noemal)
            {
                PlayerState.Instance.SwitchSanityMode(ESanityState.Deep);
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (PlayerState.Instance.SanityState == ESanityState.Deep)
            {
                PlayerState.Instance.SwitchSanityMode(ESanityState.Noemal);
            }
        }
    }
}
