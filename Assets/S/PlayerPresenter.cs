using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPresenter : MonoBehaviour
{

    // todo 移动到scene管理器
    public Transform EyePos;

    // Start is called before the first frame update
    void Start()
    {

        //MainGameManager.Instance.ProgressDist = Mathf.Floor(transform.position.x);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // 裁剪
        //if (transform.position.x < MainGameManager.Instance.MapManager.MoveRangeMin)
        //{
        //    transform.position = new Vector3(MainGameManager.Instance.MapManager.MoveRangeMin, transform.position.y, transform.position.z);
        //}
        //if (transform.position.x > MainGameManager.Instance.MapManager.MoveRangeMax)
        //{
        //    transform.position = new Vector3(MainGameManager.Instance.MapManager.MoveRangeMax, transform.position.y, transform.position.z);
        //}
    }

    public void LateUpdate()
    {
        if (transform.position.x < MainGameManager.Instance.MapManager.MoveRangeMin)
        {
            transform.position = new Vector3(MainGameManager.Instance.MapManager.MoveRangeMin, transform.position.y, transform.position.z);
        }
        if (transform.position.x > MainGameManager.Instance.MapManager.MoveRangeMax)
        {
            transform.position = new Vector3(MainGameManager.Instance.MapManager.MoveRangeMax, transform.position.y, transform.position.z);
        }
    }
    
}
