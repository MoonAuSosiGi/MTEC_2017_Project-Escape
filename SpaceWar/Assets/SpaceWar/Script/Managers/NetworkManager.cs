using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nettention;
using Nettention.Proud;
using SpaceWar;
using System;

public class NetworkManager : Singletone<NetworkManager> {

    #region NetworkManager_INFO
    //-- NetworkManager ---------------------------------------------------------------------//
    public HostID m_hostID;
    public HostID m_p2pID;

    // 서버 guid 
    System.Guid m_protocolVersion = new System.Guid("{0xa1152276,0xe4a,0x416c,{0x97,0xba,0xa1,0x1a,0x3e,0xd0,0xea,0x55}}");

    // 서버 포트
    int m_serverPort = 54532;

    private string m_userName = null;

    public string USER_NAME
    {
        get { return m_userName; }
        set { m_userName = value; }
    }

    // 호스트 유무
    private bool m_isHost = false;

    public bool IS_HOST
    {
        get { return m_isHost; }
    }

    // 서버 아이피
    private string m_serverIP = "localhost";

    public string SERVER_IP
    {
        get { return m_serverIP; }
        set { m_serverIP = value; }
    }

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
    private bool m_isGameRunning = false;
    private bool m_isTeamMode = false;

    public bool LOGIN_STATE { get { return m_isLogin; } }
    public bool CONNECT_STATE { get { return m_isConnect; } }
    public bool GAME_RUNNING { get { return m_isGameRunning; } }
    public bool IS_TEAMMODE { get { return m_isTeamMode; } set { m_isTeamMode = value; } }

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
    public Dictionary<string , Bullet> m_bulletList = new Dictionary<string , Bullet>();
    public Bullet GetNetworkBullet(string bulletID)
    {
        if (m_bulletList.ContainsKey(bulletID))
            return m_bulletList[bulletID];
        else
            return null;
    }

    #region NetworkObjectList
    // OxyCharger
    private List<OxyCharger> m_oxyChargerList = new List<OxyCharger>();

    public bool isListInOxyCharger(OxyCharger charger)
    {
        return m_oxyChargerList.Contains(charger);
    }

    public int GetOxyChargerIndex(OxyCharger charger)
    {
        return m_oxyChargerList.IndexOf(charger);
    }

    // itemBox
    private List<ItemBox> m_itemBoxList = new List<ItemBox>();

    public bool isListInItemBox(ItemBox box)
    {
        return m_itemBoxList.Contains(box);
    }

    public int GetItemBoxIndex(ItemBox box)
    {
        return m_itemBoxList.IndexOf(box);
    }

    // Shelter
    private List<Shelter> m_shelterList = new List<Shelter>();

    public bool isListInShelter(Shelter shelter)
    {
        return m_shelterList.Contains(shelter);
    }

    public int GetShelterIndex(Shelter shelter)
    {
        return m_shelterList.IndexOf(shelter);
    }

    // SpaceShip
    private List<SpaceShip> m_spaceShipList = new List<SpaceShip>();

    public bool isListInSpaceShip(SpaceShip spaceShip)
    {
        return m_spaceShipList.Contains(spaceShip);
    }

    public int GetSpaceShipIndex(SpaceShip spaceShip)
    {
        return m_spaceShipList.IndexOf(spaceShip);
    }
    #endregion

    public GameObject m_itemBoxParent = null;
    public GameObject m_oxyChargerParent = null;
    public GameObject m_shelterParent = null;
    public GameObject m_spaceShipParent = null;
    #endregion

    #region UnityMethod
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

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
        #region Network Lobby 
        // 로그인 성공시
        m_s2cStub.NotifyLoginSuccess = (HostID remote , RmiContext rmiContext , int hostid, bool host) =>
        {
            m_isLogin = true;
            m_hostID = (HostID)hostid;
            m_isHost = host;
            NetworkLog("Login Success " + (int)hostid);

            if (loginResult != null)
                loginResult(true);

            return true;
        };

        // 로그인 실패시
        m_s2cStub.NotifyLoginFailed = (HostID remote , RmiContext rmiContext , System.String reason) =>
        {
            NetworkLog("Login Failed " + reason);
            if (loginResult != null)
                loginResult(false);
            return true;
        };

