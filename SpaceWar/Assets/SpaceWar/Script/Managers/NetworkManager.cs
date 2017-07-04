using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention;
using Nettention.Proud;
using SpaceWar;

public class NetworkManager : Singletone<NetworkManager> {

    #region NetworkManager_INFO
    //-- NetworkManager ---------------------------------------------------------------------//
    public HostID m_hostID;
    public HostID m_p2pID;

    // 서버 guid 
    System.Guid m_protocolVersion = new System.Guid("{0xa1152276,0xe4a,0x416c,{0x97,0xba,0xa1,0x1a,0x3e,0xd0,0xea,0x55}}");

    // 서버 포트
    int m_serverPort = 54532;

    // 서버 아이피
    private string m_serverIP = "localhost";

    // 프라우드넷 클라이언트 객체
    private NetClient m_netClient = null;
    // 서버로 RMI 메시지 전송을 위해 C2S Proxy 객체 선언
    private Proxy m_c2sProxy = null;


    //서버로부터 RMI 메시지를 받기 위한 S2C Stub 객체
    private Stub m_s2cStub = null;

    // 서버 접속 정보
    private Nettention.Proud.NetConnectionParam m_param;

    // 서버 연결 유무
    private bool m_isConnect = false;
    private bool m_isLogin = false;

    public bool LOGIN_STATE {  get { return m_isLogin; } }
    public bool CONNECT_STATE { get { return m_isConnect; } }

    private List<NetworkPlayer> m_players = new List<NetworkPlayer>();

    public List<NetworkPlayer> NETWORK_PLAYERS
    {
        get { return m_players; }
    }


    public Dictionary<int , GameObject> m_networkItemList = new Dictionary<int , GameObject>();

    public GameObject GetNetworkItem(int itemID)
    {
        if (m_networkItemList.ContainsKey(itemID))
            return m_networkItemList[itemID];
        else
            return null;
    }

    // 추후 Bullet 으로 변경할 것
    public Dictionary<string,Gun01Bullet> m_bulletList = new Dictionary<string , Gun01Bullet>();
    public Gun01Bullet GetNetworkBullet(string bulletID)
    {
        if (m_bulletList.ContainsKey(bulletID))
            return m_bulletList[bulletID];
        else
            return null;
    }

    #endregion

