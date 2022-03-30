using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class Boss : Enemy
{
   public GameObject missile;
   public Transform missilePortA;
   public Transform missilePortB;

   // 플레이어 이동 예측
   Vector3 lookVec;
   // 어디에 내려 찍을지 저장하는 변수
   Vector3 tauntVec;
   // 플레이어 바라보는 플래스
   public bool isLook;

   void Awake() // Awake() 함수는 자식 스크립트만 단독 실행한다.
   {
      _rigidbody = GetComponent<Rigidbody>();
      _boxCollider = GetComponent<BoxCollider>();
      _meshs = GetComponentsInChildren<MeshRenderer>();
      _nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
      _animator = GetComponentInChildren<Animator>();
      _nav.isStopped = true;
      StartCoroutine("Think");
   }

   void Update()
   {
      if (isDead)
      {
         StopAllCoroutines(); // 모든 코루틴 멈추기
         return;
      }
      // 플레이어 이동 위치 예측하기
      if(isLook)
      {
         float h = Input.GetAxisRaw("Horizontal");
         float v = Input.GetAxisRaw("Vertical");
         lookVec = new Vector3(h, 0, v) * 5f;
         transform.LookAt(target.position + lookVec);
      }

      if (!isLook)
      {
         _nav.SetDestination(tauntVec); // 점프공격 할 때 목표지점으로 이동하도록 로직 추가
      }
   }

   IEnumerator Think()
   {
      yield return new WaitForSeconds(0.1f); // 보스 패턴 난이도

      int ranAction = Random.Range(0, 5);
      switch (ranAction)
      {
         case 0:
         case 1:
            // 미사일 발사 패턴
            StartCoroutine(MissileShot());
            break;
         case 2:
         case 3:
            // 돌 굴러가는 패턴
            StartCoroutine(RockShot());
            break;
         case 4:
            // 점프 공격 패턴
            StartCoroutine(Taunt());
            break;
         
      }
   }

   IEnumerator MissileShot()
   {
      
      _animator.SetTrigger("doShot");
      yield return new WaitForSeconds(0.2f);
      GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
      BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
      bossMissileA.target = target;
      
      yield return new WaitForSeconds(0.3f);
      GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
      BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
      bossMissileB.target = target;

      yield return new WaitForSeconds(2f);
      StartCoroutine("Think");
   }
   IEnumerator RockShot()
   {
      isLook = false; // 돌 만드는 동안 시선 멈추기
      _animator.SetTrigger("doBigShot");
      Instantiate(bullet, transform.position, transform.rotation);
      yield return new WaitForSeconds(3f);
      isLook = true;
      StartCoroutine("Think");
   }
   IEnumerator Taunt()
   {
      tauntVec = target.position + lookVec;
      isLook = false;
      _boxCollider.enabled = false; // 점프하는 도중에 플레이어 밀지 않도록 하기위해
      _nav.isStopped = false;

      _animator.SetTrigger("doTaunt");
      yield return new WaitForSeconds(1.5f);
      _meleeArea.enabled = true;

      yield return new WaitForSeconds(0.5f);
      _meleeArea.enabled = false;
      
      yield return new WaitForSeconds(1f);
      isLook = true;
      _boxCollider.enabled = true;
      _nav.isStopped = true;
      StartCoroutine("Think");
   }
   
}
