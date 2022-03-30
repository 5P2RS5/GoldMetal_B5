using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;
    
    public GameObject[] weapons;
    public bool[] hasWeapons;
   
    public GameObject[] grenades;
    public int hasGrenades;
    public GameObject grnadeObj;
    public GameManager manager;

    public AudioSource jumpSound;
    
    public int ammo;
    public int coin;
    public int health;
    public int score;
    public int max_ammo;
    public int max_coin;
    public int max_health;
    public int max_hasGrenades;
    public Camera followCamera;
    
    float h;
    float v;
    
    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;
    bool fDown;
    bool gDown;
    bool rDown;
    
    
    bool isFireReady = true;
    bool isBorder;
    bool isDamage;
    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isShop;
    bool isDead;
    
    Vector3 moveVec;
    Vector3 dodgeVec; // 회피시 방향에 대한 변수

    MeshRenderer[] _meshs;
    Animator _animator;
    Rigidbody _rigidbody;
    GameObject nearObject;
    public Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;
    

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _meshs = GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interaction();

    }

    // 인풋관련
    void GetInput()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
        
        wDown = Input.GetButton("Walk");
        
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        
    }

    // 플레이어 이동관련
    void Move()
    {
        moveVec = new Vector3(h, 0, v).normalized;

        // 회피시 방향전환 불가능하게 막기
        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || !isFireReady || isReload || isDead)
            moveVec = Vector3.zero;
        
        if(!isBorder)
            transform.position += moveVec * Speed * (wDown ? 0.3f : 1) * Time.deltaTime;

        _animator.SetBool("isRun", moveVec != Vector3.zero);
        _animator.SetBool("isWalk", wDown);
    }

    // 플레이어 시선 전환
    void Turn()
    {
        // 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);
        
        // 마우스에 의한 회전
        if (fDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // y축 오브젝트 보는거 방지
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isDead)
        {
            _rigidbody.AddForce(Vector3.up * 15, ForceMode.Impulse); // 즉발적인 힘은 Impulse
            isJump = true;
            _animator.SetBool("isJump", true);
            _animator.SetTrigger("doJump");

            jumpSound.Play();
        }
    }

    void Grenade()
    {
        if(hasGrenades == 0)
            return;
        
        if (gDown && !isReload && !isSwap && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grnadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;
        
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap && !isShop && !isDead)
        {
            equipWeapon.Use();
            if (Weapon.Type.Melee == equipWeapon.type)
            {
                _animator.SetTrigger("doSwing");
            }
            else if (Weapon.Type.Range == equipWeapon.type && equipWeapon.curAmmo > 0)
            {
                _animator.SetTrigger("doShot");
            }
            // _animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
        
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (ammo <= 0)
            return;
        if (equipWeapon.curAmmo == equipWeapon.maxAmmo)
            return;
        if (rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isReload && !isDead)
        {
            _animator.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        int reAmmo2 = equipWeapon.maxAmmo - equipWeapon.curAmmo;
        Debug.Log(reAmmo);
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo2;
        isReload = false;
    }
    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isShop && !isDead)
        {
            dodgeVec = moveVec;
            Speed *= 2;
            _animator.SetTrigger("doDodge");
            isDodge = true;
            
            Invoke("DodgeOut", 0.55f);
        }
    }

    void DodgeOut()
    {
        isDodge = false;
        Speed *= 0.5f;
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;
        
        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;
        
        if ((sDown1 || sDown2 || sDown3)&& !isJump && !isDodge && !isShop && !isDead)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            weapons[weaponIndex].gameObject.SetActive(true);
            
            _animator.SetTrigger("doSwap");

            isSwap = true;
            
            Invoke("SwapOut", 0.4f);
        }
    }
    
    void SwapOut()
    {
        isSwap = false;
    }
    void Interaction()
    {
        if (iDown && nearObject != null && !isJump && !isDodge && !isSwap && !isDead)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;
                
                Destroy(nearObject);
            }
            
            if (nearObject.tag == "Shop")
            {
                isShop = true;
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                                
            }
            
        }
    }

    void FreezeRotation()
    {
        _rigidbody.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, 
            transform.forward, 5, LayerMask.GetMask("Wall"));
    }
    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            _animator.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > max_ammo)
                        ammo = max_ammo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > max_coin)
                        coin = max_coin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > max_health)
                        health = max_health;
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == max_hasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    // if (hasGrenades > max_hasGrenades)
                    //     hasGrenades = max_hasGrenades;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";
                
                if(other.GetComponent<Rigidbody>() != null)
                    Destroy(other.gameObject);
                
                StartCoroutine(OnDamage(isBossAtk));
            }

            if (other.GetComponent<Rigidbody>() != null)
            {
                Destroy(other.gameObject);
            }
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;
        foreach (MeshRenderer mesh in _meshs)
        {
            mesh.material.color = Color.yellow;
        }
        if(isBossAtk)
            _rigidbody.AddForce(transform.forward * -25, ForceMode.Impulse);
        
        if (health <= 0 && !isDead)
            OnDie();
        
        yield return new WaitForSeconds(1f);
        
        isDamage = false;
        foreach (MeshRenderer mesh in _meshs)
        {
            mesh.material.color = Color.white;
        }
        
        if(isBossAtk)
            _rigidbody.velocity = Vector3.zero;
        
    }

    void OnDie()
    {
        _animator.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
        {
            nearObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
        else if (other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
         
    }
}