        // 플레이어가 나갔을 때
        m_s2cStub.NotifyPlayerLost = (HostID remote , RmiContext rmiContext , int hostID) =>
        {
            NetworkLog("Player Lost .. " + hostID);

            if (playerLost != null)
                playerLost(hostID);

            if (!GAME_RUNNING)
                return true;

            Debug.Log("Player Lost ");
            NetworkPlayer target = null;
            foreach (NetworkPlayer p in m_players)
            {
                if (p.m_hostID == (HostID)hostID)
                {
                    target = p;
                    break;
                }
            }

            GameObject.Destroy(target.gameObject);
            m_players.Remove(target);
            return true;
        };

        // 처음 들어왔을때 기존 정보를 싹다 받아야함
        m_s2cStub.NotifyNetworkUserSetup = (HostID remote , RmiContext rmiContext , int userHostID , string userName ,
            bool ready , bool teamRed) =>
        {
            Debug.Log("Notify Network User Setup " + userHostID);
            if (otherRoomPlayerInfo != null)
                otherRoomPlayerInfo(userHostID , userName , ready , teamRed);
            return true;
        };

        // 누군가가 방을 들어왔다.
        m_s2cStub.NotifyNetworkConnectUser = (HostID remote , RmiContext rmiContext ,int userHostID, string userName) =>
        {
            if (otherPlayerConnect != null)
                otherPlayerConnect(userHostID , userName);
            return true;
        };

        // 팀 변경 정보가 넘어왔다
        m_s2cStub.NotifyNetworkGameTeamChange = (HostID remote , RmiContext rmiContext , int userHostID , bool teamRed) =>
        {
            if (otherPlayerTeamChange != null)
                otherPlayerTeamChange(userHostID , teamRed);
            return true;
        };

        // 레디 정보가 넘어왔다
        m_s2cStub.NotifyNetworkReady = (HostID remote , RmiContext rmiContext ,int userHostID, string userName , bool ready) =>
        {
            if (otherPlayerReady != null)
                otherPlayerReady(userHostID , userName , ready);
            return true;
        };

        // 게임 모드가 변경됨
        m_s2cStub.NotifyNetworkGameModeChange = (HostID remote , RmiContext rmiContext , int gameMode , bool teamMode) =>
        {
            if (gameModeChange != null)
                gameModeChange(gameMode , teamMode);
            return true;
        };

        // 플레이어 수가 바뀜 
        m_s2cStub.NotifyNetworkGamePlayerCountChange = (HostID remote , RmiContext rmiContext , int playerCount) =>
        {
            if (playerCountChnage != null)
                playerCountChnage(playerCount);
            return true;
        };

        // 게임이 시작되었음!
        m_s2cStub.NotifyNetworkGameStart = (HostID remote , RmiContext rmiContext) =>
        {
            if (gameStart != null)
                gameStart();
            return true;
        };

        // 게임이 실패
        m_s2cStub.NotifyNetworkGameStartFailed = (HostID remote , RmiContext rmiContext) =>
        {
            if (gameStartFailed != null)
                gameStartFailed("test");
            return true;
        };

        // 호스트가 나갔다.
        m_s2cStub.NotifyNetworkGameHostOut = (HostID remote , RmiContext rmiContext) =>
        {
            if (hostOut != null)
                hostOut();
            return true;
        };

