using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : Bullet
{
    Rigidbody _rigidbody;
    float angularPower = 2;
    float scaleValue = 0.1f;
    bool isShoot;
    
    // Start is called before the first frame update
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());
        
    }

    IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(2.2f);
        isShoot = true;
    }
    IEnumerator GainPower()
    {
        while (!isShoot)
        {
            angularPower += 0.2f;
            scaleValue += 0.486f  * Time.deltaTime;
            transform.localScale = Vector3.one * scaleValue;
            _rigidbody.AddTorque(transform.right * angularPower, ForceMode.Acceleration); // 지속적으로 오릴 때 엑셀
            yield return null;
        }
    }
}
