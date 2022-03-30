using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public enum Type
    {
        Melee, Range
    }
    public Type type;
    
    public int damage;
    public float rate;
    public int maxAmmo;
    public int curAmmo;
    
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    
    public Transform bulletPos; // 총알 발사 위치 (발사 되는 곳)
    public GameObject bullet; // 총알

    public Transform bulletCasePos; // 탄피 나오는 위치
    public GameObject bulletCase; // 탄피
    
    
    
    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if(type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        // 1
        yield return new WaitForSeconds(0.1f); // 0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        // 2 
        yield return new WaitForSeconds(0.3f); // 0.1초 대기
        meleeArea.enabled = false;
        // 3
        yield return new WaitForSeconds(0.3f); // 0.1초 대기
        trailEffect.enabled = false;
    }
    
    IEnumerator Shot()
    {
        // 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation); // 인스턴스화
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>(); // 물리 적용
        bulletRigid.velocity = bulletPos.forward * 50; // 속도
        
        yield return null;
        
        // 탄피 배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation); // 인스턴스화
        Rigidbody caseRigid = instantBullet.GetComponent<Rigidbody>(); // 물리 적용
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3); // 속도
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
        
        
    }
    
    // 일반 함수 : Use() 메인루틴 -> Swing() 서브루틴 -> Use 메인루틴
    // 코루틴 : Use() 메인 루틴 + Swing() 코루틴(동시 실행)
}