        #endregion
        #region InGame Network Play
        // 네트워크 플레이어의 이동 메시지가 왔다.
        m_s2cStub.NotifyPlayerMove = (HostID remote , RmiContext rmiContext ,
            int sendHostID , string name , float curX , float curY , float curZ ,
            float velocityX , float velocityY , float velocityZ ,
            float crx , float cry , float crz ,
            float rx , float ry , float rz) =>
        {
            foreach (NetworkPlayer p in m_players)
            {
                if (p.HOST_ID == (HostID)sendHostID)
                {
                    p.RecvNetworkMove(new UnityEngine.Vector3(curX , curY , curZ) ,
                       new UnityEngine.Vector3(velocityX , velocityY , velocityZ) ,
                       new UnityEngine.Vector3(crx , cry , crz) ,
                       new UnityEngine.Vector3(rx , ry , rz));
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
                if ((int)p.m_hostID == sendHostID)
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
            NetworkLog("Other Client Join " + name + " host " + m_hostID + " h " + hostID);
            if (m_hostID == (HostID)hostID)
                return true;

            GameObject MP = GameManager.Instance().OnJoinedRoom(name , false ,
                 new UnityEngine.Vector3(0.0f , 80.0f , 0.0f));
            MP.GetComponent<NetworkPlayer>().NetworkPlayerSetup((HostID)hostID , name);
            return true;
        };

        // 아이템 생성 로직
        m_s2cStub.NotifyCreateItem = (HostID remote , RmiContext rmiContext , int sendHostID ,
            int itemCID , int itemID , UnityEngine.Vector3 pos , UnityEngine.Vector3 rot) =>
        {
            NetworkLog("NotifyCreateItem .." + itemCID);

            // 아이템 등록 
            m_networkItemList.Add(itemID , GameManager.Instance().CommandItemCreate(itemCID , itemID , pos , rot));
            return true;
        };

        // 아이템을 장비했다 ( 다른 플레이어 )
        m_s2cStub.NotifyPlayerEquipItem = (HostID remote , RmiContext rmiContext , int hostID ,
            int itemCID , int itemID) =>
        {

            GameObject item = GetNetworkItem(itemID);
            NetworkLog("NotifyPlayerEquipItem " + itemID + " null ? " + (item == null));

            if (item != null)
            {
                foreach (var p in m_players)
                {
                    if ((int)p.m_hostID == hostID)
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
                    p.UnEquipWeapon(pos , rot);
                    break;
                }
            }
            return true;
        };

        // 총알 생성해라
        m_s2cStub.NotifyPlayerBulletCreate = (HostID remote , RmiContext rmiContext ,
            int sendHostID , int bulletCode , string bulletID ,
            UnityEngine.Vector3 pos , UnityEngine.Vector3 rot) =>
        {
            NetworkLog("Bullet Create " + sendHostID + " t " + (int)m_hostID);

            Bullet b = GetNetworkBullet(bulletID);
            Debug.Log("B  BB " + (b == null));
            
            if (b != null && b.ITEM_CODE == bulletCode)
            {
                // 이미 생성되어 있는 것
                // 재활용ㅋ
                b.NetworkReset(new Nettention.Proud.Vector3(pos.x,pos.y,pos.z));
                b.transform.position = pos;
                b.transform.eulerAngles = rot;
                b.NETWORK_ID = bulletID;
                b.IS_REMOTE = true;
                
            }
            else
            {
                GameObject bullet = null;
                string path = "";
                // 타입 검사 후 생성하는 로직 -현재 임시로직
                switch (bulletCode)
                {
                    case 1: path = "Bullet/Gun01/Gun01_01"; break;
                    case 2: path = "Bullet/Gun02/Gun02_02"; break;
                }

               bullet = GameObject.Instantiate(Resources.Load(path)) as GameObject;
                bullet.transform.parent = transform;
                bullet.transform.position = pos;
                bullet.transform.localEulerAngles = rot;
                b = bullet.GetComponent<Bullet>();
                b.ITEM_CODE = bulletCode;
                b.NETWORK_ID = bulletID;
                Debug.Log("ddddddddddddddddddddddddddddd " + bulletID);
                b.IS_REMOTE = true;
                b.enabled = true;
                b.NetworkBulletEnable();
                bullet.SetActive(true);
                m_bulletList.Add(bulletID , b);
            }


            return true;
        };

        // 총알 이동
        m_s2cStub.NotifyPlayerBulletMove = (HostID remote , RmiContext rmiContext ,
            int sendHostID , string bulletID , UnityEngine.Vector3 pos , UnityEngine.Vector3 velocity , UnityEngine.Vector3 rot) =>
        {
            Bullet b = GetNetworkBullet(bulletID);

            if (b == null)
                NetworkLog("ERROR bulletID 미등록 " + bulletID);
            else
            {
                b.NetworkMoveRecv(pos , velocity , rot);
            }
            return true;
        };

        // 총알 부딪혀서 삭제해라
        m_s2cStub.NotifyPlayerBulletDelete = (HostID remote , RmiContext rmiContext , int sendHostID , string bulletID) =>
        {
            Bullet b = GetNetworkBullet(bulletID);

            if (b == null)
                NetworkLog("ERROR bulletID 미등록");
            else
                b.NetworkRemoveEvent();
            return true;
        };

        // hp 업데이트 이벤트
        m_s2cStub.NotifyPlayerChangeHP = (HostID remote , RmiContext rmiContext ,
            int targetHostID , string name , float hp , float prevhp , float maxhp,UnityEngine.Vector3 dir) =>
        {
            NetworkLog("damage host " + (int)m_hostID + " target " + targetHostID);
            if ((int)m_hostID == targetHostID)
            {
                GameManager.Instance().ChangeHP(hp , prevhp , maxhp);
                GameManager.Instance().PLAYER.m_player.Damage(dir);
            }
            return true;
        };

        // oxy 업데이트 이벤트
        m_s2cStub.NotifyPlayerChangeOxygen = (HostID remote , RmiContext rmiContext ,
            int targetHostID , string name , float oxygen , float prevoxy , float maxoxy) =>
        {
            if ((int)m_hostID == targetHostID)
            {
                if(oxygen <= 0)
                {
                    C2SRequestPlayerDamage((int)m_hostID , "" , "oxy" , 5.0f,UnityEngine.Vector3.zero);
                }
                GameManager.Instance().ChangeOxy(oxygen , prevoxy , maxoxy);
            }
            return true;
        };

        // OxyCharger 조작 이벤트
        m_s2cStub.NotifyUseOxyCharger = (HostID remote , RmiContext rmiContext ,
            int sendHostID , int oxyChargerIndex , float userOxy) =>
        {
            m_oxyChargerList[oxyChargerIndex].RecvOxy(userOxy);
            return true;
        };

        // itemBox 조작 이벤트 ( 아이템 박스 사용 결과와 생성할 아이템 코드가 날아옴 )
        m_s2cStub.NotifyUseItemBox = (HostID remote , RmiContext rmiContext ,
            int sendHostID , int itemBoxIndex , int itemID) =>
        {
            if (itemBoxIndex < 0 || m_itemBoxList.Count <= itemBoxIndex)
            {
                Debug.Log("NotifyUseItemBox index Error " + itemBoxIndex);
                return true;
            }
            ItemBox box = m_itemBoxList[itemBoxIndex];

            box.ItemBoxClose();

            if ((int)m_hostID != sendHostID)
                box.ItemBoxNetworkOpen();
            else
            {
                // 같을 경우 
                Debug.Log("얻었다 ! " + itemID + "를!");
                //임의
                GameManager.Instance().RecvItem(itemID-1 , box.transform.GetChild(3).gameObject);
            }

            return true;
        };

        // 쉘터 문 정보가 왔을 때 
        m_s2cStub.NotifyShelterInfo = (HostID remote ,
            RmiContext rmiContext ,
            int sendHostID , int shelterID , bool doorState ,
            bool lightState) =>
        {
            Debug.Log("shelterinfo " + lightState + " id " + shelterID);


            if (lightState)
                m_shelterList[shelterID].LightOn();
            else
                m_shelterList[shelterID].LightOff();

            // 네트워크에서 쏜 것이므로 메시지는 던지지않음
            if (doorState)
                m_shelterList[shelterID].OpenDoor(true);
            else
                m_shelterList[shelterID].CloseDoor(true);

            return true;
        };

        // 메테오 시간
        m_s2cStub.NotifyMeteorCreateTime = (HostID remote , RmiContext rmiContext , int time) =>
        {
            if (m_isGameRunning == false)
                return true;
            if (time >= 0)
            {
              //  GameManager.Instance().m_inGameUI.StartMeteor();
              //  GameManager.Instance().m_inGameUI.RecvMeteorInfo(time);
            }
            else if (time < -1)
            {
              //  GameManager.Instance().m_inGameUI.StopMeteor();
            }
            return true;
        };

        // 메테오 생성해라
        m_s2cStub.NotifyMeteorCreate = (HostID remote , RmiContext rmiContext ,
            float anglex , float anglez) =>
        {
            if (m_isGameRunning == false)
                return true;
            GameManager.Instance().CreateMeteor(anglex , anglez);
            return true;
        };

        // 우주선 조작 정보가 넘어옴
        m_s2cStub.NotifySpaceShipEngineCharge = (HostID remote , RmiContext rmiContext ,
            int spaceShipID , float fuel) =>
        {
            Debug.Log("넘어옴 정보 " + spaceShipID);
            var spaceShip = m_spaceShipList[spaceShipID];

            spaceShip.SpaceShipEngineCharge(fuel , false);
            return true;
        };


        #endregion

        // -- 게임 결과에 관련된 것들
        #region ResultStub 
        // 게임 결과 // 자기 자신
        m_s2cStub.NotifyGameResultInfoMe = (HostID remote , RmiContext rmiContext ,
            string gameMode , int winState , int playTime , int kills , int assists ,
            int death , int getMoney) =>
        {
            if (gameResultInfoToMe != null)
                gameResultInfoToMe(gameMode , winState , playTime , kills , assists , death , getMoney);
          //  GameManager.Instance().SetResultProfileInfo(gameMode , winState , playTime , kills , assists , death , getMoney);
            return true;
        };

        // 게임 결과 // 적들
        m_s2cStub.NotifyGameResultInfoOther = (HostID remote , RmiContext rmiContext ,
            string name , int state) =>
        {
            Debug.Log("Result 적들 "+(gameResultInfoToOther == null) );
            //GameManager.Instance().AddResultOtherProfileInfo(name , state);

            if (gameResultInfoToOther != null)
                gameResultInfoToOther(name , state);

            return true;
        };

        // 이제 결과를 띄워라
        m_s2cStub.NotifyGameResultShow = (HostID remote , RmiContext rmiContext) =>
        { 
            //GameManager.Instance().GameResultShow();

            if (gameResultShow != null)
                gameResultShow();

            return true;
        };
        #endregion

        #endregion

        //   ServerConnect();
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
        if (m_isConnect)
            return;
        m_param.serverIP = m_serverIP;
        try
        {
            m_netClient.Connect(m_param);
        }
        catch (Exception)
        {
            if (loginResult != null)
                loginResult(false);
        }


    }
    #endregion

    #region InGame Method

    public void NetworkShelterServerSetup()
    {
        for (int i = 0; i < m_shelterList.Count; i++)
        {
            m_shelterList[i].SHELTER_ID = GetShelterIndex(m_shelterList[i]);
            C2SRequestShelterStartSetup(m_shelterList[i].SHELTER_ID);
        }
    }

    public void NetworkObjectSetup()
    {
        NetworkLog("Network Object Setup -- 사용 가능한 상태로 만들기 --");
        NetworkSetupItemBox();
        NetworkSetupOxyCharger();
        NetworkSetupSpaceShip();
        NetworkSetupShelter();
        NetworkLog("Network Object Setup --          Finish           --");
    }

    // 쉘터
    void NetworkSetupShelter()
    {
        NetworkLog("Shelter--");
        for (int i = 0; i < m_shelterParent.transform.childCount; i++)
        {
            m_shelterList.Add(m_shelterParent.transform.GetChild(i).GetComponent<Shelter>());
            m_shelterList[i].SHELTER_ID = i;
        }
    }

    // 아이템박스
    void NetworkSetupItemBox()
    {
        NetworkLog("ItemBox--");
        for (int i = 0; i < m_itemBoxParent.transform.childCount; i++)
        {
            m_itemBoxList.Add(m_itemBoxParent.transform.GetChild(i).GetComponent<ItemBox>());
            m_itemBoxList[i].ITEMBOX_ID = i;
        }
    }

    // 산소 충전기
    void NetworkSetupOxyCharger()
    {
        NetworkLog("OxyCharger--");
        for (int i = 0; i < m_oxyChargerParent.transform.childCount; i++)
        {
            m_oxyChargerList.Add(m_oxyChargerParent.transform.GetChild(i).GetComponent<OxyCharger>());
            m_oxyChargerList[i].OXY_CHARGER_ID = i;
        }
    }

    // 우주선
    void NetworkSetupSpaceShip()
    {
        NetworkLog("SpaceShip--");
        for (int i = 0; i < m_spaceShipParent.transform.childCount; i++)
        {
            m_spaceShipList.Add(m_spaceShipParent.transform.GetChild(i).GetComponent<SpaceShip>());
            m_spaceShipList[i].SPACESHIP_ID = i;
        }
    }
    #endregion

    #region EventDelegate
    void OnJoinServerComplete(ErrorInfo info , ByteArray replyFromServer)
    {
        // 성공적으로 연결 되면.
        if (Nettention.Proud.ErrorType.ErrorType_Ok == info.errorType)
        {
            NetworkLog("Server Connected ----------------------- ");
            // 유저네임으로 로그인 요청 ( 중복도 상관 없음 , 내부적으로는 hostID 로 판단)
            RequestServerConnect(USER_NAME);

            m_isConnect = true; // bool 변수 값 true 로 변경합니다.          
        }
        else
        {
            // 에러처리를 합니다.
            NetworkLog("Server connection failed. ::  " + info.ToString());
        }
    }
    // 서버와 연결이 해제 되면 콜백됩니다.
    void OnLeaveServer(Nettention.Proud.ErrorInfo info)
    {
        if (Nettention.Proud.ErrorType.ErrorType_Ok != info.errorType)
        {
            // 에러입니다.
            NetworkLog("OnLeaveServer. " + info.ToString());
        }

        NetworkLog("Server Disconnected");
        // 서버와의 연결이 종료되었으니 초기화면으로 나가거나, 다른처리를 해주어야 하빈다.
        m_isConnect = false;

    }

    // p2p member join
    void OnP2PMemberJoin(HostID memberHostID , HostID groupHostID , int memberCount , ByteArray customField)
    {
        m_p2pID = groupHostID;
        NetworkLog("P2P MemberJoin " + memberHostID + " groupHostID " + groupHostID + " member Count " + memberCount);
    }

    // p2p member leave
    void OnP2PMemberLeave(HostID memberHostID , HostID groupHostID , int memberCount)
    {
        NetworkLog("P2P MemberLeave " + memberHostID + " groupHostID " + groupHostID + " memberCount " + memberCount);
    }
    #endregion

    // 바뀌는 네트워크 메소드 
    #region NetworkPlay 

    #region NetworkLobby Method

    // 호스트가 나갔다
    public delegate void HostOut();
    public event HostOut hostOut = null;

    // 플레이어가 나갔다.
    public delegate void PlayerLost(int lostPlayerID);
    public event PlayerLost playerLost = null;

    // 로그인 정보를 받아야 할 델리게이트
    public delegate void LoginResult(bool result);
    public event LoginResult loginResult = null;

    // 처음으로 들어왔을때 이미 들어온 놈들에 대한 정보
    public delegate void OtherRoomPlayerInfo(int hostID , string userName , bool ready , bool redTeam);
    public event OtherRoomPlayerInfo otherRoomPlayerInfo = null;

    //누군가가 방에 들어왔다.
    public delegate void OtherPlayerConnect(int hostID , string userName);
    public event OtherPlayerConnect otherPlayerConnect = null;

    //팀변경 
    public delegate void OtherPlayerTeamChange(int hostID  , bool redTeam);
    public event OtherPlayerTeamChange otherPlayerTeamChange = null;

    // 레디
    public delegate void OtherPlayerReady(int hostID , string userName , bool ready);
    public event OtherPlayerReady otherPlayerReady = null;

    // 인원
    public delegate void PlayerCountChange(int playerCount);
    public event PlayerCountChange playerCountChnage = null;

    // 게임모드 변경
    public delegate void GameModeChange(int gameMode , bool teamMode);
    public event GameModeChange gameModeChange = null;

    // 맵 변경
    public delegate void MapChange(string changeMap);
    public event MapChange mapChange = null;

    // 게임 시작
    public delegate void GameStart();
    public event GameStart gameStart = null;

    // 게임 시작 실패
    public delegate void GameStartFailed(string reason);
    public event GameStartFailed gameStartFailed = null;


    // 로그인 요청. 서버에 접속하고 바로 날린다.
    public void RequestServerConnect(string name)
    {
        m_c2sProxy.RequestServerConnect(HostID.HostID_Server , RmiContext.ReliableSend , name);
    }

    // 내가 로비에 들어왔음을 알리고 정보를 받는다.
    public void RequestLobbyConnect()
    {
        m_c2sProxy.RequestLobbyConnect(HostID.HostID_Server , RmiContext.ReliableSend);
    }

    // 팀 선택 
    public void RequestNetworkGameTeamSelect(bool teamRed)
    {
        m_c2sProxy.RequestNetworkGameTeamSelect(HostID.HostID_Server , RmiContext.ReliableSend ,
            USER_NAME , teamRed);
    }

    // 레디하기
    public void RequestNetworkGameReady(bool ready)
    {
        m_c2sProxy.RequestNetworkGameReady(HostID.HostID_Server , RmiContext.ReliableSend ,
            USER_NAME , ready);
    }

    #region Room Host Method
    // 방장이 맵을 바꾼다!
    public void RequestNetworkChangeMap(string mapName)
    {
        m_c2sProxy.RequestNetworkChangeMap(HostID.HostID_Server , RmiContext.ReliableSend ,
            mapName);
    }

    // 방장이 플레이어 수를 바꾼다!
    public void RequestNetworkPlayerCount(int playerCount)
    {
        m_c2sProxy.RequestNetworkPlayerCount(HostID.HostID_Server , RmiContext.ReliableSend , playerCount);
    }

    // 방장이 게임 모드를 바꾼다!
    public void RequestNetworkGameModeChange(int gameMode,bool teamMode)
    {
        m_c2sProxy.RequestNetworkGameModeChange(HostID.HostID_Server , RmiContext.ReliableSend , gameMode , teamMode);
    }

    // 방장이 게임 시작을 눌렀다!
    public void RequestNetworkGameStart()
    {
        m_c2sProxy.RequestNetworkGameStart(HostID.HostID_Server , RmiContext.ReliableSend);
    }

    // 방장이 나갔어!
    public void RequestNetworkHostOut()
    {
        m_c2sProxy.RequestNetworkHostOut(HostID.HostID_Server,RmiContext.ReliableSend,(int)m_hostID);
    }

    // 게임 씬으로 넘어왔어!
    public void RequestGameSceneJoin(UnityEngine.Vector3 pos)
    {
        m_isGameRunning = true;
        m_c2sProxy.RequestGameSceneJoin( HostID.HostID_Server,RmiContext.ReliableSend, pos , (int)m_hostID , USER_NAME);   
    }

    #endregion

    #endregion

    #region NetworkResult Delegate
    //자기 자신 정보 전달
    public delegate void GameResultInfoToMe(string gameMode , int winState , int playTime , int kills , int assists , int deah , int getMoney);
    public event GameResultInfoToMe gameResultInfoToMe = null;

    // 적들의 정보가 하나씩 넘어옴
    public delegate void GameResultInfoToOther(string name , int state);
    public event GameResultInfoToOther gameResultInfoToOther = null;

    // 결과를 띄우세요
    public delegate void GameResultShow();
    public event GameResultShow gameResultShow = null;
    
    #endregion

    #endregion

    #region C2S_Method
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
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerEquipItem(m_p2pID , sendOption , (int)m_hostID , itemCID , itemID);
    }

