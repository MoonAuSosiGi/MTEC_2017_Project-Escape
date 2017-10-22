using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

public class Bullet : MonoBehaviour {

    #region Bullet_INFO

    #region Bullet Setting
    protected string m_weaponID = null;
    public string WEAPON_ID { get { return m_weaponID; } set { m_weaponID = value; } }

    protected float m_damage = 0.0f;
    public float DAMAGE { get { return m_damage; } set { m_damage = value; } }

    private bool m_alive = false;
    public bool IS_ALIVE { get { return m_alive; } set { m_alive = value; } }
    #endregion

    #region Network
    // 네트워크의 접속을 받는 놈인가
    protected bool m_isRemote = false;
    // 네트워크 식별 아이디
    protected string m_networkID = "";
    // 어떤놈의 총알인가
    private int m_itemCode = -1;
    public int ITEM_CODE { get { return m_itemCode; } set { m_itemCode = value; } }

    public bool IS_REMOTE { get { return m_isRemote; } set { m_isRemote = value; } }
    public string NETWORK_ID { get { return m_networkID; } set { m_networkID = value; } }

    protected PositionFollower m_positionFollower = null;
    protected AngleFollower m_angleFollowerX = null;
    protected AngleFollower m_angleFollowerY = null;
    protected AngleFollower m_angleFollowerZ = null;
    #endregion

    // 기본 정보들
    [SerializeField] protected float m_speed = 0.0f;
    public float SPEED { get { return m_speed; } set { m_speed = value; } }

    protected int m_targetID = -1;
    public int TARGET_ID
    {
        get { return m_targetID; }
        set { m_targetID = value; }
    }

    //네트워크 총알 아닐 때 체크용
    protected UnityEngine.Vector3 m_startPos = UnityEngine.Vector3.zero;
    private float m_tick = 0.0f;

    [SerializeField] protected GameObject m_bulletTrailEffect = null;
    [SerializeField] protected GameObject m_shotEffect = null;
    [SerializeField] protected GameObject m_shotOtherObjectEffect = null;

    public GameObject BULLET_TRAIL_EFFECT {  get { return m_bulletTrailEffect; }  set { m_bulletTrailEffect = value; } }
    public GameObject BULLET_HIT_EFFECT { get { return m_shotEffect; } set { m_shotEffect = value; } }
    public GameObject BULLET_OTHER_HIT_EFFECT { get { return m_shotOtherObjectEffect; } set { m_shotOtherObjectEffect = value; } }

    protected bool m_hitEnemy = false;
    protected Quaternion m_shotRot;
    public Quaternion SHOT_ROTATION { get { return m_shotRot; } }

    #region Sound Play ---------------------------------------------------------------------
    [SerializeField] protected AudioClip m_hitMain = null;
    [SerializeField] protected AudioClip m_hitSpaceShip = null;
    [SerializeField] protected AudioClip m_hitShelter = null;
    [SerializeField] protected AudioClip m_hitland = null;

    public AudioClip HIT_MAIN { get { return m_hitMain; } set { m_hitMain = value; } }
    public AudioClip HIT_SPACESHIP { get { return m_hitSpaceShip; } set { m_hitSpaceShip = value; } }
    public AudioClip HIT_SHELTER{ get { return m_hitShelter; } set { m_hitShelter = value; } }
    public AudioClip HIT_LAND { get { return m_hitland; } set { m_hitland = value; } }

    protected AudioSource m_bulletAudioSource = null;
    public AudioSource AUDIO_SOURCE { get { return m_bulletAudioSource; } set { m_bulletAudioSource = value; } }
    #endregion
    #endregion

