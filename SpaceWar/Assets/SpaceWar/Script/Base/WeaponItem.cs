﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item {

    #region WeaponItem INFO

    public enum AttackTiming
    {
        SCRIPT_ONLY = 0,
        ANIMATION_ONLY
    }
    private Transform m_player = null;
    public Transform PLAYER { get { return m_player; } set { m_player = value; } }

    [SerializeField] private AttackTiming m_attackTiming = AttackTiming.SCRIPT_ONLY;

    public AttackTiming ATTACK_TIMING { get { return m_attackTiming; } set { m_attackTiming = value; } }

    // 데미지 지정
    protected float m_damage = 0.0f;
    public float DAMAGE { get { return m_damage; } set { m_damage = value; } }


    // 공격 명령 내릴때 처음 뜨는 이펙트 
    private GameObject m_shotEffect = null;

    public GameObject SHOT_EFFECT { get { return m_shotEffect; } set { m_shotEffect = value; } }
   
    // 무기 쿨타임
    [SerializeField] private float m_coolTime = 0.0f;

    public float COOL_TIME { get { return m_coolTime; } set { m_coolTime = value; } }
    
    private int m_currentBulletIndex = 0;

    // 원거리 무기 전용
    private int m_ammo = 0;
    public int AMMO { get { return m_ammo; } set { m_ammo = value; } }

    // 총알 나가는 위치
    [SerializeField] private Transform m_firePoint = null;
    public Transform FIRE_POS { get { return m_firePoint; } set { m_firePoint = value; } }

    // 무기 발사 소리
    protected AudioSource m_source = null;
    public AudioSource AUDIO_SOURCE { get { return m_source; } set { m_source = value; } }
    [SerializeField] private AudioClip m_weaponShootSound = null;
    public AudioClip SHOT_SOUND { get { return m_weaponShootSound; } set { m_weaponShootSound = value; } }
    
    #region SWORD 
    // 칼 휘두르는 이펙트 리스트
    private List<Animator> m_swordAnimatorList = new List<Animator>();
    private int m_curSwordEffectIndex = 0; // 이것은 누를때마다 변경됨
    public List<Animator> SWORD_ANIMATOR_LIST
    {
        get { return m_swordAnimatorList; } 
    }

    #endregion



    #endregion

    #region Unity Method 
    void Start()
    {
        Setup();
        m_source = this.GetComponent<AudioSource>();
    }
    #endregion

    #region Weapon Method
    public void SoundPlay()
    {
        if (m_weaponShootSound != null)
        {
            m_source.clip = m_weaponShootSound;
            m_source.Play();
        }
    }

    public void Attack(Transform character)
    {
        m_player = character;
        switch (m_type)
        {
            case ItemType.GUN:
            case ItemType.RIFLE:
                
                if(m_attackTiming == AttackTiming.SCRIPT_ONLY)
                    GunAttack(character);
                break;
            case ItemType.ROCKETLAUNCHER:
                if (m_attackTiming == AttackTiming.SCRIPT_ONLY)
                    RocketAttack(character);
                    break;
            case ItemType.MELEE:
            //    EnableMeleeColider();
                break;
            case ItemType.ETC_GRENADE:
              //  GrenadeAttack();
                break;
        }
    }

  

    public void AnimationEventAttack()
    {
        switch(m_type)
        {
            case ItemType.GUN:
            case ItemType.RIFLE:
                GunAttack(m_player);
                break;
            case ItemType.ROCKETLAUNCHER:
                RocketAttack(m_player);
                break;
            case ItemType.MELEE:
                AttackSword();
                break;
            case ItemType.ETC_GRENADE:
                GrenadeAttack();
                break;
        }
    }

    public void AnimationEventAttackEnd()
    {
        switch(m_type)
        {
            case ItemType.GUN:
            case ItemType.RIFLE:
                break;
            case ItemType.MELEE:
                // DisableMeleeColider();
                AttackSwordEnd();
                break;  
        }
    }

    #region Guns Weapon Attack
    void GunAttack(Transform character)
    {
        //if (AMMO <= 0)
        //{
        //    // 여기는 더이상 쏠 수 없다는 것을 알려야함
        //    return;
        //}

        int maxBullet = WeaponManager.Instance().GetWeaponData(ITEM_ID).Bulletcount;
        AMMO--;
        GameManager.Instance().UpdateWeapon(AMMO , maxBullet);

        SoundPlay();
        m_shotEffect.transform.position = m_firePoint.position;
        m_shotEffect.SetActive(true);
        
        string hostID = (NetworkManager.Instance() != null) ?  NetworkManager.Instance().m_hostID.ToString() : "1";
        string networkID = hostID + "_" + m_itemID + "_" + m_currentBulletIndex;
        
        WeaponManager.Instance().RequestBulletCreate(this,networkID ,m_itemID, m_firePoint.position ,
            character.localEulerAngles);
        m_currentBulletIndex++;
        Invoke("ShotEffectEnd" , m_coolTime - 0.1f);
    }

    void ShotEffectEnd()
    {
        m_shotEffect.SetActive(false);
    }

    void RocketAttack(Transform character)
    {
        //if (AMMO <= 0)
        //{
        //    // 여기는 더이상 쏠 수 없다는 것을 알려야함
        //    return;
        //}

        int maxBullet = WeaponManager.Instance().GetWeaponData(ITEM_ID).Bulletcount;
        AMMO--;
        GameManager.Instance().UpdateWeapon(AMMO , maxBullet);

        SoundPlay();
        if (m_shotEffect != null)
        {
            m_shotEffect.transform.position = m_firePoint.position;
            m_shotEffect.SetActive(true);
        }

        string hostID = (NetworkManager.Instance() != null) ? NetworkManager.Instance().m_hostID.ToString() : "1";
        string networkID = hostID + "_" + m_itemID + "_" + m_currentBulletIndex;
        Quaternion rot = character.rotation;
        WeaponManager.Instance().RequestBulletCreate(this,networkID , m_itemID , 
            m_firePoint.position , rot.eulerAngles);
        m_currentBulletIndex++;
        Invoke("ShotEffectEnd" , m_coolTime - 0.1f);
    }
    #endregion

    #endregion

    #region Equip Weapon Logic
    protected override void EquipItem()
    {
        if (GameManager.Instance() != null)
            GameManager.Instance().EquipWeapon(ITEM_ID , AMMO , WeaponManager.Instance().GetWeaponData(ITEM_ID).Bulletcount);
        this.GetComponent<SphereCollider>().enabled = false;
    }

    protected override void UnEquipItem()
    {
        if (GameManager.Instance() != null)
            GameManager.Instance().UnEquipWeapon(GameManager.Instance().PLAYER.m_player.CUR_EQUIP_INDEX);
        this.GetComponent<SphereCollider>().enabled = true;
    }
    #endregion

    #region MELEE_ATTACK
    // 신상 칼질
    public void AttackSword()
    {
        GameManager.Instance().m_inGameUI.ShowDebugLabel("AttackSword");
        m_swordAnimatorList[m_curSwordEffectIndex].gameObject.SetActive(true);
        //m_swordAnimatorList[m_curSwordEffectIndex].transform.parent = m_player;
        m_swordAnimatorList[m_curSwordEffectIndex].transform.SetParent(m_player.GetChild(0) , false);
        m_swordAnimatorList[m_curSwordEffectIndex].SetInteger("ATTACK" , 1);    
    }

    public void AttackSwordEnd()
    {
        GameManager.Instance().m_inGameUI.ShowDebugLabel("AttackSword End");
        m_swordAnimatorList[m_curSwordEffectIndex].SetInteger("ATTACK" , 0);
        //m_swordAnimatorList[m_curSwordEffectIndex].transform.parent = transform;
        m_swordAnimatorList[m_curSwordEffectIndex].transform.SetParent(transform , false);
        m_swordAnimatorList[m_curSwordEffectIndex].gameObject.SetActive(false);
    }
    #endregion

    #region ETC Weapons Attack
    
    protected void GrenadeAttack()
    {
        Grenade g = (this as Grenade);
        g.GRENADE_SHOT_ROT = m_player.transform.localRotation;
        g.Attack();
        GameManager.Instance().PLAYER.m_player.UnEquipGrenade(this);
        NetworkManager.Instance().RequestGrenadeCreate(g.ITEM_NETWORK_ID , g.transform.position);
    }


    #endregion
}
