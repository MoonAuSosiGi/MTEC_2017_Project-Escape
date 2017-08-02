using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
   
    #endregion

    #region Setting Information
    public void SetProfileInfo(string gameMode,int winState,int playTime,int killCount,int assistCount,int deathCount,int money)
    {
        m_gameResult.text = (winState == 1) ? "VICTORY" : "LOSE";
        m_gameMode.text = "TEST MODE";

        m_userName.text = GameManager.Instance().PLAYER.m_name;
        m_playTime.text = playTime.ToString(); // 임시
        m_killCount.text = killCount.ToString();
        m_assistCount.text = assistCount.ToString();
        m_deathCount.text = deathCount.ToString();
        m_money.text = money.ToString();
    }


    // 실 데이터 적용
    public void SettingEnd()
    {
        List<GameManager.UserInformation> infoList = GameManager.Instance().m_infoList;

        for (int i = 0; i < infoList.Count; i++)
        {
            GameManager.UserInformation user = infoList[i];
            UserInfo info = m_userInfoList[i];

            info.m_UserObj.SetActive(true);

            info.m_userName.text = user.m_name;

            string stateSprName = "";
            switch((PlayerState)user.m_state)
            {
                case PlayerState.ALIVE:         stateSprName = "icon_Play"; break;
                case PlayerState.DEATH:         stateSprName = "icon_Dead"; break;
                case PlayerState.SPACESHIP:     stateSprName = "icon_survival"; break;
            }

            info.m_stateLogo.spriteName = stateSprName;

            // 임시
            info.m_teamLogo.gameObject.SetActive(false);
        }

        for(int i = infoList.Count; i < m_userInfoList.Count; i++)
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
        Debug.Log("BackToMainLobbyButton ");
    }
    #endregion


    #region UnityMethod
    void Awake()
    {
        GameManager.Instance().m_resultUI = this;
        gameObject.SetActive(false);
    }
    #endregion
}
