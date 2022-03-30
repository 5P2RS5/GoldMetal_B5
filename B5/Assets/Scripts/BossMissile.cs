using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMissile : Bullet // 자식 : 부모
{
    public Transform target;
    NavMeshAgent _nav;
    void Awake()
    {
        _nav = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        _nav.SetDestination(target.position);
    }
}
