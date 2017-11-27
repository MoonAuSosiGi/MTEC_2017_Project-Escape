using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour {

    #region LobbyUI_INFO

    // 게임 세팅 팝업
    public GameObject m_popup;

    // 방장일 때 나와야함
    public GameObject m_gameSetting = null;
    // 레디 / 스타트 변경용
    public UILabel m_readyLabel = null;
    private bool m_isReady = false;
    private int m_myIndex = -1;

    // 방장 전용 
    private int m_playerCount = 10;
    private int m_curPlayer = 1;
    private bool m_teamMode = false;
    private int m_gameMode = (int)GameManager.GameMode.SURVIVAL;
    public UILabel m_gameSettingPlayerCount = null;

    // 내부에서 바뀌어야할 내용들
    #region UI_DATA
    public UILabel m_myName = null;
    public UILabel m_myMoney = null;
    public UISprite m_myTeamBG = null;
    // 우측 상단 메뉴
    public UISprite m_currentMapIcon = null;
    public UILabel m_currentMapName = null;
    public UILabel m_currentMapSize = null;
    public UILabel m_currentPlayerCount = null;

    // 팀 선택용
    public GameObject m_redTeam = null;
    public GameObject m_blueTeam = null;

    // 게임 세팅 
    public UISprite m_individualSpr = null;
    public UISprite m_teamSpr = null;
    public UILabel m_gameModeText = null;

    // 에러 로그용
    public UILabel m_errorLog = null;
    #endregion

    // 팀 관련 표시 UI ( 우측메뉴 - 팀만)
    #region Team_UI_DATA
    public GameObject m_rightTeamMenu = null;
    public List<PlayerLobbyData> m_playerLobbyList = new List<PlayerLobbyData>();
    public List<TeamRightData> m_teamList = new List<TeamRightData>();

    // 오른쪽에 팀전일 때만 뜨는 리스트 
    [System.Serializable]
    public class TeamRightData
    {
        public UISprite m_teamBG = null;
        public UILabel m_userName = null;
        public int m_hostID = -1;

        public void SetTeamColor(bool teamRed)
        {
            Color c = new Color();

            c.r = (teamRed) ? 200.0f / 255.0f : 80.0f / 255.0f;
            c.g = 80.0f / 255.0f;
            c.b = (teamRed) ? 80.0f / 255.0f : 200.0f / 255.0f;
            c.a = 204.0f / 255.0f;

            m_teamBG.color = c;
        }

        public void SetName(string userName)
        {
            m_userName.text = userName;
        }
    }
    
    // 캐릭터들
    [System.Serializable]
    public class PlayerLobbyData
    {
        public int m_hostID = -1;
        public TextMesh m_userName = null;
        public SpriteRenderer m_teamColorArrow = null;
        public GameObject m_readyEffect = null;

        public void SetTeamColor(bool teamMode , bool teamRed = true)
        {
            if (!teamMode)
            {
                m_teamColorArrow.color = Color.white;
                return;
            }
            Color c = new Color();

            c.r = (teamRed) ? 200.0f / 255.0f : 80.0f / 255.0f;
            c.g = 80.0f / 255.0f;
            c.b = (teamRed) ? 80.0f / 255.0f : 200.0f / 255.0f;
            c.a = 204.0f / 255.0f;

            m_teamColorArrow.color = c;
        }

        public void SetName(string userName)
        {
            m_userName.text = userName;
        }

        public void SetReady(bool ready)
        {
            m_readyEffect.SetActive(ready);
        }
    }

    #endregion
    #endregion

    #region UnityMethod
    void Start()
    {
        m_teamMode = NetworkManager.Instance().IS_TEAMMODE;
        // 호스트가 나갔다
        NetworkManager.Instance().hostOut += LobbyUI_hostOut;

        // 처음 등장시 이미 들어와있는 애들
        NetworkManager.Instance().otherRoomPlayerInfo += LobbyUI_otherRoomPlayerInfo;

        // 다른 플레이어가 들어왔다.
        NetworkManager.Instance().otherPlayerConnect += LobbyUI_otherPlayerConnect;

        // 다른 플레이어가 나갔다.
        NetworkManager.Instance().playerLost += LobbyUI_playerLost;

        // 팀 선택이 도착함
        NetworkManager.Instance().otherPlayerTeamChange += LobbyUI_otherPlayerTeamChange;

        // 제한 플레이어 수가 바뀜
        NetworkManager.Instance().playerCountChnage += LobbyUI_playerCountChnage;

        // 레디했다
        NetworkManager.Instance().otherPlayerReady += LobbyUI_otherPlayerReady;

        // 방장이 게임 모드를 바꾼다.
        NetworkManager.Instance().gameModeChange += LobbyUI_gameModeChange;

        // 맵을 바꿈
        NetworkManager.Instance().mapChange += LobbyUI_mapChange;

        // 게임 시작
        NetworkManager.Instance().gameStart += LobbyUI_gameStart;

        // 게임 시작 실패
        NetworkManager.Instance().gameStartFailed += LobbyUI_gameStartFailed;

        

        DefSetup();

        if (NetworkManager.Instance().IS_HOST)
            DefHostSetup();
        else
            DefPlayerSetup();

        if (NetworkManager.Instance().IS_TEAMMODE)
            TeamModeSetup();
        else
            IndividualModeSetup();


    }

    void EventHandlerAllRemove()
    {
        // 호스트가 나갔다
        NetworkManager.Instance().hostOut -= LobbyUI_hostOut;

        // 처음 등장시 이미 들어와있는 애들
        NetworkManager.Instance().otherRoomPlayerInfo -= LobbyUI_otherRoomPlayerInfo;

        // 다른 플레이어가 들어왔다.
        NetworkManager.Instance().otherPlayerConnect -= LobbyUI_otherPlayerConnect;

        // 다른 플레이어가 나갔다.
        NetworkManager.Instance().playerLost -= LobbyUI_playerLost;

        // 팀 선택이 도착함
        NetworkManager.Instance().otherPlayerTeamChange -= LobbyUI_otherPlayerTeamChange;

        // 제한 플레이어 수가 바뀜
        NetworkManager.Instance().playerCountChnage -= LobbyUI_playerCountChnage;

        // 레디했다
        NetworkManager.Instance().otherPlayerReady -= LobbyUI_otherPlayerReady;

        // 방장이 게임 모드를 바꾼다.
        NetworkManager.Instance().gameModeChange -= LobbyUI_gameModeChange;

        // 맵을 바꿈
        NetworkManager.Instance().mapChange -= LobbyUI_mapChange;

        // 게임 시작
        NetworkManager.Instance().gameStart -= LobbyUI_gameStart;

        // 게임 시작 실패
        NetworkManager.Instance().gameStartFailed -= LobbyUI_gameStartFailed;
    }

    private void LobbyUI_gameStartFailed(string reason)
    {
        Debug.Log("게임 시작 실패");
    }

    private void LobbyUI_gameStart()
    {
        EventHandlerAllRemove();
        TimeForEscape.Util.Scene.LoadingScene.LOAD_SCENE_NAME = "Space_1";
        SceneManager.LoadScene("Space_LoadingScene");
    }

    private void LobbyUI_mapChange(string changeMap)
    {
        // 맵정보를 여기서 판단하고 변환해야함 
        m_currentMapName.text = changeMap;
    }

    private void LobbyUI_gameModeChange(int gameMode , bool teamMode)
    {
        NetworkManager.Instance().IS_TEAMMODE = teamMode;
        for (int i = 0; i < m_playerLobbyList.Count; i++)
        {
            PlayerLobbyData data = m_playerLobbyList[i];
            data.SetTeamColor(true , teamMode);
        }
        m_gameMode = gameMode;
        m_teamMode = teamMode;
        GameManager.CURRENT_GAMEMODE = (GameManager.GameMode)gameMode;
        NetworkManager.Instance().IS_TEAMMODE = teamMode;

        switch ((GameManager.GameMode)m_gameMode)
        {
            case GameManager.GameMode.DEATH_MATCH:
                m_gameModeText.text = "DEATH MATCH"; break;
            case GameManager.GameMode.SURVIVAL:
                m_gameModeText.text = "SURVIVAL"; break;
        }
        if (m_teamMode)
            TeamModeSetup();
        else
            IndividualModeSetup();
    }

    private void LobbyUI_otherPlayerReady(int hostID , string userName , bool ready)
    {
        for (int i = 0; i < m_playerLobbyList.Count; i++)
        {
            PlayerLobbyData data = m_playerLobbyList[i];

            if (data.m_hostID == hostID)
            {
                data.m_readyEffect.SetActive(ready);
                break;
            }
        }
    }

    private void LobbyUI_playerCountChnage(int playerCount)
    {
        m_playerCount = playerCount;
        m_currentPlayerCount.text = m_curPlayer + " / " + m_playerCount;
    }

    private void LobbyUI_otherPlayerTeamChange(int hostID , bool redTeam)
    {
        for (int i = 0; i < m_playerLobbyList.Count; i++)
        {
            PlayerLobbyData data = m_playerLobbyList[i];

            if (data.m_hostID == hostID)
            {
                data.SetTeamColor(true , redTeam);
                break;
            }
        }

        for (int i = 0; i < m_teamList.Count; i++)
        {
            TeamRightData data = m_teamList[i];

            if (data.m_hostID == hostID)
            {
                data.SetTeamColor(redTeam);
                break;
            }
        }
    }

    private void LobbyUI_otherPlayerConnect(int hostID , string userName)
    {
        Vector4[] vecs = { new Vector4(3.68276f , 0.0f , 6.0f) ,
            new Vector4(0.0f , 6.0f , 0.7862077f) , new Vector4(0.0f , 3.517242f , 6.0f) };
        m_curPlayer++;
        for (int i = 0; i < m_playerLobbyList.Count; i++)
        {
            PlayerLobbyData data = m_playerLobbyList[i];

            if (data.m_hostID == -1)
            {

                if (i < vecs.Length)
                    data.m_readyEffect.transform.parent.GetChild(1).GetChild(3)
                        .GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_EmissionColor" ,
                        vecs[i]);
                data.m_readyEffect.transform.parent.gameObject.SetActive(true);
                data.m_hostID = hostID;
                data.m_userName.text = userName;
                data.m_readyEffect.SetActive(false);

                if (NetworkManager.Instance().IS_TEAMMODE)
                    data.SetTeamColor(true);
                else
                    data.SetTeamColor(false);
                break;
            }
        }

        for (int i = 0; i < m_teamList.Count; i++)
        {
            TeamRightData data = m_teamList[i];

            if (data.m_hostID == -1)
            {
                data.m_hostID = hostID;
                data.m_userName.text = userName;
                break;
            }
        }


        m_currentPlayerCount.text = m_curPlayer + " / " + m_playerCount;
    }

    private void LobbyUI_otherRoomPlayerInfo(int hostID , string userName , bool ready , bool redTeam)
    {
        m_curPlayer++;
        Vector4[] vecs = { new Vector4(3.68276f , 0.0f , 6.0f) ,
            new Vector4(0.0f , 6.0f , 0.7862077f) , new Vector4(0.0f , 3.517242f , 6.0f) };
        for (int i = 0; i < m_playerLobbyList.Count; i++)
        {
            PlayerLobbyData data = m_playerLobbyList[i];

            if (data.m_hostID == -1)
            {

                data.m_readyEffect.transform.parent.gameObject.SetActive(true);
                data.m_hostID = hostID;
                data.m_userName.text = userName;
                data.m_readyEffect.SetActive(ready);
                if (i < vecs.Length)
                    data.m_readyEffect.transform.parent.GetChild(1).GetChild(3)
                         .GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_EmissionColor" ,
                        vecs[i]);


                if (NetworkManager.Instance().IS_TEAMMODE)
                    data.SetTeamColor(true , redTeam);
                else
                    data.SetTeamColor(false , redTeam);
                break;
            }
        }


        for (int i = 0; i < m_teamList.Count; i++)
        {
            TeamRightData data = m_teamList[i];

            if (data.m_hostID == -1)
            {
                data.m_hostID = hostID;
                data.SetTeamColor(redTeam);
                data.m_userName.text = userName;
                data.m_teamBG.gameObject.SetActive(NetworkManager.Instance().IS_TEAMMODE);
                break;
            }
        }
        m_currentPlayerCount.text = m_curPlayer + " / " + m_playerCount;
    }

    private void LobbyUI_hostOut()
    {
        EventHandlerAllRemove();
        SceneManager.LoadScene(0);
    }

    private void LobbyUI_playerLost(int sendHostID)
    {
        m_curPlayer--;
        for (int i = 0; i < m_playerLobbyList.Count; i++)
        {
            PlayerLobbyData data = m_playerLobbyList[i];

            if (data.m_hostID == sendHostID)
            {
                data.m_hostID = -1;
                data.m_readyEffect.SetActive(false);
                data.m_readyEffect.transform.parent.gameObject.SetActive(false);
                break;
            }
        }

        for (int i = 0; i < m_teamList.Count; i++)
        {
            TeamRightData data = m_teamList[i];

            if (data.m_hostID == sendHostID)
            {
                data.m_hostID = -1;
                break;
            }
        }

        m_currentPlayerCount.text = m_curPlayer + " / " + m_playerCount;
    }
    #endregion


    /// 기본 기능 메소드들
    #region Default

    // 공통 실행
    void DefSetup()
    {
        m_currentPlayerCount.text = m_curPlayer + " / " + m_playerCount;
        m_myName.text = NetworkManager.Instance().USER_NAME;

        int rand = Random.Range(0 , m_playerLobbyList.Count);

        m_myIndex = rand;
        PlayerLobbyData data = m_playerLobbyList[rand];
        data.m_readyEffect.transform.parent.gameObject.SetActive(true);
        data.m_hostID = (int)NetworkManager.Instance().m_hostID;
        data.SetName(NetworkManager.Instance().USER_NAME);
        data.SetReady(false);

        m_redTeam.SetActive(m_teamMode);
        m_blueTeam.SetActive(m_teamMode);
    }

    // 방장일때 기본 셋업
    void DefHostSetup()
    {
        m_gameSetting.SetActive(true);
        // 추후 이건 Dict 에서 가져오게끔 변경해야함
        m_readyLabel.text = "Game Start";
    }
    
    // 방장이 아닐 때 기본 셋업
    void DefPlayerSetup()
    {
        m_gameSetting.SetActive(false);
        // 추후 이건 Dict 에서 가져오게끔 변경해야함
        m_readyLabel.text = "Ready";

        //준비 완료를 알림
        NetworkManager.Instance().RequestLobbyConnect();
    }

    // 상관없이 실행되는 UI
    //팀전
    void TeamModeSetup()
    {
        // 오른쪽 팀 리스트를 보여준다.
        m_rightTeamMenu.SetActive(true);
        m_redTeam.SetActive(m_teamMode);
        m_blueTeam.SetActive(m_teamMode);

        for(int i = 0; i < m_playerLobbyList.Count; i++)
        {
            PlayerLobbyData data = m_playerLobbyList[i];
            if(data.m_hostID != -1)
                data.SetTeamColor(true);
        }

        for(int i = 0; i < m_teamList.Count; i++)
        {
            TeamRightData data = m_teamList[i];

            data.m_teamBG.gameObject.SetActive(data.m_hostID != -1);
            data.SetTeamColor(true);
            
        }
    }

    //개인전
    void IndividualModeSetup()
    {
        // 오른쪽 팀 리스트 메뉴를 없앤다.
        m_rightTeamMenu.SetActive(false);
        m_redTeam.SetActive(m_teamMode);
        m_blueTeam.SetActive(m_teamMode);
        m_playerLobbyList[m_myIndex].SetTeamColor(false);

    }
    #endregion


    // 버튼
    #region Button Method

    // 방장 - 팝업
    public void GameSettingButton()
    {
        m_popup.SetActive(!m_popup.activeSelf);

        // 모드 세팅
        TeamModeSetting((m_teamMode) ? "Team" : "Individual");

        // 인원수 세팅
        m_currentPlayerCount.text = m_curPlayer + " / " + m_playerCount;

        // 현재 맵 세팅

        // 게임 모드 세팅
    }

    // 레디 or 스타트
    public void GameStartReadyButton()
    {
        if(NetworkManager.Instance().IS_HOST)
        {
            Debug.Log(" << Game Start >> ");
            NetworkManager.Instance().RequestNetworkGameStart();
        }
        else
        {
            m_isReady = !m_isReady;
            m_playerLobbyList[m_myIndex].m_readyEffect.SetActive(m_isReady);
            NetworkManager.Instance().RequestNetworkGameReady(m_isReady);
        }
    }

    // 팀선택
    public void TeamSelect(GameObject obj)
    {
        bool teamRed = obj.name.Equals("Team_RED");
        Color c = new Color();

        c.r = (teamRed) ? 200.0f / 255.0f : 80.0f / 255.0f;
        c.g = 80.0f / 255.0f;
        c.b = (teamRed) ? 80.0f / 255.0f : 200.0f / 255.0f;
        c.a = 204.0f / 255.0f;
        m_myTeamBG.color = c;

        m_playerLobbyList[m_myIndex].SetTeamColor(true,teamRed);

        NetworkManager.Instance().RequestNetworkGameTeamSelect(teamRed);

    }

    // 돌아가기
    public void BackToLobby()
    {
        if (NetworkManager.Instance().IS_HOST)
            NetworkManager.Instance().RequestNetworkHostOut();
        SceneManager.LoadScene(0);
    }
    #endregion

    #region GameSetting Button Method
    // 맵 변경
    public void MapSelectButton(GameObject arrow)
    {
        Debug.Log("MapSelectButton " + arrow.name);
        if(arrow.name.Equals("LeftArrow"))
        {

        }
        else
        {

        }
    }

    // 모드 변경
    public void GameModeChangeButton(GameObject arrow)
    {
        Debug.Log("GameModeChangeButton " + arrow.name);
        if (arrow.name.Equals("LeftArrow"))
        {
            if (m_gameMode == (int)GameManager.GameMode.DEATH_MATCH)
                m_gameMode = (int)GameManager.GameMode.SURVIVAL;
            else
                m_gameMode = (int)GameManager.GameMode.DEATH_MATCH;
        }  
        else
        {
            if (m_gameMode == (int)GameManager.GameMode.DEATH_MATCH)
                m_gameMode = (int)GameManager.GameMode.SURVIVAL;
            else
                m_gameMode = (int)GameManager.GameMode.DEATH_MATCH;
        }
        
        switch((GameManager.GameMode) m_gameMode)
        {
            case GameManager.GameMode.DEATH_MATCH:
                m_gameModeText.text = "DEATH MATCH"; break;
            case GameManager.GameMode.SURVIVAL:
                m_gameModeText.text = "SURVIVAL"; break;
        }
        NetworkManager.Instance().RequestNetworkGameModeChange(m_gameMode , m_teamMode);
    }

    // 인원 수 변경
    public void GamePlayerCountChangeButton(GameObject arrow)
    {
        Debug.Log("GamePlayerCountChangeButton " + arrow.name);
        if (arrow.name.Equals("LeftArrow"))
        {
            if (m_playerCount - 1 >= m_curPlayer)
            {
                m_playerCount--;
                NetworkManager.Instance().RequestNetworkPlayerCount(m_playerCount);
                ErrorLog("" , false);
            }
            else
                ErrorLog("현재 접속 인원보다 적게 설정할 수 없습니다.");
        }
        else
        {
            if (m_playerCount +1 <= 10)
            {
                m_playerCount++;
                NetworkManager.Instance().RequestNetworkPlayerCount(m_playerCount);
                ErrorLog("" , false);
            }
            else
                ErrorLog("최대 인원은 10명입니다.");
        }
        m_gameSettingPlayerCount.text = m_playerCount.ToString();
        m_currentPlayerCount.text = m_curPlayer + " / " + m_playerCount;
    }

    // 팀 모드 세팅
    public void TeamModeChangeButton(GameObject obj)
    {
        Debug.Log("TeamModeChangeButton " + obj.name);
        TeamModeSetting(obj.name);
      
        NetworkManager.Instance().RequestNetworkGameModeChange(m_gameMode , m_teamMode);

        if (m_teamMode)
            TeamModeSetup();
        else
            IndividualModeSetup();
    }

    void TeamModeSetting(string name)
    {
        Color select = new Color();
        select.r = select.g = select.b = 180.0f / 255.0f;
        select.a = 204.0f/255.0f;

        Color origin = new Color();
        origin.r = origin.g = origin.b = 28.0f / 255.0f;
        origin.a = 204.0f / 255.0f;

        bool team = false;

        if (name.Equals("Individual"))
        {
            m_individualSpr.color = select;
            m_teamSpr.color = origin;
        }
        else
        {
            m_individualSpr.color = origin;
            m_teamSpr.color = select;
            team = true;
        }

        m_teamMode = team;
    }

    // 게임 세팅 확인 취소 버튼
    public void GameSettingPopupButton(GameObject obj)
    {
        if (obj.name.Equals("Ok"))
        {
            
        }
        else
        {

        }
        m_popup.SetActive(false);
    }

    #endregion

    #region ETC
    void ErrorLog(string message,bool show = true)
    {
        m_errorLog.transform.parent.gameObject.SetActive(show);
        m_errorLog.text = message;
    }
    #endregion
}
