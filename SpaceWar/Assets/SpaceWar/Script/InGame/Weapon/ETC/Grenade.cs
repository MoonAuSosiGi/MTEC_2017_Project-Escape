using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

public class Grenade : WeaponItem {

    #region Grenade INFO
  
    #region Network
    // 네트워크 세팅
    private bool m_isNetwork = false;
    public bool IS_NETWORK { get { return m_isNetwork; } set { m_isNetwork = value; } }

    protected PositionFollower m_positionFollower = null;
    protected AngleFollower m_angleFollowerX = null;
    protected AngleFollower m_angleFollowerY = null;
    protected AngleFollower m_angleFollowerZ = null;

    private UnityEngine.Vector3 m_startPos = UnityEngine.Vector3.zero;
    #endregion

    #region Effect Setting
    private GameObject m_grenadeBaseHitEffect = null;
    private GameObject m_grenadeOtherHitEffect = null;

    public GameObject GRENADE_BASE_HIT { get { return m_grenadeBaseHitEffect; } set { m_grenadeBaseHitEffect = value; } }
    public GameObject GRENADE_OTHER_HIT { get { return m_grenadeOtherHitEffect; } set { m_grenadeOtherHitEffect = value; } }
    #endregion
    
    // 수류탄의 Gravity
    // 실제로 더해지는 값
    private float m_gravityPower = 5.0f;
    private float m_gravity = 0.0f;
    //수류탄 속도
    private float m_grenadeSpeed = 40.0f;
    public float GRENADE_SPEED { get { return m_grenadeSpeed; } set { m_grenadeSpeed = value; } }
    public float GRAVITY_POWER { get { return m_gravityPower; } set { m_gravityPower = value; } }
    
    //발사 기준회전 값 ( 캐릭터 )
    Quaternion m_shotRot;
    public Quaternion GRENADE_SHOT_ROT { get { return m_shotRot; } set { m_shotRot = value; } }

    //Grenade 발사 
    private bool m_isGrenadeStart = false;
    //Grenade 도착
    private bool m_isShot = false;

    // 리지드봐디
    Rigidbody m_rigidbody = null;
    #endregion

    #region Unity Method
    void Start()
    {
        m_rigidbody = gameObject.AddComponent<Rigidbody>();
        m_rigidbody.isKinematic = true;
    }

    void FixedUpdate()
    {
        if (m_isGrenadeStart && m_isShot == false)
            GrenadeMove();
        if (m_isNetwork && m_isShot == false)
            NetworkUpdate();
    }
    #endregion

    #region Grenade Function

    public void Attack()
    {
        if (m_isGrenadeStart)
            return;

        // 콜라이더들 얻기
        SphereCollider[] colls = this.GetComponents<SphereCollider>();
        // 수류탄 공격 전에 아이템을 얻는 콜라이더를 끈다.
        colls[0].enabled = false;

        //// 수류탄 자체 콜라이더는 켠다
        colls[1].enabled = true;
        
        // 공격 명령
        m_isGrenadeStart = true;

        m_isNetwork = false;

        transform.parent = null;

    }

    void GrenadeMove()
    {
        if (m_isGrenadeStart == false || m_isNetwork == true)
            return;

        this.transform.RotateAround(GravityManager.Instance().CurrentPlanet.transform.position , m_shotRot * UnityEngine.Vector3.right ,
           m_grenadeSpeed * Time.deltaTime);


        UnityEngine.Vector3 dir = (GravityManager.Instance().CurrentPlanet.transform.position - transform.position).normalized;
        transform.position += dir * m_gravity * Time.deltaTime;

        m_gravity += m_gravityPower * Time.deltaTime;

        GrenadeMoveSend();
       
    }

    void GrenadeMoveSend()
    {
        // 네트워크 샌드
        NetworkManager.Instance().RequestGrenadeMove(ITEM_NETWORK_ID , transform.position , m_rigidbody.velocity , transform.localScale);
    }

    void GrenadeDelete()
    {
        // 비활성화
        gameObject.SetActive(false);

        if(m_isNetwork)
        {
            // 삭제 요청을 보낸다
            NetworkManager.Instance().C2SRequestItemDelete(m_itemNetworkID);
        }
        
    }
    #endregion

    #region Grenade Network Logic
    public void NetworkGrenadeEnable()
    {
        if (m_positionFollower != null)
            return;
        m_positionFollower = new PositionFollower();
        m_angleFollowerX = new AngleFollower();
        m_angleFollowerY = new AngleFollower();
        m_angleFollowerZ = new AngleFollower();
        m_isNetwork = true;

        // 콜라이더들 얻기
        SphereCollider[] colls = this.GetComponents<SphereCollider>();
        // 수류탄 공격 전에 아이템을 얻는 콜라이더를 끈다.
        colls[0].enabled = false;

        // 수류탄 자체 콜라이더는 켠다
        colls[1].enabled = true;
        
        transform.parent = null;
    }