    #region UnityMethod
    void Start()
    {
        // 관련 클래스 생성 -----------------------------------//

        // 클라이언트 생성
        m_netClient = new NetClient();

        // 프록시 생성
        m_c2sProxy = new Proxy();

        // Stub 생성
        m_s2cStub = new Stub();

        #region Network Event Delegate Setting
        // 클라이언트 이벤트 델리게이트 세팅-------------------------//
        m_netClient.JoinServerCompleteHandler = OnJoinServerComplete;
        m_netClient.LeaveServerHandler = OnLeaveServer;
        m_netClient.P2PMemberJoinHandler = OnP2PMemberJoin;
        m_netClient.P2PMemberLeaveHandler = OnP2PMemberLeave;
        #endregion

        #region Connect Info Setup
        // S2C Stub RMI 함수 델리게이트 세팅 ------------------------//

        // 클라이언트에 Proxy 와 Stub 을 붙임 -----------------------//
        m_netClient.AttachStub(m_s2cStub);
        m_netClient.AttachProxy(m_c2sProxy);

        // 접속 정보를 세팅 -----------------------------------------//
        m_param = new NetConnectionParam();

        // 프로토콜 버전

        m_param.protocolVersion = new Nettention.Proud.Guid();
        m_param.protocolVersion.Set(m_protocolVersion);
        m_param.serverPort = (ushort)m_serverPort;
        #endregion

        #region Stub Setup
        // 로그인 성공시
        m_s2cStub.NotifyLoginSuccess = (HostID remote, RmiContext rmiContext,int hostid) =>
        {
            m_isLogin = true;
            m_hostID = (HostID)hostid;
            NetworkLog("Login Success " + (int)hostid);

            GameManager.Instance().OnJoinedRoom(GameManager.Instance().PLAYER.m_name , true ,
            new UnityEngine.Vector3(0.0f , 80.0f , 0.0f));
            return true;
        };

        // 로그인 실패시
        m_s2cStub.NotifyLoginFailed = (HostID remote ,RmiContext rmiContext , System.String reason) =>
        {
            NetworkLog("Login Failed " + reason);
            return true;
        };
        
        // 플레이어가 나갔을 때
        m_s2cStub.NotifyPlayerLost = (HostID remote , RmiContext rmiContext , int hostID) =>
        {
            NetworkLog("Player Lost .. " + hostID);

            NetworkPlayer target = null;
            foreach(NetworkPlayer p in m_players)
            {
                if(p.m_hostID == (HostID)hostID)
                {
                    target = p;
                    break;
                }
            }

            GameObject.Destroy(target.gameObject);
            m_players.Remove(target);
            return true;
        };

        // 네트워크 플레이어의 이동 메시지가 왔다.
        m_s2cStub.NotifyPlayerMove = (HostID remote , RmiContext rmiContext ,
            int sendHostID , string name , float curX , float curY , float curZ ,
            float velocityX , float velocityY , float velocityZ,
            float crx,float cry,float crz,
            float rx, float ry, float rz) =>
        {
            foreach (NetworkPlayer p in m_players)
            {
                if (p.m_userName.Equals(name))
                {
                    p.RecvNetworkMove(new UnityEngine.Vector3(curX , curY , curZ) ,
                       new UnityEngine.Vector3(velocityX , velocityY , velocityZ),
                       new UnityEngine.Vector3(crx,cry,crz),
                       new UnityEngine.Vector3(rx,ry,rz));
                    return true;
                }
            }
            return true;
        };

        // 네트워크 플레이어의 애니메이션 메시지가 왔다.
        m_s2cStub.NotifyPlayerAnimation = (HostID remote , RmiContext rmiContext ,
            int sendHostID , string name , string animationName , int aniValue) =>
        {
            foreach (NetworkPlayer p in m_players)
            {
                if (p.m_userName.Equals(name))
                {
                    p.RecvNetworkAnimation(animationName , aniValue);
                    return true;
                }
            }
            return true;
        };

        // 다른 클라이언트 들어왔을 때 첫 생성
        m_s2cStub.NotifyOtherClientJoin = (HostID remote , RmiContext rmiContext ,
            int hostID , string name , float x , float y , float z) =>
        {
            NetworkLog("Other Client Join " + name + " host "+m_hostID + " h " +hostID );
            if (m_hostID == (HostID)hostID)
                return true;
            
            GameObject MP = GameManager.Instance().OnJoinedRoom(name , false,
                 new UnityEngine.Vector3(0.0f,80.0f,0.0f));
            MP.GetComponent<NetworkPlayer>().NetworkPlayerSetup((HostID)hostID , name);
            return true;
        };

        // 아이템 생성 로직
        m_s2cStub.NotifyCreateItem = (HostID remote , RmiContext rmiContext , int sendHostID ,
            int itemCID,int itemID , UnityEngine.Vector3 pos , UnityEngine.Vector3 rot) =>
        {
            NetworkLog("NotifyCreateItem .." + itemCID);
            
            // 아이템 등록 
            m_networkItemList.Add(itemID,GameManager.Instance().CommandItemCreate(itemCID,itemID , pos , rot));
            return true;
        };

        // 아이템을 장비했다 ( 다른 플레이어 )
        m_s2cStub.NotifyPlayerEquipItem = (HostID remote , RmiContext rmiContext , int hostID ,
            int itemCID , int itemID) =>
        {
            
            GameObject item = GetNetworkItem(itemID);
            NetworkLog("NotifyPlayerEquipItem " + itemID + " null ? " +(item==null));

            if (item != null)
            {
                foreach(var p in m_players)
                {
                    if((int)p.m_hostID == hostID)
                    {
                        p.EquipWeapon(item);
                        break;
                    }
                }
            }

            return true;
        };

        // 아이템을 해제했다 ( 다른 플레이어 )
        m_s2cStub.NotifyPlayerUnEquipItem = (HostID remote , RmiContext rmiContext , int hostID , int itemCID ,
            int itemID , UnityEngine.Vector3 pos , UnityEngine.Vector3 rot) =>
        {
            GameObject item = GetNetworkItem(itemID);

            if (item != null)
            {
                foreach (var p in m_players)
                {
                    p.UnEquipWeapon(pos,rot);
                    break;
                }
            }
                return true;
        };

        // 총알 생성해라
        m_s2cStub.NotifyPlayerBulletCreate = (HostID remote , RmiContext rmiContext , 
            int sendHostID , string bulletType , string bulletID , 
            UnityEngine.Vector3 pos , UnityEngine.Vector3 rot) =>
        {
            NetworkLog("Bullet Create " + sendHostID + " t " + (int)m_hostID);

            Gun01Bullet b = GetNetworkBullet(bulletID);

            if(b != null)
            {
                // 이미 생성되어 있는 것
                // 재활용ㅋ
                b.enabled = true;
                b.transform.position = pos;
                b.transform.eulerAngles = rot;
            }
            else
            {
                // 타입 검사 후 생성하는 로직이 들어가야함 ( 지금은 일단 하나니까 ..)
                GameObject bullet = GameObject.Instantiate(Resources.Load("Bullet/Gun01Bullet")) as GameObject;
                
                bullet.transform.parent = transform;
                bullet.transform.position = pos;
                bullet.transform.localEulerAngles = rot;
                b = bullet.GetComponent<Gun01Bullet>();
                b.m_bulletID = bulletID;
                b.NETWORK_BULLET = true;
                b.enabled = true;
                bullet.SetActive(true);
                m_bulletList.Add(bulletID , b);
            }


            return true;
        };

        // 총알 이동
        m_s2cStub.NotifyPlayerBulletMove = (HostID remote , RmiContext rmiContext , 
            int sendHostID , string bulletID , UnityEngine.Vector3 pos, UnityEngine.Vector3 velocity, UnityEngine.Vector3 rot) =>
        {
            Gun01Bullet b = GetNetworkBullet(bulletID);

            if (b == null)
                NetworkLog("ERROR bulletID 미등록 " + bulletID);
            else
            {
                b.NetworkMoveRecv(pos , velocity , rot);
            }
            return true;
        };

        #endregion

        ServerConnect();
    }

