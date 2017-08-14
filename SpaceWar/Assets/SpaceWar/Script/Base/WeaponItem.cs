using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item {

    #region WeaponItem INFO
    public enum WeaponType
    {
        NONE,
        GUN,
        RIFLE,
        MELEE,
        ROCKETLAUNCHER,
        SKILLWEAPON
    }

    [SerializeField] private WeaponType m_type = WeaponType.NONE;
    [SerializeField] private Vector3 m_localSetPos = Vector3.zero;
    [SerializeField] private Vector3 m_localSetRot = Vector3.zero;
    [SerializeField] private Vector3 m_localSetScale = Vector3.zero;
    [SerializeField] private Vector3 m_sponeRotation = Vector3.zero;

    public enum AttackTiming
    {
        SCRIPT_ONLY,
        ANIMATION_ONLY
    }
    [SerializeField] private AttackTiming m_attackTiming = AttackTiming.SCRIPT_ONLY;
    public AttackTiming ATTACK_TIMING { get { return m_attackTiming; } }
    private Transform m_player = null;

    public WeaponType WEAPON_TYPE { get { return m_type; } }
    public Vector3 LOCAL_SET_POS { get { return m_localSetPos; } set { m_localSetPos = value; } }
    public Vector3 LOCAL_SET_ROT { get { return m_localSetRot; } set { m_localSetRot = value; } }
    public Vector3 LOCAL_SET_SCALE { get { return m_localSetScale; } set { m_localSetScale = value;} }
    public Vector3 SPONE_ROTATITON { get { return m_sponeRotation; } set { m_sponeRotation = value; } }

    // 무기 쿨타임
    [SerializeField] private float m_coolTime = 0.0f;

    public float COOL_TIME { get { return m_coolTime; } set { m_coolTime = value; } }

    // 원거리 무기의 경우 총알로 사용
    [SerializeField] private GameObject[] m_bullets = null;
    [SerializeField] private GameObject[] m_effects = null;


    private int m_currentBulletIndex = 0;

    // 총알 나가는 위치
    [SerializeField] private Transform m_firePoint = null;
    public Transform FIRE_POS { get { return m_firePoint; } set { m_firePoint = value; } }
    
    #endregion

    #region Weapon Method
    public void Attack(Transform character)
    {
        switch(m_type)
        {
            case WeaponType.GUN:
            case WeaponType.RIFLE:
                m_player = character;
                if(m_attackTiming == AttackTiming.SCRIPT_ONLY)
                    GunAttack(character);
                break;
        }
    }

  

    public void AnimationEventAttack()
    {
        switch(m_type)
        {
            case WeaponType.GUN:
            case WeaponType.RIFLE:
                GunAttack(m_player);
                break;
        }
    }

    void GunAttack(Transform character)
    {
        m_effects[0].transform.position = m_firePoint.position;
        m_effects[0].SetActive(true);

        GameObject bullet = m_bullets[m_currentBulletIndex];
        bullet.transform.parent = null;
        bullet.transform.position = m_firePoint.position;
        bullet.transform.rotation = character.rotation;
        Bullet b = bullet.GetComponent<Bullet>();
      
        // 네트워크 총알이 아니므로
        b.IS_REMOTE = false;
        
        if (NetworkManager.Instance() != null)
        {
            // 식별 아이디
            b.NETWORK_ID = NetworkManager.Instance().m_hostID + "_" + m_itemID + "_" + m_currentBulletIndex;
            // 생성 명령
            NetworkManager.Instance().C2SRequestBulletCreate(b.NETWORK_ID , m_firePoint.position , bullet.transform.localEulerAngles,m_itemID);
        }
        bullet.SetActive(true);
        b.BulletSetup();

        if (m_currentBulletIndex == m_bullets.Length - 1)
            m_currentBulletIndex = 0;
        else
            m_currentBulletIndex++;
    }



    #endregion

    #region Equip Weapon Logic
    public void EquipWeapon()
    {
        this.GetComponent<Collider>().enabled = false;
    }

    public void UnEquipWeapon()
    {
        this.GetComponent<Collider>().enabled = true;
    }
    #endregion
}
