using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour {

    #region Grenade_INFO
    [SerializeField] private WeaponItem m_weaponInfo = null;
    [SerializeField] private float m_speed = 0.0f;
    [SerializeField] private GameObject m_effectBoom = null;
    [SerializeField] private GameObject m_effectBoom_Stone = null;
    [SerializeField] private float m_boomTime = 3.0f;
    [SerializeField] private GameObject m_grenadeRenderer = null;
    [SerializeField] private Rigidbody m_rigidBody = null;

    private bool m_isBoomTrigger = false;
    #region Network INFO
    private bool m_isNetwork = false;
    public bool IS_NETWORK { get { return m_isNetwork; } set { m_isNetwork = value; } }
    private string m_networkID = null;
    public string NETWORK_ID { get { return m_networkID; } set { m_networkID = value; } }

    private Nettention.Proud.PositionFollower m_positionFollower = null;
    private Nettention.Proud.AngleFollower m_angleFollowerX = null;
    private Nettention.Proud.AngleFollower m_angleFollowerY = null;
    private Nettention.Proud.AngleFollower m_angleFollowerZ = null;
    #endregion

    private float m_gravityPower = 0.0f;
    private Vector3 m_gravityPosition = Vector3.zero;
    private Quaternion m_shotRot;

    private bool m_isMoveAble = true;
    #endregion

    #region Unity Method
    void Update()
    {
        if (!m_isNetwork)
            GrenadeMove();
        else
            NetworkUpdate();
    }
    #endregion

    #region Main Logic
    public void GrenadeSetup(Quaternion shotRot)
    {
        m_gravityPosition = GravityManager.Instance().CurrentPlanet.transform.position;
        m_shotRot = shotRot;
        transform.SetParent(null , true);

        SphereCollider[] colls = this.GetComponents<SphereCollider>();

        // F 키
        colls[0].enabled = false;

        // 범위
        colls[1].enabled = true;
        
        // 바로터져야함
        //Invoke("GrenadeBoom" , m_boomTime);
    }

    void GrenadeMove()
    {
        if (m_isMoveAble == false)
            return;
        this.transform.RotateAround(GravityManager.Instance().CurrentPlanet.transform.position , m_shotRot * Vector3.right,
           m_speed * Time.deltaTime);


        Vector3 dir = (m_gravityPosition - transform.position).normalized;
        transform.position += dir * m_gravityPower * Time.deltaTime;

        m_gravityPower += 5f * Time.deltaTime;

       // transform.GetChild(0).Rotate(new Vector3(Random.Range(-360.0f , 360.0f) , Random.Range(-360.0f , 360.0f) , Random.Range(-360.0f , 360.0f)));

        MoveSend();
    }

    void MoveSend()
    {
        Vector3 velo = this.GetComponent<Rigidbody>().velocity;
        Debug.Log(" pos " +transform.position + " velo "+ velo);
        if (NetworkManager.Instance() == null)
            return;
        
        NetworkManager.Instance().RequestGrenadeMove(m_networkID , transform.position , velo , transform.GetChild(0).localEulerAngles);
    }

    void OnTriggerEnter(Collider col)
    {
        if (enabled == false)
            return;
        Debug.Log("Boom " + col.name + " tag "+col.tag + " boom ? "+m_effectBoom.activeSelf);

        if(!col.CompareTag("Weapon") && !col.CompareTag("Bullet"))
        {
            m_isMoveAble = false;

            if (col.CompareTag("PlayerCharacter"))
            {
                NetworkPlayer p = col.transform.GetComponent<NetworkPlayer>();
                
                if(p != null)
                {
                    if (!m_isBoomTrigger)
                        GrenadeBoom();
                    Debug.Log("Damage " + p.name);
                }
                else
                {
                    m_isMoveAble = true;
                }
            }
            else if(col.CompareTag("NonSpone"))
            {
                m_effectBoom_Stone.SetActive(true);
                if (!m_isBoomTrigger)
                    GrenadeBoomStone();
            }
            else
            {
                if (!m_isBoomTrigger)
                    GrenadeBoom();
            }
        }        
    }

    void GrenadeBoom()
    {
        m_effectBoom.SetActive(true);
        m_isMoveAble = false;

        m_grenadeRenderer.SetActive(false);
        m_isBoomTrigger = true;
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().RequestGrenadeBoom(m_networkID,false);
    }
    void GrenadeBoomStone()
    {
        m_isBoomTrigger = true;
        m_effectBoom_Stone.SetActive(true);
        m_isMoveAble = false;
        m_grenadeRenderer.SetActive(false);
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().RequestGrenadeBoom(m_networkID , true);
    }


    public void GrenadeBoomEnd()
    {
        gameObject.SetActive(false);
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().RequestGrenadeRemove(m_networkID);
    }
    #endregion

    #region Network Logic

    public void NetworkGrenadeEnable()
    {
        m_positionFollower = new Nettention.Proud.PositionFollower();
        m_angleFollowerX = new Nettention.Proud.AngleFollower();
        m_angleFollowerY = new Nettention.Proud.AngleFollower();
        m_angleFollowerZ = new Nettention.Proud.AngleFollower();

        SphereCollider[] colls = this.GetComponents<SphereCollider>();

        // F 키
        colls[0].enabled = false;

        // 범위
        colls[1].enabled = false;


    }

    public void NetworkGrenadeBoom(bool isStone)
    {
        if (isStone == false)
            m_effectBoom.SetActive(true);
        else
            m_effectBoom_Stone.SetActive(true);
        m_grenadeRenderer.SetActive(false);

    }

    public void NetworkMoveRecv(Vector3 pos, Vector3 velocity, Vector3 rot)
    {
        var npos = new Nettention.Proud.Vector3(pos.x , pos.y , pos.z);
        var nvel = new Nettention.Proud.Vector3(velocity.x , velocity.y , velocity.z);

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
        transform.position = new Vector3((float)p.x , (float)p.y , (float)p.z);

        float fx = (float)m_angleFollowerX.FollowerAngle * Mathf.Rad2Deg;
        float fy = (float)m_angleFollowerY.FollowerAngle * Mathf.Rad2Deg;
        float fz = (float)m_angleFollowerZ.FollowerAngle * Mathf.Rad2Deg;

        var rot = Quaternion.Euler(fx , fy , fz);
        transform.GetChild(0).localRotation = rot;

    }
    #endregion
}