    void Update()
    {
        if (m_netClient != null)
            m_netClient.FrameMove();
    }
    #endregion

    #region Server Control Method
    public void ServerConnect()
    {
        m_param.serverIP = m_serverIP;
        m_netClient.Connect(m_param);
        
    }
    #endregion

    #region EventDelegate
    void OnJoinServerComplete(ErrorInfo info , ByteArray replyFromServer)
    {
        // 성공적으로 연결 되면.
        if (Nettention.Proud.ErrorType.ErrorType_Ok == info.errorType)
        {

            Debug.Log("Server Connected");
            
            GameManager.Instance().PLAYER.m_name = "TEST " + Random.Range(1 , 100);
            C2SRequestLogin(GameManager.Instance().PLAYER.m_name);
            m_isConnect = true; // bool 변수 값 true 로 변경합니다.
          
        }
        else
        {
            // 에러처리를 합니다.
            Debug.Log("Server connection failed.");
            Debug.Log(info.ToString());
        }
    }
    // 서버와 연결이 해제 되면 콜백됩니다.
    void OnLeaveServer(Nettention.Proud.ErrorInfo info)
    {
        if (Nettention.Proud.ErrorType.ErrorType_Ok != info.errorType)
        {
            // 에러입니다.
            Debug.Log("OnLeaveServer. " + info.ToString());
        }

        Debug.Log("Server Disconnected");
        // 서버와의 연결이 종료되었으니 초기화면으로 나가거나, 다른처리를 해주어야 하빈다.
        m_isConnect = false;

    }

