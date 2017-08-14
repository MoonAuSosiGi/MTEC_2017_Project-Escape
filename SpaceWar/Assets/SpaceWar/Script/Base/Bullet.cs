using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

public class Bullet : MonoBehaviour {

    #region Bullet_INFO

    #region Network
    // 네트워크의 접속을 받는 놈인가
    private bool m_isRemote = false;
    // 네트워크 식별 아이디
    private string m_networkID = "";
    // 어떤놈의 총알인가
    private int m_itemCode = -1;
    public int ITEM_CODE { get { return m_itemCode; } set { m_itemCode = value; } }

    public bool IS_REMOTE { get { return m_isRemote; } set { m_isRemote = value; } }
    public string NETWORK_ID { get { return m_networkID; } set { m_networkID = value; } }

    private PositionFollower m_positionFollower = null;
    private AngleFollower m_angleFollowerX = null;
    private AngleFollower m_angleFollowerY = null;
    private AngleFollower m_angleFollowerZ = null;
    #endregion

    // 기본 정보들
    [SerializeField] private float m_speed = 0.0f;
    [SerializeField] private float m_duringTime = 0.0f;

    private UnityEngine.Vector3 m_prevPos = UnityEngine.Vector3.zero;

    //네트워크 총알 아닐 때 체크용
    private UnityEngine.Vector3 m_startPos = UnityEngine.Vector3.zero;
    private float m_tick = 0.0f;

    [SerializeField] private GameObject[] m_effects;
    [SerializeField] private GameObject m_bulletTrailEffect = null;
    [SerializeField] private GameObject m_shotEffect = null;
    [SerializeField] private GameObject m_shotOtherObjectEffect = null;

    private bool m_hitEnemy = false;
    private Quaternion m_shotRot;

    #endregion

    #region Network Method -----------------------------------------------------------------
    public void NetworkBulletEnable()
    {

        m_positionFollower = new PositionFollower();
        m_angleFollowerX = new AngleFollower();
        m_angleFollowerY = new AngleFollower();
        m_angleFollowerZ = new AngleFollower();
        
        this.GetComponent<SphereCollider>().enabled = false;

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
        m_positionFollower.SetTarget(pos , new Nettention.Proud.Vector3(0.0f , 0.0f , 0.0f));
        m_angleFollowerX.TargetAngle = 0.0f;
        m_angleFollowerY.TargetAngle = 0.0f;
        m_angleFollowerZ.TargetAngle = 0.0f;
        BulletSetup();
    }
    #endregion

    #region Unity Method -------------------------------------------------------------------------
      // Update is called once per frame
    void Update()
    {
        m_prevPos = transform.position;
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
    public void BulletSetup()
    {
        if (m_bulletTrailEffect != null)
            m_bulletTrailEffect.SetActive(true);
        if(!m_isRemote)
            this.GetComponent<SphereCollider>().enabled = true;
        else
            this.GetComponent<SphereCollider>().enabled = false;

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
        if (m_bulletTrailEffect != null)
            m_bulletTrailEffect.SetActive(false);
        if (!m_isRemote)
            this.GetComponent<SphereCollider>().enabled = false;
        
    }
    
    public void BulletMove()
    {
        // this.transform.Translate(Vector3.forward * Speed);
        //this.transform.rotation = AnchorPlanet.PlayerCharacter.rotation;

        this.transform.RotateAround(
            GravityManager.Instance().CurrentPlanet.transform.position , m_shotRot * UnityEngine.Vector3.right , 
            m_speed * Time.deltaTime);

        MoveSend();
    }

    void MoveSend()
    {
        UnityEngine.Vector3 velo = GetComponent<Rigidbody>().velocity;//(transform.position - m_prevPos) / Time.deltaTime;
        if(NetworkManager.Instance() != null)
        NetworkManager.Instance().C2SRequestBulletMove(m_networkID ,
            transform.position , velo , transform.localEulerAngles);
    }


  



    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Weapon"))
        {
            m_hitEnemy= true;

            
            if (other.CompareTag("PlayerCharacter"))
            {
                NetworkPlayer p = other.transform.GetComponent<NetworkPlayer>();

                if (p != null)
                {
                    if (m_shotEffect != null)
                        m_shotEffect.SetActive(true);
                    NetworkManager.Instance().C2SRequestPlayerDamage((int)p.m_hostID , p.m_userName , "test" , Random.Range(10.0f , 15.0f),m_startPos);
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
            }
            else
            {
                // 기타 오브젝트
                if (m_shotEffect != null)
                    m_shotEffect.SetActive(true);
            }

            //   Debug.Log(other.tag);

            BulletDelete();
            if(NetworkManager.Instance() != null)
                NetworkManager.Instance().C2SRequestBulletRemove(m_networkID);
        }

    }
    #endregion

}