    #region Network Method -----------------------------------------------------------------
    public void NetworkBulletEnable()
    {
        m_targetID = -1;
        m_positionFollower = new PositionFollower();
        m_angleFollowerX = new AngleFollower();
        m_angleFollowerY = new AngleFollower();
        m_angleFollowerZ = new AngleFollower();
        
        //this.GetComponent<SphereCollider>().enabled = false;

        if (m_bulletTrailEffect != null)
            m_bulletTrailEffect.SetActive(true);
        if (m_shotOtherObjectEffect != null)
            m_shotOtherObjectEffect.SetActive(false);
        if (m_shotEffect != null)
            m_shotEffect.SetActive(false);

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
    public void NetworkRemoveEvent()
    {
        BulletDelete();
    }

    public void NetworkReset(Nettention.Proud.Vector3 pos)
    {
        m_positionFollower = new PositionFollower();
        m_angleFollowerX = new AngleFollower();
        m_angleFollowerY = new AngleFollower();
        m_angleFollowerZ = new AngleFollower();
        m_targetID = -1;

        m_positionFollower.SetTarget(pos , new Nettention.Proud.Vector3(0.0f , 0.0f , 0.0f));
        m_angleFollowerX.TargetAngle = 0.0f;
        m_angleFollowerY.TargetAngle = 0.0f;
        m_angleFollowerZ.TargetAngle = 0.0f;
        transform.position = new UnityEngine.Vector3((float)pos.x , (float)pos.y , (float)pos.z);
        BulletSetup();
    }
    #endregion

    #region Unity Method -------------------------------------------------------------------------

    void Start()
    {
        m_bulletAudioSource = this.GetComponent<AudioSource>();
    }
      // Update is called once per frame
    void Update()
    {
        if (m_isRemote)
        {
            NetworkUpdate();
            return;
        }

        if (!m_hitEnemy)
        {
            BulletMove();
        }

    }
    #endregion

    #region Bullet Method --------------------------------------------------------------------------
    public virtual void BulletSetup()
    {
        if (m_bulletTrailEffect != null)
            m_bulletTrailEffect.SetActive(true);
        
        this.GetComponent<SphereCollider>().enabled = true;
        
        m_shotRot = GravityManager.Instance().GRAVITY_TARGET.transform.GetChild(0).rotation;
        m_startPos = GravityManager.Instance().GRAVITY_TARGET.transform.position;
        if (m_shotOtherObjectEffect != null)
            m_shotOtherObjectEffect.SetActive(false);
        if (m_shotEffect != null)
            m_shotEffect.SetActive(false);
        m_hitEnemy = false;
    }

    public void BulletDelete()
    {
        BulletEffectReset();
        if (m_bulletTrailEffect != null)
            m_bulletTrailEffect.SetActive(false);
        
        this.GetComponent<SphereCollider>().enabled = false;
        gameObject.SetActive(false);
        if (IS_REMOTE == false)
            WeaponManager.Instance().RequestBulletRemove(this);
    }
    
    public virtual void BulletMove()
    {
        UnityEngine.Vector3 velo = m_shotRot * UnityEngine.Vector3.right * m_speed * Time.deltaTime;
        
        this.transform.RotateAround(
            GravityManager.Instance().CurrentPlanet.transform.position , m_shotRot * UnityEngine.Vector3.right , 
            m_speed * Time.deltaTime);

        if(!m_isRemote)
            MoveSend(velo);
    }

    protected void MoveSend(UnityEngine.Vector3 velo)
    {
        
        if(NetworkManager.Instance() != null)
        NetworkManager.Instance().C2SRequestBulletMove(m_networkID ,
            transform.position , velo , transform.localEulerAngles);
    }


    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Weapon") && !other.CompareTag("Bullet") && !other.CompareTag("DeathZone"))
        {
            //Debug.Log("other " + other.name + " tag " + other.tag);
            m_hitEnemy= true;
            
            if (other.CompareTag("PlayerCharacter"))
            {
                NetworkPlayer p = other.transform.GetComponent<NetworkPlayer>();
                
                if (p != null)
                {
                    if (IS_REMOTE == true && TARGET_ID == (int)p.HOST_ID)
                        return;

                    if (m_shotEffect != null)
                        m_shotEffect.SetActive(true);

                    SoundPlay(m_hitMain);
                    if (IS_REMOTE == false)
                        NetworkManager.Instance().C2SRequestPlayerDamage((int)p.m_hostID , p.m_userName , m_weaponID , m_damage ,m_startPos);
                }
                else
                {
                    m_hitEnemy = false;
                    return;
                }
            }
            else if(string.IsNullOrEmpty(other.tag) || other.CompareTag("NonSpone"))
            {
                // 여기에 부딪치면 다른 이펙트를 보여준다.
                if (m_shotOtherObjectEffect != null)
                    m_shotOtherObjectEffect.SetActive(true);
                if(m_shotEffect != null)
                    m_shotEffect.SetActive(true);

                SoundPlay(m_hitland);


            }
            else
            {
                // 기타 오브젝트
                if (m_shotEffect != null)
                    m_shotEffect.SetActive(true);
                //        BULLET_TRAIL_EFFECT.SetActive(false);

                if (other.CompareTag("SpaceShipControlPanel"))
                    SoundPlay(m_hitSpaceShip);
                else if (other.CompareTag("Shelter"))
                    SoundPlay(m_hitShelter);
            }


            this.GetComponent<SphereCollider>().enabled = false;
            BulletHitEvent();
        }

    }

    void SoundPlay(AudioClip clip)
    {
        if(clip != null)
        {
            m_bulletAudioSource.clip = clip;
            m_bulletAudioSource.Play();
        }
    }

    // 파티클 등의 이펙트 처리용
    void BulletHitEvent()
    {
        var destroyTime = transform.GetComponentInChildren<Bullet_DestroyTime>();

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            Transform t = transform.GetChild(0).GetChild(i);

            var off = t.GetComponent<BulletParticle_Off>();
        
            var rateOff = t.GetComponent<BulletParticle_RateOff>();

            if (off != null)            off.BulletHitEvent();
            if (destroyTime != null)    destroyTime.BulletHitEvent();
            if (rateOff != null)        rateOff.BulletHitEvent();
        }
    }

    // 파티클 등의 이펙트 리셋
    void BulletEffectReset()
    {
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            Transform t = transform.GetChild(0).GetChild(i);

            var off = t.GetComponent<BulletParticle_Off>();
            var rateOff = t.GetComponent<BulletParticle_RateOff>();

            if (off != null)        off.Reset();
            if (rateOff != null)    rateOff.Reset();
        }
    }
    #endregion

}
