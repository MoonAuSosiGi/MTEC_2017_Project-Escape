using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class ResultUI : MonoBehaviour {

    #region ResultUI_INFO

    public UILabel m_gameResult = null;
    public UILabel m_gameMode = null;

    #region Profile
    public UILabel m_userName = null;
    public UILabel m_playTime = null;
    public UILabel m_killCount = null;
    public UILabel m_assistCount = null;
    public UILabel m_deathCount = null;
    public UILabel m_money = null;
    #endregion

    [System.Serializable]
    public class UserInfo
    {
        public GameObject m_UserObj = null;
        public UILabel m_userName = null;
        public UISprite m_stateLogo = null;
        public UISprite m_teamLogo = null;
    }

    public List<UserInfo> m_userInfoList = new List<UserInfo>();


    private enum PlayerState
    {
        ALIVE = 0,
        DEATH,
        SPACESHIP
    }
    #region NetworkResult 
    private string m_resultMode = null;
    private int m_resultWinState = 0;
    private int m_resultPlayTime = 0;
    private int m_resultKills = 0;
    private int m_resultAssists = 0;
    private int m_resultDeath = 0;
    private int m_resultMoney = 0;

    private bool m_resultUIAlready = false;
    public bool RESULT_UI_ALREADY { get { return m_resultUIAlready; } set { m_resultUIAlready = value; } }

    public class UserInformation
    {
        public string m_name = null;
        public int m_state = 0;
        public int m_team = 0;

        public UserInformation(string name , int state , int team)
        {
            m_name = name;
            m_state = state;
            m_team = team;
        }
    }
    public List<UserInformation> m_infoList = new List<UserInformation>();
    #endregion

    #endregion

    #region Setting Information
    public void SetProfileInfo(string gameMode,int winState,int playTime,int killCount,int assistCount,int deathCount,int money)
    {
        m_gameResult.text = (winState == 1) ? "VICTORY" : "LOSE";
        m_gameMode.text = "TEST MODE";

        m_userName.text = NetworkManager.Instance().USER_NAME;

        // 플레이 타임
        // int time = playTime / 1000;
        var time = TimeSpan.FromMilliseconds(playTime);

        string hours = (time.Hours > 0) ? time.Hours + ":" : "";
        m_playTime.text = hours + time.Minutes.ToString("00") + ":" + time.Seconds; // 임시
        m_killCount.text = killCount.ToString();
        m_assistCount.text = assistCount.ToString();
        m_deathCount.text = deathCount.ToString();
        m_money.text = money.ToString();
    }

    public void AddResultOtherProfileInfo(string userName , int state , int team = 0)
    {
        m_infoList.Add(new UserInformation(userName , state , team));
    }

    // 실 데이터 적용
    public void SettingEnd()
    {

        for (int i = 0; i < m_infoList.Count; i++)
        {
            UserInformation user = m_infoList[i];
            UserInfo info = m_userInfoList[i];

            info.m_UserObj.SetActive(true);

            info.m_userName.text = user.m_name;

            string stateSprName = "";
            switch((PlayerState)user.m_state)
            {
                case PlayerState.ALIVE:         stateSprName = "Icon_Play"; break;
                case PlayerState.DEATH:         stateSprName = "Icon_Dead"; break;
                case PlayerState.SPACESHIP:     stateSprName = "Icon_survival"; break;
            }

            info.m_stateLogo.spriteName = stateSprName;

            // 임시
            info.m_teamLogo.gameObject.SetActive(false);
        }

        for(int i = m_infoList.Count; i < m_userInfoList.Count; i++)
        {
            m_userInfoList[i].m_UserObj.SetActive(false);
        }
    }
    #endregion


    #region Button Method
    public void DetailButton()
    {
        Debug.Log("DetailButton ");
    }

    public void BackToMainLobbyButton()
    {
        NetworkManager.Instance().gameResultInfoToMe -= ResultUI_gameResultInfoToMe;
        NetworkManager.Instance().gameResultInfoToOther -= ResultUI_gameResultInfoToOther;
        NetworkManager.Instance().gameResultShow -= ResultUI_gameResultShow;
        NetworkManager.Instance().RequestGameExit();
        GameObject.Destroy(NetworkManager.Instance().gameObject);
        SceneManager.LoadScene(0);
    }
    #endregion


    #region UnityMethod
    void Start()
    {
        GameManager.Instance().m_resultUI = this;
        // 내 정보가 넘어왔다.
        NetworkManager.Instance().gameResultInfoToMe += ResultUI_gameResultInfoToMe;
        // 다른 사람 정보가 넘어왔다.
        NetworkManager.Instance().gameResultInfoToOther += ResultUI_gameResultInfoToOther;

        // 이제 결과를 띄우셔도 좋습니다.
        NetworkManager.Instance().gameResultShow += ResultUI_gameResultShow;
        gameObject.SetActive(false);
    }

    private void ResultUI_gameResultShow()
    {
        SettingEnd();

        InvokeRepeating("ResultCheck" , Time.deltaTime , Time.deltaTime);
    }

    private void ResultUI_gameResultInfoToOther(string name , int state)
    {
        AddResultOtherProfileInfo(name , state);
    }

    private void ResultUI_gameResultInfoToMe(string gameMode , int winState , int playTime , int kills , int assists , int death , int getMoney)
    {
        SetProfileInfo(gameMode , winState , playTime , kills , assists , death , getMoney);
    }

    void ResultCheck()
    {
        if(RESULT_UI_ALREADY)
        {
            CancelInvoke("ResultCheck");
            gameObject.SetActive(true);
        }
    }

    #endregion
}