    //item UnEquip Send
    public void C2SRequestUnEquipItem(int itemCID,int itemID,UnityEngine.Vector3 pos,UnityEngine.Vector3 rot)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Reliable;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerUnEquipItem(m_p2pID , sendOption , (int)m_hostID , itemCID , itemID , pos , rot);
    }

    //bullet Create Send
    public void C2SRequestBulletCreate(string bulletID,UnityEngine.Vector3 pos,UnityEngine.Vector3 rot,int bulletCode)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Reliable;
        //sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerBulletCreate(m_p2pID , sendOption , (int)m_hostID , bulletCode , bulletID , pos , rot);
    }

    // bullet move
    public void C2SRequestBulletMove(string bulletID,UnityEngine.Vector3 pos,UnityEngine.Vector3 velocity,UnityEngine.Vector3 rot)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Unreliable;
     //   sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifyPlayerBulletMove(m_p2pID , sendOption , (int)m_hostID , bulletID , pos , velocity , rot);
    }
    
    // bullet delete
    public void C2SRequestBulletRemove(string bulletID)
    {
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Unreliable;
        //   sendOption.maxDirectP2PMulticastCount = 30;
        sendOption.enableLoopback = false;

        m_c2sProxy.NotifyPlayerBulletDelete(m_p2pID , sendOption , (int)m_hostID , bulletID);
    }

    // damage
    public void C2SRequestPlayerDamage(int targetHostID,string name,string weaponName,float damage,UnityEngine.Vector3 dir)
    {
        m_c2sProxy.RequestPlayerDamage(HostID.HostID_Server , RmiContext.ReliableSend , (int)m_hostID ,
            targetHostID , name , weaponName , damage,dir);
    }

    // use Oxy
    public void C2SRequestPlayerUseOxy(string name,float useOxy)
    {
        m_c2sProxy.RequestPlayerUseOxy(HostID.HostID_Server , RmiContext.ReliableSend , (int)m_hostID , name , useOxy);
    }

    // user OxyCharger
    public void C2SRequestPlayerUseOxyCharger(OxyCharger charger,float UseOxy)
    {
        m_c2sProxy.RequestUseOxyCharger(HostID.HostID_Server , RmiContext.ReliableSend ,
            (int)m_hostID , GetOxyChargerIndex(charger) , UseOxy);
    }

    // use itemBox
    public void C2SRequestPlayerUseItemBox(ItemBox box)
    {
        Debug.Log("itemBox Use ");
        m_c2sProxy.RequestUseItemBox(HostID.HostID_Server , RmiContext.ReliableSend ,
            (int)m_hostID , GetItemBoxIndex(box));   
    }

    // 쉘터 등록
    public void C2SRequestShelterStartSetup(int shelterID)
    {
        m_c2sProxy.RequestShelterStartSetup(HostID.HostID_Server , RmiContext.ReliableSend ,
            shelterID);
    }

    // 쉘터 변경 요청
    public void C2SRequestShelterDoorControl(int shelterID , bool doorState)
    {
        Debug.Log("Shelter Door Control");

        m_c2sProxy.RequestShelterDoorControl(HostID.HostID_Server, 
            RmiContext.ReliableSend,(int)m_hostID , shelterID , doorState);
    }

    // 쉘터 입장
    public void C2SRequestShelterEnter(int shelterID,bool enter)
    {
        Debug.Log("Shelter Enter " +enter);
        m_c2sProxy.RequestShelterEnter(HostID.HostID_Server , 
            RmiContext.ReliableSend , (int)m_hostID,shelterID , enter);
    }

    // 우주선 조작
    public void C2SNotifySpaceShipEngineCharge(int spaceShipID,float fuel)
    {
        NetworkLog("SpaceShip Charge "+fuel);
        var sendOption = new RmiContext(); // (2)
        sendOption.reliability = MessageReliability.MessageReliability_Reliable;
        sendOption.enableLoopback = false;
        m_c2sProxy.NotifySpaceShipEngineCharge(m_p2pID , sendOption , spaceShipID , fuel);
    }

    // 우주선을 탔음을 알림
    public void C2SRequestSpaceShip()
    {
        NetworkLog("RequestSpaceShip");
        m_c2sProxy.RequestSpaceShip(HostID.HostID_Server , RmiContext.ReliableSend , (int)m_hostID);
    }

    // 게임이 끝났음을 알림
    public void C2SRequestGameEnd()
    {
        m_isGameRunning = false;
        m_c2sProxy.RequestGameEnd(HostID.HostID_Server , RmiContext.ReliableSend);
    }

    #endregion

    void NetworkLog(object o)
    {
        Debug.Log("NetworkManager : "+o);
    }
}
