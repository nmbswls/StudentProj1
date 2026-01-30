using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public partial class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; set; }
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    public int Sanity;

    public enum ESanityState
    {
        Noemal,
        Deep,
        Danger,
    }

    public ESanityState SanityState;
    public float ChangeStateTimer;
    public int DangerClearTimes;
    public bool IsDead = false;

    public event Action<ESanityState> EventOnDeepModeChange;
    public event Action EventOnPlayerDead;

    public void Update()
    {
        if(IsDead)
        {
            return;
        }

        if(SanityState == ESanityState.Deep)
        {
            if(Time.time - ChangeStateTimer > 3f)
            {
                SwitchSanityMode(ESanityState.Danger);
            }
        }
        else if(SanityState == ESanityState.Danger)
        {
            if (Time.time - ChangeStateTimer > 10)
            {
                //SwitchSanityMode(ESanityState.Danger);
                IsDead = true;
                EventOnPlayerDead?.Invoke();
            }
        }
    }


    public void SwitchSanityMode(ESanityState state)
    {
        if(SanityState == state)
        {
            return;
        }
        SanityState = state;
        ChangeStateTimer = Time.time;

        EventOnDeepModeChange?.Invoke(state);

        if(state == ESanityState.Danger)
        {
            DangerClearTimes = 0;
        }
    }

    public void AddCLearTimes()
    {
        DangerClearTimes += 1;
        if(DangerClearTimes >= 3)
        {
            SwitchSanityMode(ESanityState.Noemal);
        }
    }
}
