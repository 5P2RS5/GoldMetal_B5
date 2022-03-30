using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target; // 따라가야할 오브젝트

    public Vector3 offset; // 따라갈 목표와 위치 오프셋을 public변수로 선언
    
    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
    }
}
