using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention.Proud;

public class Gun01Bullet : MonoBehaviour {

    
    public float Speed;
    public float DuringTime;
    public string m_BulletType = "Gun01Bullet";
    private UnityEngine.Vector3 m_prevPos = UnityEngine.Vector3.zero;

   
    public GameObject[] Effect;
    public bool HitEnemy;

    public Quaternion ShotRotation;
    public bool IsRemote;


    public int WeaponID;
    public int Mynum;

    #region Network
    public string m_bulletID = "";
    public bool m_networkBullet = false;

    public bool NETWORK_BULLET
    {
        get { return m_networkBullet; }
        set
        {
            m_networkBullet = value;
            if (m_networkBullet)
                NetworkBulletEnable();
        }
    }

    PositionFollower m_positionFollower = null;
    AngleFollower m_angleFollowerX = null;
    AngleFollower m_angleFollowerY = null;
    AngleFollower m_angleFollowerZ = null;

    

    void NetworkBulletEnable()
    {
        //if(m_positionFollower == null)
        {
            m_positionFollower = new PositionFollower();
            m_angleFollowerX = new AngleFollower();
            m_angleFollowerY = new AngleFollower();
            m_angleFollowerZ = new AngleFollower();
        }
        

        this.GetComponent<SphereCollider>().enabled = false;
        Effect[1].SetActive(true);
        Effect[0].SetActive(false);
    }

    public void NetworkMoveRecv(UnityEngine.Vector3 pos, UnityEngine.Vector3 velocity, UnityEngine.Vector3 rot)
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

    #endregion

    private void OnEnable()
    {
        //AnchorPlanet.PlanetAnchor.rotation = this.transform.rotation;
        //this.transform.parent = AnchorPlanet.PlanetAnchor;
        if (m_networkBullet)
            return;
        if (IsRemote)
        {
            ShotRotation = this.transform.rotation;
            this.GetComponent<SphereCollider>().enabled = false;
        }
        else
        {
            ShotRotation = AnchorPlanet.PlayerCharacter.rotation;
            this.GetComponent<SphereCollider>().enabled = true;
        }



        Effect[1].SetActive(true);
        HitEnemy = false;
        StartCoroutine(SetTime());


       
    }

    // Update is called once per frame
    void Update () {

        if (m_networkBullet)
        {
            NetworkUpdate();
            return;
        }
        m_prevPos = transform.position;
        if (!HitEnemy)
        {
            BulletMove();
        }

    }

    public void BulletMove()
    {
        // this.transform.Translate(Vector3.forward * Speed);
        //this.transform.rotation = AnchorPlanet.PlayerCharacter.rotation;
        
        this.transform.RotateAround(AnchorPlanet.Planet.position, ShotRotation * UnityEngine.Vector3.right, Speed);

        UnityEngine.Vector3 velo = (transform.position - m_prevPos) / Time.deltaTime;
        NetworkManager.Instance().C2SRequestBulletMove(m_bulletID ,
            transform.position ,velo, transform.localEulerAngles);


    }

    IEnumerator SetTime()
    {
        yield return new WaitForSeconds(DuringTime);


        this.gameObject.SetActive(false);
        StopCoroutine(SetTime());


        this.GetComponent<SphereCollider>().enabled = false;

    }

    public void HitCallReturn(UnityEngine.Vector3 Pos)
    {

        HitEnemy = true;
        this.transform.position = Pos;
        Effect[1].SetActive(false);
        Effect[0].SetActive(true);


        this.GetComponent<SphereCollider>().enabled = false;
    }



    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Weapon"))
        {
            HitEnemy = true;

            Effect[1].SetActive(false);
            Effect[0].SetActive(true);
  

            if (other.CompareTag("PlayerCharacter"))
            {
                // TODO WEAPON
                //   AnchorPlanet.PlayerCharacter.GetComponent<TestStram>().SenddataCall("Hitcall01/" + WeaponID + "/" + Mynum + "/" + other.gameObject.GetComponents<PhotonView>()[0].viewID + "/" + this.transform.position.x + "/" + this.transform.position.y + "/" + this.transform.position.z);               
            }
            else
            {
                // TODO WEAPON
                //AnchorPlanet.PlayerCharacter.GetComponent<TestStram>().SenddataCall("Hitcall01/" + WeaponID + "/" + Mynum + "/" + this.transform.position.x + "/" + this.transform.position.y + "/" + this.transform.position.z);
            }

            Debug.Log(other.tag);

            this.GetComponent<SphereCollider>().enabled = false;
        }

    }

}
