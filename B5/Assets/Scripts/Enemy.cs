using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

public class Enemy : MonoBehaviour
{
    public enum Type
    {
        A,
        B,
        C,
        D
    };

    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public int score;
    public GameManager manager;
    public Transform target;
    public bool isChase;
    public BoxCollider _meleeArea;
    public bool isAttack;
    public GameObject bullet;
    public GameObject[] coins;
    public bool isFirstC;
    public bool isDead;
    
    public Rigidbody _rigidbody;
    public BoxCollider _boxCollider;
    public MeshRenderer[] _meshs;
    public NavMeshAgent _nav;
    public Animator _animator;
    
    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        _meshs = GetComponentsInChildren<MeshRenderer>();
        _nav = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();

        if (enemyType != Type.D)
        {
            if (enemyType != Type.C)
                ChaseStart();
            else
                Invoke("ChaseStart", 2);
        }
    }

    void ChaseStart()
    {
        
        if (enemyType != Type.C)
        {
            Walking();
        }
        else
        {
            Invoke("Walking", 2);
        }
    }
    void Walking()
    {        
        isChase = true;
        _animator.SetBool("isWalk", true);
    }
    void Update()
    {
        if (_nav.enabled && enemyType != Type.D)
        {
            _nav.SetDestination(target.position);
            _nav.isStopped = !isChase;
        }
    }
    void FreezeVelocity()
    {
        if (isChase)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }

    void Targeting()
    {
        if (enemyType != Type.D && !isDead)
        {
            float targetRadius = 0; // ?????? ??????, ?????????, ?????????
            float targetRange = 0; // radius????????? ????????? ?????? ???????????? ??? ????????? ??????, ?????????, ??????

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits = //??????????????? ???????????? ?????? ??????????????? ??????????????? ????????????~
                Physics.SphereCastAll(transform.position, targetRadius,
                    transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack) // ???????????? ????????? ????????????????????? ??????????????? ????????????.
            {
                StartCoroutine("Attack");
            }
        }
    }

    IEnumerator Attack()
    {
        isChase = false; // ????????? ?????????
        isAttack = true; // ?????? ??????
        _animator.SetBool("isAttack", true); // ???????????????

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f); // ?????? ?????? ?????????
                _meleeArea.enabled = true; // 0.2?????? ???????????? ??????????????? ???????????? ???????????? ??????
                
                yield return new WaitForSeconds(1f); // ?????? ????????? 1??? ?????? ????????? ??????
                _meleeArea.enabled = false; // ?????? ??????
                
                yield return new WaitForSeconds(1f); // ??????, ?????? ?????? ?????? 1??? ??????
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                _rigidbody.AddForce(transform.forward * 20, ForceMode.Impulse);
                _meleeArea.enabled = true;
                
                yield return new WaitForSeconds(0.5f);
                _rigidbody.velocity = Vector3.zero;
                _meleeArea.enabled = false;
                
                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f); // ?????? ?????? ??????
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20; // add?????? ?????? ??????.

                yield return new WaitForSeconds(2f); // 2??? ??? ?????? ??????????????? 
                break;
        }
        
        isChase = true; // ?????? ????????????
        isAttack = false; // ????????? ??????
        _animator.SetBool("isAttack", false); // ????????????????????? ??????
    }
    
    void FixedUpdate()
    {
        FreezeVelocity();
        Targeting();
    }

    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));

        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVec, false));

        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        foreach (MeshRenderer mesh in _meshs)
            mesh.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (MeshRenderer mesh in _meshs)
                mesh.material.color = Color.white;
        }
        else
        {
            foreach (MeshRenderer mesh in _meshs)
                mesh.material.color = Color.gray;
            gameObject.layer = 14;
            isDead = true;
            isChase = false;
            _nav.enabled = false;
            _animator.SetTrigger("doDie");

            Player player = target.GetComponent<Player>();
            player.score += score;

            int ranCoin = UnityEngine.Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

            switch (enemyType)
            {
                case Type.A:
                    manager.enemyCntA--;
                    break;
                case Type.B:
                    manager.enemyCntB--;
                    break;
                case Type.C:
                    manager.enemyCntC--;
                    break;
                case Type.D:
                    manager.enemyCntD--;
                    break;
            }
            
            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                _rigidbody.freezeRotation = false;
                _rigidbody.AddForce(reactVec * 5, ForceMode.Impulse);
                _rigidbody.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                _rigidbody.AddForce(reactVec * 5, ForceMode.Impulse);
            }
            Destroy(gameObject, 4);
        }
    }
}
