using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour {

    #region Death Zone INFO
    // 이친구는 생성 변환이 완료 된 후 데스존 하위에서 튕겨져나가 데스존의 이동 대상이 됨
    [SerializeField] private GameObject m_planetLevelZoneObject = null;
    //이펙트
    [SerializeField] private GameObject m_deathZoneEffect = null;
    

    private List<GameObject> m_deathZoneLines = new List<GameObject>();

    private string m_networkID = null;
    public string NETWORK_ID { get { return m_networkID; } set { m_networkID = value; } }

    // 이녀석이 움직이는 주체인지 
    private bool m_isMoveHost = true;
    public bool IS_MOVEHOST { get { return m_isMoveHost; } set { m_isMoveHost = value; } }

    // 몇번째 인덱스로 가ㅏ고 있는지
    private int m_deathLineIndex = 0;
    public int DEATH_LINE_INDEX { get { return m_deathLineIndex; } set { m_deathLineIndex = value; } }

    // 이동속도
    [SerializeField] private float[] m_deathZoneSpeed;
    [SerializeField] private Rigidbody m_rigidBody = null;

    // 
    Vector3 m_targetPos = Vector3.zero;
    Vector3 m_endDir = Vector3.zero;
    
    private Nettention.Proud.PositionFollower m_positionFollower = null;

    //테스트
    public GameObject m_test = null;

    [SerializeField] private Material m_shader1 = null;
    [SerializeField] private Material m_shader2 = null;
    
    #endregion

    #region Unity Method

    void Start()
    {
     
        if(m_test != null)
        DeathZoneSetup(m_test.transform.position);
        for(int i = 0; i < m_planetLevelZoneObject.transform.childCount; i++)
        {
            m_deathZoneLines.Add(m_planetLevelZoneObject.transform.GetChild(i).gameObject);
        }
    }
    void Update()
    {
        if (m_isMoveHost)
            DeathZoneMoveAndSend();
        else
            DeathZonNetworkMove();
    }

    #endregion

    #region Death Zone Method
    public void DeathZoneSetup(Vector3 pos)
    {
        // 반대 방향으로 생성
        transform.rotation = Quaternion.LookRotation((Vector3.zero - pos).normalized);
        Vector3 r = transform.eulerAngles;
        transform.eulerAngles = new Vector3(r.x + 90.0f , r.y , r.z);
        m_positionFollower = new Nettention.Proud.PositionFollower();
        m_targetPos = pos;
        m_endDir = (m_targetPos - transform.position).normalized;

    }

    void DeathZoneMoveAndSend()
    {
        Vector3 pos = transform.GetChild(0).position;

        if (m_deathLineIndex >= m_deathZoneLines.Count)
        {
            Debug.Log("tt" + Vector3.Distance(pos , m_targetPos));
            if (Vector3.Distance(pos , m_targetPos) >= 10.0f)
                return;
            

            transform.GetChild(0).position += m_endDir * m_deathZoneSpeed[m_deathZoneLines.Count-1] * Time.deltaTime;
            return;
        }
        

        Vector3 dir = (m_deathZoneLines[m_deathLineIndex].transform.position - pos).normalized;
        
        transform.GetChild(0).position += dir * m_deathZoneSpeed[m_deathLineIndex] * Time.deltaTime;

        if(Vector3.Distance(pos,m_deathZoneLines[m_deathLineIndex].transform.position) <= 1.0f)
        {
            m_deathLineIndex++;
        }

        // moveSend
        if(NetworkManager.Instance() != null)
        {
            NetworkManager.Instance().NotifyDeathZoneMove(transform.GetChild(0).position , m_rigidBody.velocity);
        }
    }

    void DeathZonNetworkMove()
    {
        if(m_positionFollower == null)
            m_positionFollower = new Nettention.Proud.PositionFollower();
        m_positionFollower.FrameMove(Time.deltaTime);

        var p = new Nettention.Proud.Vector3();
        var vel = new Nettention.Proud.Vector3();
        m_positionFollower.GetFollower(ref p , ref vel);
        transform.GetChild(0).position = new Vector3((float)p.x , (float)p.y , (float)p.z);
    }

    public void NetworkMoveRecv(Vector3 pos,Vector3 velocity)
    {
        m_positionFollower.SetTarget(new Nettention.Proud.Vector3(pos.x , pos.y , pos.z) , new Nettention.Proud.Vector3(velocity.x , velocity.y , velocity.z));
    }
    
    #endregion

    #region Death Zone Collision

    public void DeathZoneHit(GameObject hitPlayer) //ContactPoint[] hitPoints)
    {
        GameObject hitEffect = GameObject.Instantiate(m_deathZoneEffect) as GameObject;
        hitEffect.transform.parent = transform;
        
        hitEffect.transform.position = hitPlayer.transform.GetChild(4).position;
        hitEffect.SetActive(true);
        

        if(hitPlayer.GetComponent<NetworkPlayer>() == null && hitPlayer.GetComponent<PlayerController>()  != null && hitPlayer.GetComponent<PlayerController>().enabled == true)
        {
            if (NetworkManager.Instance() != null)
                NetworkManager.Instance().C2SRequestPlayerDamage((int)NetworkManager.Instance().m_hostID , "" , "DeathZone" , 5.0f , Vector3.zero);
        }
    }


    #endregion
}