    // p2p member join
    void OnP2PMemberJoin(HostID memberHostID , HostID groupHostID , int memberCount , ByteArray customField)
    {
        m_p2pID = groupHostID;
        Debug.Log("P2P MemberJoin " + memberHostID + " groupHostID " + groupHostID + " member Count " + memberCount);
    }

    // p2p member leave
    void OnP2PMemberLeave(HostID memberHostID , HostID groupHostID , int memberCount)
    {
        Debug.Log("P2P MemberLeave " + memberHostID + " groupHostID " + groupHostID + " memberCount " + memberCount);
    }
    #endregion

    #region C2S_Method
    public delegate void LoginResult(bool result);
    public event LoginResult loginResult;

    //login
    public void C2SRequestLogin(string name)
    {
        m_c2sProxy.RequestServerConnect(HostID.HostID_Server , RmiContext.ReliableSend , name);
    }

    //player created
    public void C2SRequestClientJoin(string name,UnityEngine.Vector3 pos)
    {
        var sendOption = new RmiContext(); // (2)

        NetworkLog("RequestClientJoin " + name);

        m_c2sProxy.RequestClientJoin(HostID.HostID_Server , RmiContext.ReliableSend,(int)m_hostID, name , pos.x , pos.y , pos.z);
    }

    //move
    public void C2SRequestPlayerMove(string name,
        UnityEngine.Vector3 cur,UnityEngine.Vector3 velocity,
        UnityEngine.Vector3 charrot, UnityEngine.Vector3 rot)
    {
        var sendOption = new RmiContext(); // (2)

        sendOption.reliability = MessageReliability.MessageReliability_Unreliable;
        sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;

        m_c2sProxy.NotifyPlayerMove(m_p2pID , sendOption , (int)m_hostID , name ,
            cur.x , cur.y , cur.z , velocity.x , velocity.y , velocity.z,
            charrot.x,charrot.y,charrot.z,
            rot.x,rot.y,rot.z);

    }

    //animation send
    public void C2SRequestPlayerAnimation(string name,string aniName,int anivalue)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Unreliable;
        sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerAnimation(m_p2pID , sendOption , (int)m_hostID , name , aniName , anivalue);
    }

    // item Create Command
    public void C2SRequestItemCreate(int itemCID,int itemID,UnityEngine.Vector3 pos ,UnityEngine.Vector3 rot)
    {
        NetworkLog("ItemCreate " + pos + " rot " + rot);
        m_c2sProxy.RequestWorldCreateItem(HostID.HostID_Server , RmiContext.ReliableSend , (int)m_hostID ,
            itemCID,itemID , pos , rot);
    }

    // item Equip Send
    public void C2SRequestEquipItem(int itemCID,int itemID)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Reliable;
        sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerEquipItem(m_p2pID , sendOption , (int)m_hostID , itemCID , itemID);
    }

    //item UnEquip Send
    public void C2SRequestUnEquipItem(int itemCID,int itemID,UnityEngine.Vector3 pos,UnityEngine.Vector3 rot)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Reliable;
        sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerUnEquipItem(m_p2pID , sendOption , (int)m_hostID , itemCID , itemID , pos , rot);
    }

    //bullet Create Send
    public void C2SRequestBulletCreate(string bulletID,UnityEngine.Vector3 pos,UnityEngine.Vector3 rot)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Reliable;
        sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerBulletCreate(m_p2pID , sendOption , (int)m_hostID , "test" , bulletID , pos , rot);
    }

    // bullet move
    public void C2SRequestBulletMove(string bulletID,UnityEngine.Vector3 pos,UnityEngine.Vector3 velocity,UnityEngine.Vector3 rot)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Reliable;
        sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerBulletMove(m_p2pID , sendOption , (int)m_hostID , bulletID , pos , velocity , rot);
    }

    #endregion

    void NetworkLog(object o)
    {
        Debug.Log("NetworkManager : "+o);
    }
}
