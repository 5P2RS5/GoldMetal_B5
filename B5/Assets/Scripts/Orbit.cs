using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target; // 공전 목표

    public float orbitSpeed; // 속도

    Vector3 offSet; // 보정값

    void Start()
    {
        offSet = transform.position - target.position;
    }


    void Update()
    {
        transform.position = target.position + offSet;
        transform.RotateAround(target.position,
            Vector3.up, orbitSpeed * Time.deltaTime);

        offSet = transform.position - target.position;

    }
}