    public void NetworkMoveRecv(UnityEngine.Vector3 pos , UnityEngine.Vector3 velocity , UnityEngine.Vector3 rot)
    {
        var npos = new Nettention.Proud.Vector3();
        npos.x = pos.x;
        npos.y = pos.y;
        npos.z = pos.z;

        var nvel = new Nettention.Proud.Vector3();
        nvel.x = velocity.x;
        nvel.y = velocity.y;
        nvel.z = velocity.z;

        m_positionFollower.SetTarget(npos , nvel);

        m_angleFollowerX.TargetAngle = rot.x * Mathf.Deg2Rad;
        m_angleFollowerY.TargetAngle = rot.y * Mathf.Deg2Rad;
        m_angleFollowerZ.TargetAngle = rot.z * Mathf.Deg2Rad;
    }

    void NetworkUpdate()
    {
        if (m_positionFollower == null || m_angleFollowerX == null || m_angleFollowerY == null || m_angleFollowerZ == null)
            return;
        GameManager.Instance().m_inGameUI.ShowDebugLabel("Network Update Grenade ");
        Debug.Log("Network Update Grenade ");
        m_positionFollower.FrameMove(Time.deltaTime);
        m_angleFollowerX.FrameMove(Time.deltaTime);
        m_angleFollowerY.FrameMove(Time.deltaTime);
        m_angleFollowerZ.FrameMove(Time.deltaTime);

        m_angleFollowerX.FollowerAngleVelocity = 200 * Time.deltaTime;
        m_angleFollowerY.FollowerAngleVelocity = 200 * Time.deltaTime;
        m_angleFollowerZ.FollowerAngleVelocity = 200 * Time.deltaTime;

        var p = new Nettention.Proud.Vector3();
        var vel = new Nettention.Proud.Vector3();

        m_positionFollower.GetFollower(ref p , ref vel);
        transform.position = new UnityEngine.Vector3((float)p.x , (float)p.y , (float)p.z);

        float fx = (float)m_angleFollowerX.FollowerAngle * Mathf.Rad2Deg;
        float fy = (float)m_angleFollowerY.FollowerAngle * Mathf.Rad2Deg;
        float fz = (float)m_angleFollowerZ.FollowerAngle * Mathf.Rad2Deg;
        var rotate = Quaternion.Euler(fx , fy , fz);
        transform.localRotation = rotate;
    }
    #endregion

    #region Collision
    void OnTriggerEnter(Collider other)
    {
        if (m_isGrenadeStart == false && m_isNetwork == false)
            return;
        else if (m_isShot)
            return;

        // 콜라이더들 얻기
        SphereCollider[] colls = this.GetComponents<SphereCollider>();

        if (!other.CompareTag("Weapon") && !other.CompareTag("Bullet") && !other.CompareTag("DeathZone"))
        {
            m_isShot = true;
            Debug.Log("other " + other.name + " tag " + other.tag);

            if (other.CompareTag("PlayerCharacter"))
            {
                NetworkPlayer p = other.transform.GetComponent<NetworkPlayer>();

                if (p != null)
                {
                    Debug.Log("Grenade :: " + TARGET_HOST_ID + " p " + p.HOST_ID);
                    if (m_isNetwork == true && TARGET_HOST_ID == (int)p.HOST_ID)
                    {
                        m_isShot = false;
                        return;
                    }
                    BaseHitEffect();
                }
                else
                {
                    return;
                }
            }
            else if (string.IsNullOrEmpty(other.tag) || other.CompareTag("NonSpone"))
            {
                // 여기에 부딪치면 다른 이펙트를 보여준다.
                BaseHitEffect();
                OtherHitEffect();
            }
            else
            {
                // 기타 오브젝트
                BaseHitEffect();
            }
            transform.GetChild(0).gameObject.SetActive(false);
            colls[0].enabled = false;
            colls[1].enabled = false;
            Invoke("GrenadeDelete" , 1.5f);
        }
    }

    public void BoomPlayer(NetworkPlayer p)
    {
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerDamage((int)p.HOST_ID , p.m_userName , this.ITEM_NAME , this.DAMAGE , transform.position);
    }

    void BaseHitEffect()
    {
        if(m_grenadeBaseHitEffect != null)
        {
            m_grenadeBaseHitEffect.GetComponent<SphereCollider>().enabled = true;
//            m_grenadeBaseHitEffect.transform.parent = null;
            m_grenadeBaseHitEffect.SetActive(true);
        }
    }

    void OtherHitEffect()
    {
        if(m_grenadeOtherHitEffect != null)
        {
         //   m_grenadeOtherHitEffect.GetComponent<SphereCollider>().enabled = true;
            m_grenadeOtherHitEffect.SetActive(true);
        }
    }
    #endregion
}
